using System.Collections.Generic;
using Newtonsoft.Json;

namespace CodeNifty.DraftYourViewers2.TwitchUtil
{
    [JsonObject]
    public struct PaginationData
    {
        [JsonProperty(PropertyName = "cursor")]
        public string Cursor;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PaginatedArray<T>
    {
        [JsonProperty(PropertyName = "data")]
        public List<T> Data;

        [JsonProperty(PropertyName = "pagination")]
        public PaginationData Pagination;

        [JsonProperty(PropertyName = "total")]
        public int Total;

        public PaginatedArray()
        {
            Data = new List<T>();
        }

        public void AddPage(PaginatedArray<T> page)
        {
            Data.AddRange(page.Data);
            Pagination.Cursor = page.Pagination.Cursor;
            Total = page.Total;
        }
    }
}
