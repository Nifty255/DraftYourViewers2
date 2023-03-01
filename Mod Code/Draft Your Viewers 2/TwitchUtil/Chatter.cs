using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace CodeNifty.DraftYourViewers2.TwitchUtil
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Chatter
    {
        public static Dictionary<string, Cacheable<PaginatedArray<Chatter>>> ChattersCache;

        [JsonProperty(PropertyName = "user_id")]
        public string ID { get; private set; }

        [JsonProperty(PropertyName = "user_login")]
        public string LoginName { get; private set; }

        [JsonProperty(PropertyName = "user_name")]
        public string DisplayName { get; private set; }

        private static void EnsureCache()
        {
            if (ChattersCache == null)
            {
                ChattersCache = new Dictionary<string, Cacheable<PaginatedArray<Chatter>>>();
            }
        }

        public static void ClearCache()
        {
            ChattersCache = new Dictionary<string, Cacheable<PaginatedArray<Chatter>>>();
        }

        public static IEnumerator GetAllChatters(string userId, string accessToken, Action<PaginatedArray<Chatter>> onSuccess, Action<Error> onFailure)
        {
            yield return GetAllChatters(userId, accessToken, 60, onSuccess, onFailure);
        }

        public static IEnumerator GetAllChatters(string userId, string accessToken, double cacheTime, Action<PaginatedArray<Chatter>> onSuccess, Action<Error> onFailure)
        {
            EnsureCache();

            Cacheable<PaginatedArray<Chatter>> cachedChatters;
            if (ChattersCache.TryGetValue(userId, out cachedChatters))
            {
                if (DateTime.UtcNow < cachedChatters.CachedAt.AddSeconds(cachedChatters.CacheSeconds))
                {
                    Logger.LogInfo("Using cache.");
                    onSuccess.Invoke(cachedChatters.Data);
                    yield break;
                }
                ChattersCache.Remove(userId);
            }

            // Make a main array which starts out with a zero total until the first request. As each request completes, its page is merged into this main array.
            // Technically, chatter data can move around, grow, or shrink on the Twitch side throughout this process, possibly even resulting in duplicate
            // chatters appearing in the list. But I've already overengineered the hecc out of this, and this is just a mod, so let's not worry about that.
            PaginatedArray<Chatter> chatters = new PaginatedArray<Chatter>();
            string url = $"https://api.twitch.tv/helix/chat/chatters?broadcaster_id={userId}&moderator_id={userId}&first=1000";
            do
            {
                string pageUrl = url;
                if (chatters.Pagination.Cursor != "")
                {
                    pageUrl += $"&after={chatters.Pagination.Cursor}";
                }
                UnityWebRequest request = new UnityWebRequest(pageUrl);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
                request.SetRequestHeader("Client-Id", ClientId.CLIENT_ID);
                yield return request.SendWebRequest();

                if (request.responseCode != 200)
                {
                    onFailure.Invoke(JsonConvert.DeserializeObject<Error>(request.downloadHandler.text));
                    yield break;
                }
                PaginatedArray<Chatter> page = JsonConvert.DeserializeObject<PaginatedArray<Chatter>>(request.downloadHandler.text);
                chatters.AddPage(page);
            } while (chatters.Data.Count <= chatters.Total && chatters.Pagination.Cursor != null && chatters.Pagination.Cursor != "");

            // Because whatever total Twitch gave last might not reflect the final list, set the total to the chatter list size.
            chatters.Total = chatters.Data.Count;

            Cacheable<PaginatedArray<Chatter>> cache = new Cacheable<PaginatedArray<Chatter>>(chatters, cacheTime);
            ChattersCache.Add(userId, cache);

            onSuccess.Invoke(chatters);
        }
    }
}
