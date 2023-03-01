using Newtonsoft.Json;

namespace CodeNifty.DraftYourViewers2.TwitchUtil
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Error
    {
        [JsonProperty(PropertyName = "error")]
        public string ErrorText { get; private set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; private set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; private set; }
    }
}
