using System;
using System.Collections.Generic;
using System.Text;

namespace AmadeusHotels.Bll.Models
{
    public class HotelSearchItemResponse
    {
        public string Name { get; set; }

        public int Rating { get; set; }

        public string Description { get; set; }

        public bool Available { get; set; }

        public float? PriceTotal { get; set; }

        public string PriceCurrency { get; set; }

        public float? Distance { get; set; }
    }
}
