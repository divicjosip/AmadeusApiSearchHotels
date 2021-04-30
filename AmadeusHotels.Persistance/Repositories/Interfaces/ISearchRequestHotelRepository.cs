using AmadeusHotels.Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmadeusHotels.Persistance.Repositories.Interfaces
{
    public interface ISearchRequestHotelRepository
    {
        Task<List<SearchRequestHotel>> GetForCurrentPageIncludedAsync(int searchRequestId, int pageSize, int pageOffset);
        Task AddAsync(SearchRequestHotel searchRequesHotel);
        void Update(SearchRequestHotel searchRequestHotel);
    }
}
