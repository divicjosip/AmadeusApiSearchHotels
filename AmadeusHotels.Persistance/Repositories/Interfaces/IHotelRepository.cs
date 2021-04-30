using AmadeusHotels.Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmadeusHotels.Persistance.Repositories.Interfaces
{
    public interface IHotelRepository
    {
        Task InsertOrUpdate(Hotel hotel);
    }
}
