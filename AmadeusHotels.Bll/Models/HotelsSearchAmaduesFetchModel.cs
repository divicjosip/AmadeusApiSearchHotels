using AmadeusHotels.Bll.Models.AmadeusApiCustomModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmadeusHotels.Bll.Models
{
    public class HotelsSearchAmaduesFetchModel
    {
        public List<AmadeusApiHotelsSearchResponseItem> Items { get; set; }

        public string nextItemsUrl { get; set; }
    }
}
