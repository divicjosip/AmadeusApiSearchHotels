using System;
using System.Collections.Generic;
using System.Text;

namespace AmadeusHotels.Bll.Config
{
    public class AmadeusClientOptions
    {
        public string Url { get; set; }

        public string AuthTokenUrl { get; set; }

        public string ApiKey { get; set; }

        public string ApiSecret { get; set; }
    }
}
