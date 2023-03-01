using System;
using UnityEngine;
using CodeNifty.DraftYourViewers2.TwitchUtil;

namespace CodeNifty.DraftYourViewers2
{

    // Backend engine for DYV 2. Handles config, user authorization and authentication, and drafting.

    // The flow is as follows:
    // 1. This Unity Component is loaded as part of a Game Object imported and instantiated by a Loader Interface (which allows support for multiple loaders).
    // 2. When told to by the Draft App (the controlling UI), it attempts to load config data, or creates new data if nothing is found (which causes the App to prompt the user for authorization).
    // 3. If the loaded config contains a Twitch Access Token, the Draft Manager attempts to authenticate it with Twitch to ensure the token is still valid, and then calls back to the Draft App.
    // 4. From this point, the Draft App's user actions control the flow. When the streamer drafts a viewer, a chat-fetch call is made and a new Kerbal is created based on a random chatter's name.

    // If the user needs to authorize DTV,
    // 1. The DraftManager starts a temporary local HTTP server to receive the Twitch Access Token later in the process.
    // 2. It then warns the user they'll be taken to Twitch via browser to authorize DTV before doing so.
    // 3. Once the user authorizes DTV, Twitch will redirect to the local server which can then save and use the Access Token.
    // 4. The temporary HTTP server is shut down.
    public class DraftManager : MonoBehaviour
    {
        public DraftConfig Config;
        public DraftSave Save;

        private string streamerUserId;

        private string campaignPath;
        Predicate<string> kerbalExistsTest;

        public AuthServerManager authServerManager;
        public IDraftActor draftActor;

        public Action<string> AppOnStreamerAuthenticated;
        public Action<bool> AppOnCampaignLoaded;

        public void SetUp()
        {
            Config = FileManager.LoadDraftConfig();
            AuthenticateStreamer();
        }

        public void ClearSave()
        {
            Save = FileManager.NewDraftSave(campaignPath, true);
            Chatter.ClearCache();
        }

        public void ClearConfig()
        {
            Config = FileManager.NewDraftConfig(true);
            streamerUserId = "";
        }

        public void OnCampaignLoaded(string campaignPath, Predicate<string> kerbalExistsTest)
        {
            Logger.LogInfo($"HECC, a campaign with the path \"{campaignPath}\" loaded!");
            this.campaignPath = campaignPath;
            Save = FileManager.LoadDraftSave(campaignPath);
            this.kerbalExistsTest = kerbalExistsTest;
            AppOnCampaignLoaded.Invoke(true);
        }

        public void OnCampaignUnloaded()
        {
            Logger.LogInfo($"Aw beans, the campaign unloaded...");
            AppOnCampaignLoaded.Invoke(false);
        }

        public void DraftViewerIntoActiveCraft(Action<string> onDraftComplete, Action<string> onDraftFailed)
        {
            if (!draftActor.CanAddViewerToActiveCraft())
            {
                onDraftFailed.Invoke("Can't draft into the active craft.\n\nIs there space available?");
                return;
            }
            GetRandomViewer(
                new Action<Chatter>((Chatter viewer) =>
                {
                    string kerbalName = $"{viewer.DisplayName}{(Config.addKerman ? " Kerman" : "")}";
                    string kerbalId = draftActor.AddViewerToActiveCraft(kerbalName);
                    Save.draftedUserIds.Remove(viewer.ID);
                    Save.draftedUserIds.Add(viewer.ID, kerbalId);
                    FileManager.SaveDraftSave(campaignPath, Save);
                    onDraftComplete.Invoke(kerbalName);
                }),
                onDraftFailed,
                (Chatter viewer) =>
                {
                    return !Config.blockedUsernames.Contains(viewer.LoginName) &&
                        !Config.blockedUsernames.Contains(viewer.DisplayName) &&
                        (
                            !Save.draftedUserIds.ContainsKey(viewer.ID) ||
                            !kerbalExistsTest(Save.draftedUserIds[viewer.ID])
                        );
                }
            );
        }

        public void DraftViewerIntoRoster(Action<string> onDraftComplete, Action<string> onDraftFailed)
        {
            if (!draftActor.CanHireViewerToRoster())
            {
                onDraftFailed.Invoke("Can't hire to the roster.\n\nDo you have the funds? Any openings available?");
                return;
            }
            GetRandomViewer(
                new Action<Chatter>((Chatter viewer) =>
                {
                    string kerbalName = $"{viewer.DisplayName}{(Config.addKerman ? " Kerman" : "")}";
                    string kerbalId = draftActor.HireViewerToRoster(kerbalName);
                    Save.draftedUserIds.Remove(viewer.ID);
                    Save.draftedUserIds.Add(viewer.ID, kerbalId);
                    FileManager.SaveDraftSave(campaignPath, Save);
                    onDraftComplete.Invoke(kerbalName);
                }),
                onDraftFailed,
                (Chatter viewer) =>
                {
                    return !Config.blockedUsernames.Contains(viewer.LoginName) &&
                        !Config.blockedUsernames.Contains(viewer.DisplayName) &&
                        (
                            !Save.draftedUserIds.ContainsKey(viewer.ID) ||
                            !kerbalExistsTest(Save.draftedUserIds[viewer.ID])
                        );
                }
            );
        }

        public void DrawRandomViewer(Action<string> onDrawingComplete, Action<string> onDrawingFailed)
        {
            GetRandomViewer(
                new Action<Chatter>((Chatter viewer) =>
                {
                    Save.drawnUserIds.Add(viewer.ID);
                    FileManager.SaveDraftSave(campaignPath, Save);
                    onDrawingComplete.Invoke(viewer.DisplayName);
                }),
                onDrawingFailed,
                (Chatter viewer) =>
                {
                    if (Config.blockedUsernames.Contains(viewer.LoginName))
                    {
                        return false;
                    }
                    if (Config.includeDraftedInDrawings && Save.draftedUserIds.ContainsKey(viewer.ID))
                    {
                        return false;
                    }
                    return (!Config.includeDrawnInDrawings || !Save.drawnUserIds.Contains(viewer.ID));
                }
            );
        }

        public void GetRandomViewer(Action<Chatter> onViewerDrafted, Action<string> onError, Predicate<Chatter> canUseViewer = null)
        {
            if (streamerUserId == "")
            {
                return;
            }

            // Web requests are an async process, and coroutines don't like giving back data, so Action callbacks must be used.
            StartCoroutine(
                Chatter.GetAllChatters(
                    streamerUserId,
                    Config.streamerAccessToken,
                    new Action<PaginatedArray<Chatter>>((PaginatedArray<Chatter> chatters) =>
                    {
                        Logger.LogInfo($"{chatters.Data.Count} pulled.");

                        if (chatters.Data.Count == 0)
                        {
                            Logger.LogWarn($"Failed to pick viewer. Empty chat.");
                            onError.Invoke("Something went wrong!\n\nLooks like no one is in your chat.");
                            return;
                        }

                        bool isValidViewer = false;
                        int randomIndex, tries = 0;
                        do
                        {
                            randomIndex = UnityEngine.Random.Range(0, chatters.Data.Count);
                            tries++;

                            isValidViewer = canUseViewer == null || canUseViewer(chatters.Data[randomIndex]);
                            if (!isValidViewer)
                            {
                                chatters.Data.RemoveAt(randomIndex);
                            }
                        } while (!isValidViewer && tries < 25 && chatters.Data.Count > 0);
                        
                        if (!isValidViewer)
                        {
                            if (tries == 25)
                            {
                                Logger.LogWarn($"Failed to pick viewer. 25 random attempts failed validity check.");
                                onError.Invoke("Something went wrong!\n\nCouldn't pick a viewer after 25 attempts.");
                            }
                            else
                            {
                                Logger.LogWarn($"Failed to pick viewer. Ran out of viewers.");
                                onError.Invoke("Something went wrong!\n\nRan out of viewers not already in use. Did you draft them all?");
                            }
                            return;
                        }

                        onViewerDrafted.Invoke(chatters.Data[randomIndex]);
                    }), new Action<Error>((Error err) =>
                    {
                        Logger.LogError($"Failed to pick viewer with status {err.Status}: {err.ErrorText} - {err.Message}");
                        onError.Invoke("Something went wrong!\n\nCouldn't get your chatters from Twitch.");
                    })
                )
            );
        }

        public void AuthorizeStreamer()
        {
            string csrfPreventionToken = Guid.NewGuid().ToString();
            authServerManager.StartAuthRedirectServer(csrfPreventionToken, new Action<string>((string accessToken) =>
            {
                Config.streamerAccessToken = accessToken;
                FileManager.SaveDraftConfig(Config);
                AuthenticateStreamer();
            }));
            Application.OpenURL($"https://id.twitch.tv/oauth2/authorize?client_id={ClientId.CLIENT_ID}&redirect_uri=http://localhost:2550/authorize&response_type=token&scope=moderator:read:chatters&state={csrfPreventionToken}");
        }

        private void AuthenticateStreamer()
        {
            if (Config.streamerAccessToken == "")
            {
                return;
            }

            // Web requests are an async process, and coroutines don't like giving back data, so Action callbacks must be used.
            StartCoroutine(
                User.GetFirstUser(
                    Config.streamerAccessToken,
                    User.QueryBy.AccessToken,
                    new Action<User>((User user) =>
                    {
                        Logger.LogInfo($"User {user.DisplayName} authenticated.");
                        streamerUserId = user.Id;
                        AppOnStreamerAuthenticated.Invoke(user.DisplayName);
                    }), new Action<Error>((Error err) =>
                    {
                        Logger.LogError($"Failed to authenticate with status {err.Status}: {err.ErrorText} - {err.Message}\nClearing config user data.");
                        Config.streamerAccessToken = "";
                        FileManager.SaveDraftConfig(Config);
                    })
                )
            );
        }
    }
}
