using System;
using System.Collections.Generic;
using System.Text;

namespace AmadeusHotels.Bll.Models
{
    public class HotelsSearchResponse
    {
        public List<HotelSearchItemResponse> Items { get; set; }

        public int CurrentPageSize { get; set; }
        public int CurrentPageOffset { get; set; }
        public bool HasNextPage { get; set; }

        public HotelsSearchResponse(HotelsSearchUserRequest hotelsSearchRequest)
        {
            this.Items = new List<HotelSearchItemResponse>();
            CurrentPageSize = hotelsSearchRequest.PageSize;
            CurrentPageOffset = hotelsSearchRequest.PageOffset;
        }
    }
}
