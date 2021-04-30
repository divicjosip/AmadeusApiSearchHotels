using AmadeusHotels.Entities.Models;
using AmadeusHotels.Persistance.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmadeusHotels.Persistance.Repositories
{
    public class SearchRequestHotelRepository : BaseRepository, ISearchRequestHotelRepository
    {
        public SearchRequestHotelRepository(AppDbContext context) : base(context) { }

        public async Task<List<SearchRequestHotel>> GetForCurrentPageIncludedAsync(int searchRequestId, int pageSize, int pageOffset)
        {
            int skip = pageOffset == 0 ? 0 : pageOffset * pageSize;
            return await _context.SearchRequestHotels
                                 .Where(x => x.SearchRequestId == searchRequestId)
                                 .Include(x => x.Hotel)
                                 .OrderBy(x => x.Distance)
                                 .Skip(skip)
                                 .Take(pageSize)
                                 .ToListAsync();

        }


        public async Task AddAsync(SearchRequestHotel searchRequestHotel)
        {
            await _context.SearchRequestHotels.AddAsync(searchRequestHotel);
        }

        public void Update(SearchRequestHotel searchRequestHotel)
        {
            _context.SearchRequestHotels.Update(searchRequestHotel);
        }


    }
}
