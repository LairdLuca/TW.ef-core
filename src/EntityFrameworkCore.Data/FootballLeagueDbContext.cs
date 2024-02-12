using EntityFrameworkCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.Data
{
    public class FootballLeagueDbContext : DbContext
    {
        public FootballLeagueDbContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = Path.Combine(path, "FootballLeague_EFCore.db");
        }

        public DbSet<Team> Teams { get; set; }
        public DbSet<Coach> Coaches { get; set; }
        public string DbPath { get; private set; }

        override protected void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Using SQL Server
            //optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB; Initial Catalog=FootballLeague_EFCore; Encrypt=False");

            optionsBuilder.UseSqlite($"Data Source={DbPath}")
                .LogTo(Console.WriteLine, LogLevel.Information)

                // this line is to avoid tracking entities globally
                //.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)

                // this two lines are used only for educational purposes, do not use in production
                .EnableSensitiveDataLogging() 
                .EnableDetailedErrors();  
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Team>().HasData(
                    new Team
                    {
                        Id = 1,
                        Name = "Tivoli Gardens F.C.",
                        CreatedDate = DateTimeOffset.UtcNow.DateTime
                    },
                    new Team
                    {
                        Id = 2,
                        Name = "Waterhouse F.C.",
                        CreatedDate = DateTimeOffset.UtcNow.DateTime
                    },
                    new Team
                    {
                        Id = 3,
                        Name = "Humble Lions F.C.",
                        CreatedDate = DateTimeOffset.UtcNow.DateTime
                    }
                );

        }
    }
}
