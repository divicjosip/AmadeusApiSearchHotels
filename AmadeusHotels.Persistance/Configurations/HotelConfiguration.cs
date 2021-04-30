using AmadeusHotels.Entities.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmadeusHotels.Persistance.Configurations
{
    internal class HotelConfiguration : AbstractEntityConfiguration<Hotel>
    {
        public override void Configure(EntityTypeBuilder<Hotel> builder)
        {
            base.Configure(builder);

            builder.HasKey(x => x.HotelId);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(250);

        }
    }
}
