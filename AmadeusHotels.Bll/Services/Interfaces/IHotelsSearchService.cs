using AmadeusHotels.Bll.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AmadeusHotels.Bll.Services.Interfaces
{
    public interface IHotelsSearchService
    {
        Task<HotelsSearchResponse> SearchHotels(HotelsSearchUserRequest hotelsSearchRequest, CancellationToken cancellationToken);
    }
}
