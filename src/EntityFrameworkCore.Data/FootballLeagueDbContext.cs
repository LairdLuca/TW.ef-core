using EntityFrameworkCore.Data.Configurations;
using EntityFrameworkCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public FootballLeagueDbContext(DbContextOptions<FootballLeagueDbContext> options) : base(options)
        {

        }

        public DbSet<Team> Teams { get; set; }
        public DbSet<Coach> Coaches { get; set; }
        public DbSet<League> Leagues { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<TeamsAndLeaguesView> TeamsAndLeaguesViews { get; set; }
        
        public string DbPath { get; private set; }

        override protected void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Using SQL Server
            //optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB; Initial Catalog=FootballLeague_EFCore; Encrypt=False");

            optionsBuilder.UseSqlite($"Data Source={DbPath}")
                //.UseLazyLoadingProxies()
                .LogTo(Console.WriteLine, LogLevel.Information)

                // this line is to avoid tracking entities globally
                //.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)

                // this two lines are used only for educational purposes, do not use in production
                .EnableSensitiveDataLogging() 
                .EnableDetailedErrors();  
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // With this method, we can apply the configuration for each entity
            //modelBuilder.ApplyConfiguration(new TeamConfiguration());
            //modelBuilder.ApplyConfiguration(new LeagueConfiguration());

            // With this line, it will apply all configurations (IEntityTypeConfiguration<TEntity>) in the same assembly as the DbContext
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly()); 

            modelBuilder.Entity<TeamsAndLeaguesView>().HasNoKey().ToView("vm_TeamsAndLeaguesView");

            modelBuilder.HasDbFunction(typeof(FootballLeagueDbContext)
                .GetMethod(nameof(GetEarliestTeamMatch), new[] { typeof(int) }))
                .HasName("fn_GetEarliestTeamMatch");
        }

        public DateTime GetEarliestTeamMatch(int teamId) => throw new NotImplementedException();
    }
}
