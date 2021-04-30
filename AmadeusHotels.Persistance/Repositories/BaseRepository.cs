using System;
using System.Collections.Generic;
using System.Text;

namespace AmadeusHotels.Persistance.Repositories
{
    public abstract class BaseRepository
    {
        protected readonly AppDbContext _context;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
        }
    }
}
