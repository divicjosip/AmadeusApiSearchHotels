using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AmadeusHotels.Bll.Services.Interfaces
{
    public interface IAmadeusTokenService
    {
        Task<string> getAmadeusToken(CancellationToken cancellationToken);
    }
}
