using System.Collections.Generic;
using Newtonsoft.Json;

namespace CodeNifty.DraftYourViewers2.TwitchUtil
{
    public class UnpaginatedArray<T>
    {
        [JsonProperty(PropertyName = "data")]
        public List<T> Data;

        public UnpaginatedArray()
        {
            Data = new List<T>();
        }
    }
}
