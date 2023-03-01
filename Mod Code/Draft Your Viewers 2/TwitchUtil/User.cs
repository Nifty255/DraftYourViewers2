using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace CodeNifty.DraftYourViewers2.TwitchUtil
{
    [JsonObject(MemberSerialization.OptIn)]
    struct User
    {
        public enum QueryBy
        {
            Id,
            LoginName,
            AccessToken
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; private set; }

        [JsonProperty(PropertyName = "login")]
        public string LoginName { get; private set; }

        [JsonProperty(PropertyName = "display_name")]
        public string DisplayName { get; private set; }

        public static IEnumerator GetFirstUser(string query, QueryBy queryBy, Action<User> onSuccess, Action<Error> onFailure)
        {
            string url = "https://api.twitch.tv/helix/users";
            UnityWebRequest request = new UnityWebRequest();

            switch (queryBy)
            {
                case QueryBy.Id:
                    url += $"?id={query}";
                    break;
                case QueryBy.LoginName:
                    url += $"?login={query}";
                    break;
                case QueryBy.AccessToken:
                    request.SetRequestHeader("Authorization", $"Bearer {query}");
                    break;
            }
            request.SetRequestHeader("Client-Id", ClientId.CLIENT_ID);
            request.url = url;
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.responseCode != 200)
            {
                onFailure.Invoke(JsonConvert.DeserializeObject<Error>(request.downloadHandler.text));
            }

            UnpaginatedArray<User> users = JsonConvert.DeserializeObject<UnpaginatedArray<User>>(request.downloadHandler.text);
            onSuccess.Invoke(users.Data[0]);
        }
    }
}
