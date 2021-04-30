using System;
using System.Collections.Generic;
using System.Text;

namespace AmadeusHotels.Entities.Models
{
    public class Hotel
    {
        public string HotelId { get; set; }

        public string Name { get; set; }

        public int Rating { get; set; }

        public string Description { get; set; }

        public ICollection<SearchRequestHotel> SearchRequestHotels { get; set; }
    }
}
