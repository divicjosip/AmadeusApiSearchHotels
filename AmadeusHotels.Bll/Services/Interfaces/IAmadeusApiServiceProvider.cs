using AmadeusHotels.Bll.Models;
using AmadeusHotels.Bll.Models.AmadeusApiCustomModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AmadeusHotels.Bll.Services.Interfaces
{
    public interface IAmadeusApiServiceProvider
    {
        Task<HotelsSearchAmaduesFetchModel> FetchAmadeusHotels(HotelsSearchUserRequest hotelsSearchRequest,CancellationToken cancellationToken);
        Task<HotelsSearchAmaduesFetchModel> FetchNextAmadeusHotelsRecursively(string uri, int itemsToFetch, CancellationToken cancellationToken);
    }
}
