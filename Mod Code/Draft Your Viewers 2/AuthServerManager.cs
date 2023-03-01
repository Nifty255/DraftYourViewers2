﻿using System;
using System.Collections;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

namespace CodeNifty.DraftYourViewers2
{
    // Twitch authorization server for DYV 2. Why is this required? Because getting a list of users in a chat apparently requires the streamer's
    // access token, and I'm not paying for a proper server to do this properly for one tiny little OAuth2 flow that 12 people will use.

    // So what's this do? When the streamer needs to authorize DYV2, this starts a local server and waits until the server gets Twitch's
    // redirect or until the user cancels. If the redirect contains a Twitch access token, it passes it back via Actions.
    public class AuthServerManager : MonoBehaviour
    {
        private Thread serverThread;
        private HttpListener serverListener;

        private string csrfPreventionToken;
        private Action<string> authCompleteCallback;
        private volatile string authTokenReceived;

        private void FixedUpdate()
        {
            if (serverThread == null || serverThread.IsAlive) { return; }
            serverThread = null;

            string authToken = authTokenReceived;
            authTokenReceived = "";
            if (authToken == "") { return; }

            if (authCompleteCallback != null)
            {
                Logger.LogInfo($"GOT ACCESS TOKEN \"{authToken}\"");
                StartCoroutine(StopServerLater());
                authCompleteCallback.Invoke(authToken);
            }
        }

        private IEnumerator StopServerLater()
        {
            yield return new WaitForSeconds(1f);
            serverListener.Stop();
        }

        public void StartAuthRedirectServer(string csrfPreventionToken, Action<string> authCompleteCallback)
        {
            this.csrfPreventionToken = csrfPreventionToken;
            this.authCompleteCallback = authCompleteCallback;
            serverListener = new HttpListener();
            serverListener.Prefixes.Add("http://localhost:2550/");
            serverListener.Start();
            serverThread = new Thread(ListenForAuth);
            serverThread.Start();
        }

        public void CancelAuth()
        {
            serverThread.Abort();
            serverListener.Stop();
        }

        private void ListenForAuth()
        {
            while(true)
            {
                HttpListenerContext ctx = serverListener.GetContext();

                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse res = ctx.Response;

                switch(req.Url.AbsolutePath)
                {
                    case "/authorize":
                        if (req.HttpMethod != "GET")
                        {
                            ErrorPage(res, 405, $"Bruh, this is a temporary Twitch auth server for a KSP 2 mod. You can only GET, not {req.HttpMethod} KEKW");
                            break;
                        }
                        SuccessPage(res);
                        break;
                    case "/data":
                        if (req.HttpMethod != "GET")
                        {
                            ErrorResponse(res, 405, $"Bruh, this is a temporary Twitch auth server for a KSP 2 mod. You can only GET, not {req.HttpMethod} KEKW");
                            break;
                        }
                        if (req.QueryString["state"] != csrfPreventionToken)
                        {
                            ErrorResponse(res, 403, $"Walp, the CSRF prevention token didn't match, so I guess someone's trying to attack your auth attempt MonkaS");
                            break;
                        }
                        if (req.QueryString["access_token"] == null)
                        {
                            ErrorResponse(res, 400, $"Auth token? modCheck");
                            break;
                        }
                        authTokenReceived = string.Copy(req.QueryString["access_token"]);
                        SuccessResponse(res);
                        return;
                    default:
                        ErrorPage(res, 404, $"Bruh, this is a temporary Twitch auth server for a KSP 2 mod. \"{req.Url.AbsolutePath}\" doesn't exist KEKW");
                        break;
                }
            }
        }

        private void SuccessPage(HttpListenerResponse res)
        {
            byte[] content = Encoding.UTF8.GetBytes(string.Format(successTemplate, header));
            res.ContentType = "text/html";
            res.ContentEncoding = Encoding.UTF8;
            res.ContentLength64 = content.LongLength;
            res.OutputStream.Write(content, 0, content.Length);
            res.Close();
        }

        private void SuccessResponse(HttpListenerResponse res)
        {
            byte[] content = Encoding.UTF8.GetBytes("ok");
            res.ContentType = "text/plain";
            res.ContentEncoding = Encoding.UTF8;
            res.ContentLength64 = content.LongLength;
            res.OutputStream.Write(content, 0, content.Length);
            res.Close();
        }

        private void ErrorPage(HttpListenerResponse res, int status, string error)
        {
            byte[] content = Encoding.UTF8.GetBytes(string.Format(bodyTemplate, header, status, error));
            res.StatusCode = status;
            res.ContentType = "text/html";
            res.ContentEncoding = Encoding.UTF8;
            res.ContentLength64 = content.LongLength;
            res.OutputStream.Write(content, 0, content.Length);
            res.Close();
        }

        private void ErrorResponse(HttpListenerResponse res, int status, string error)
        {
            byte[] content = Encoding.UTF8.GetBytes($"{status} - {error}");
            res.StatusCode = status;
            res.ContentType = "text/plain";
            res.ContentEncoding = Encoding.UTF8;
            res.ContentLength64 = content.LongLength;
            res.OutputStream.Write(content, 0, content.Length);
            res.Close();
        }

        // HTML stuff aaaaaall the way down here so it doesn't clutter the actual code.
        private const string header = @"<head>
        <title>Draft Your Viewers 2</title>
        <style>
            html,body {
                border: 0;
                background-color: #22262e;
                color: #d6e0ff;
                font-family: Roboto, Helvetica, sans-serif;
            }
            .bordered {
                width: 600px;
                margin: 48px auto;
                background-color: #2e3540;
                border: 4px solid #10182c;
                border-radius: 12px;
            }
            .padded {
                padding: 12px;
            }
            .centered {
                text-align: center;
            }
        </style>
        <script src='https://code.jquery.com/jquery-3.6.3.min.js' integrity='sha256-pvPw+upLPUjgMXY0G+8O0xUf+/Im1MZjXxxgOcBQBXU=' crossorigin='anonymous'></script>
        <script>
            $(document).ready(() => {
                $.ajax({
                    url: `/data?${$(location).attr('hash').substring(1)}`,
                    success: (data) => {
                        $('#header').text('Success!');
                        $('#detail').text('You may now close this page and return to the game.');
                    },
                    error: (xhr) => {
                        $('#header').text(`Well that's not right...`);
                        $('#detail').text(`${xhr.status} - ${xhr.responseText}`);
                    }
                });
            });
        </script>
    </head>";

        private const string successTemplate = @"
<html>
    {0}
    <body>
        <section class='bordered padded centered'>
            <h1 id='header'>One sec...</h1>
        </section>
        <section class='bordered padded centered'>
            <p id='detail'>Authorizing...</p>
        </section>
    </body>
</html>
";

        private const string bodyTemplate = @"
<html>
    {0}
    <body>
        <section class='bordered padded centered'>
            <h1 class='header'>Well that's not right...</h1>
        </section>
        <section class='bordered padded centered'>
            <p>{1} - {2}</p>
        </section>
    </body>
</html>
";
    }
}
