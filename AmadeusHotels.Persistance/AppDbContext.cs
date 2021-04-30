using AmadeusHotels.Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmadeusHotels.Persistance
{
    public class AppDbContext : DbContext
    {
        public const string MigrationsHistoryTableName = "__EFMigrationsHistory";

        public AppDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        public DbSet<SearchRequest> SearchRequests { get; set; }
        public DbSet<SearchRequestHotel> SearchRequestHotels { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
    }
}
