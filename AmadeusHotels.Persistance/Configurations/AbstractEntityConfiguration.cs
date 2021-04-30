using System;
using System.Collections.Generic;
using System.Text;

namespace AmadeusHotels.Persistance.Configurations
{
    internal abstract class AbstractEntityConfiguration<TEntity> : AbstractConfiguration<TEntity> where TEntity : class
    {
    }
}
