using AmadeusHotels.Entities.Models;
using AmadeusHotels.Persistance.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmadeusHotels.Persistance.Repositories
{
    public class HotelRepository : BaseRepository, IHotelRepository
    {

        public HotelRepository(AppDbContext context) : base(context) { }

        public async Task InsertOrUpdate(Hotel hotel)
        {
            var existingHotel = _context.Hotels.Find(hotel.HotelId);
            if (existingHotel == null)
            {
                await _context.Hotels.AddAsync(hotel);
            }
            else
            {
                _context.Entry(existingHotel).CurrentValues.SetValues(hotel);
            }
        }
    }
}
