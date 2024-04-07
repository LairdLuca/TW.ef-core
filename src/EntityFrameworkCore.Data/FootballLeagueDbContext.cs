using Castle.Core.Configuration;
using EntityFrameworkCore.Data.Configurations;
using EntityFrameworkCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
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
        public FootballLeagueDbContext(DbContextOptions<FootballLeagueDbContext> options) : base(options)
        {

        }

        public DbSet<Team> Teams { get; set; }
        public DbSet<Coach> Coaches { get; set; }
        public DbSet<League> Leagues { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<TeamsAndLeaguesView> TeamsAndLeaguesViews { get; set; }
        

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

    public class FootballLeagueDbContextFactory : IDesignTimeDbContextFactory<FootballLeagueDbContext>
    {
        public FootballLeagueDbContext CreateDbContext(string[] args)
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            string sqliteDatabaseName = configuration.GetConnectionString("SqliteDatabaseConnectionString");
            string dbPath = Path.Combine(path, sqliteDatabaseName);
            string connectionString = $"Data Source={dbPath}";

            var optionsBuilder = new DbContextOptionsBuilder<FootballLeagueDbContext>();
            optionsBuilder.UseSqlite(connectionString);

            return new FootballLeagueDbContext(optionsBuilder.Options);
        }
    }
}
