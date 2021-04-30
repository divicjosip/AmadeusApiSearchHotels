using System;
using System.Collections.Generic;
using System.Text;

namespace AmadeusHotels.Bll.Models.AmadeusApiCustomModels
{
    public class AmadeusApiErrorResponse
    {
        public List<AmadeusErrorItem> Errors { get; set; }
    }

    public class AmadeusErrorItem
    {
        public int Code { get; set; }

        public string Title { get; set; }

        public string Detail { get; set; }

        public int Status { get; set; }
    }
}
