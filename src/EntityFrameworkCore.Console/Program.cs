using EntityFrameworkCore.Data;
using EntityFrameworkCore.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace EntityFrameworkCore.Console
{
    internal class Program
    {
        private static FootballLeagueDbContext context;

        static async Task Main(string[] args)
        {
            // First we need an instace of context
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            var dbPath = Path.Combine(path, "FootballLeague_EFCore.db");
            var optionsBuilder = new DbContextOptionsBuilder<FootballLeagueDbContext>();
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            context = new FootballLeagueDbContext(optionsBuilder.Options);



            // For SQLite Users to see where the database is being created
            //System.Console.WriteLine(context.DbPath);


            // Ensure the database is created
            //context.Database.EnsureCreated();

            // Ensure the database is created and migrated to the latest version
            await context.Database.MigrateAsync();


            #region Read Queries
            // Select all teams
            //await  StampAllTeams();
            //await StampAllTeamsQuerySyntax();

            //Select one team
            //await StampOneTeam();

            //Select all record thast meet a condition
            //await StampFilteredTeams();

            // Aggregate Methods
            //await AggregateMethods();

            // Grouping and Aggregating
            //await GroupByMethod();

            // Ordering
            //await OrderByMethods();

            // Skip and Take - Great for Paging
            //await SkipAndTake();

            // Select and Projections - more precise quries
            //await SelectAndProjections();

            // No Tracking - EF Core tracks object that are returned by queries.
            // This is less useful in disconnected applications like APIs and Web apps
            //await NoTracking();

            // IQueryables vs List Types
            //await ListVsQeryable();
            #endregion

            #region Write Queries
            // Inserting Data
            //await InsertOperations();

            // Update Operationss
            //await UpdateOperations();

            // Delete Operations
            //await DeleteOperations();

            // Execute Operations
            //await ExecuteOperations();
            #endregion

            #region Related Data
            // Insert Related Data
            //await InsertRelatedData();
            //await InsertOneRecordWithAudit();

            // Eager Loading Data
            //await EagerLoadingData();

            // Explicit Loading Data
            //await ExplicitLoadingData();

            // Lazy Loading
            //await LazyLoadingData();

            // Filtering Includes
            // Get all teams and only home matches where they have scored
            //await FileringIncludes();

            // Projection and Anonymous types
            //await ProjectionAndAnonymousTypes();

            #endregion

            #region Raw SQL
            // Queryng a Keyless Entity
            //await QueryngKeylessEntityOrVIew();

            // Querying with Raw SQL
            //await QueryingWithRawSQL();




            #endregion

            #region Additional Queries
            //await TemporalTableQuery();

            //TransactionSupport();

            //Concurrency Check
            var team = await context.Teams.FindAsync(1);
            team.Name = "New Name with concurrency Check";
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                System.Console.WriteLine(ex.Message);
                throw;
            }

            #endregion

        }

        public static void TransactionSupport()
        {
            var transaction = context.Database.BeginTransaction();
            var league = new League
            {
                Name = "Testing Transactions"
            };

            context.Add(league);
            context.SaveChanges();
            transaction.CreateSavepoint("CreatedLeague");

            var coach = new Coach
            {
                Name = "Transaction Coach",
            };
            context.Add(coach);
            context.SaveChanges();

            var Teams = new List<Team>
            {
                new Team
                {
                    Name = "Transaction Team 1",
                    LeagueId = league.Id,
                    CoachId = coach.Id
                },
            };
            context.AddRange(Teams);
            context.SaveChanges();

            try
            {
                transaction.Commit();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                //transaction.Rollback();
                transaction.RollbackToSavepoint("CreatedLeague");
            }
        }

        public static async Task TemporalTableQuery()
        {
            using var sqlServerContext = new FootballLeagueSqlServerDbContext();

            var teamHistory = sqlServerContext.Teams
               .TemporalAll()
               .Where(q => q.Id == 1)
               .Select(q => new
               {
                   q.Name,
                   ValueFrom = EF.Property<DateTime>(q, "PeriodStart"),
                   ValueTo = EF.Property<DateTime>(q, "PeriodEnd")
               })
               .ToList();

            foreach (var team in teamHistory)
            {
                System.Console.WriteLine($"{team.Name} - {team.ValueFrom} to {team.ValueTo}");
            }
        }

        public static async Task QueryingWithRawSQL()
        {
            // FromSqlRaw
            System.Console.WriteLine("Enter Team Name: ");
            var teamName = System.Console.ReadLine();
            var teamNameParam = new SqliteParameter("teamName", teamName);
            var teams = context.Teams.FromSqlRaw($"SELECT * FROM Teams WHERE Name = @teamName ", teamNameParam);
            foreach (var t in teams)
            {
                System.Console.WriteLine(t.Name);
            }

            // FromSql
            teams = context.Teams.FromSql($"SELECT * FROM Teams WHERE Name = {teamName} ");
            foreach (var t in teams)
            {
                System.Console.WriteLine(t.Name);
            }

            // FromSqlInterpolated
            teams = context.Teams.FromSqlInterpolated($"SELECT * FROM Teams WHERE Name = {teamName} ");
            foreach (var t in teams)
            {
                System.Console.WriteLine(t.Name);
            }

            // Mixing with LINQ
            var teamsList = context.Teams.FromSqlRaw($"SELECT * FROM Teams WHERE Name = @teamName ", teamNameParam)
                .Where(t => t.Id > 1)
                .OrderBy(t => t.Name)
                .Include(t => t.Coach)
                .ToList();

            foreach (var t in teamsList)
            {
                System.Console.WriteLine($"{t.Name} - {t.Coach.Name}");
            }

            // Execute Stored Procedures
            var leagueId = 1;
            var league = context.Leagues.FromSqlInterpolated($"EXEC dbo.StoredProcedureToGetLeagueNameHere {leagueId}");

            // Non-queryng statemant
            var someNewTeamName = "New Team Name Here";
            int success = context.Database.ExecuteSqlInterpolated($"UPDATE Teams SET Name = {someNewTeamName} WHERE Id = 1");

            var teamToDeleteId = 1;
            int deleted = context.Database.ExecuteSqlInterpolated($"EXEC dbo.DeleteTeam {teamToDeleteId}");

            // Query Scalar or Non-Entity Type
            var leagueIds = context.Database.SqlQuery<int>($"SELECT Id FROM Leagues").ToList();

            // Execute User-Defined Query
            var earlistMatch = context.GetEarliestTeamMatch(1);
        }

        public static async Task QueryngKeylessEntityOrVIew()
        {
            var details = await context.TeamsAndLeaguesViews.ToListAsync();
        }

        public static async Task ProjectionAndAnonymousTypes()
        {
            var teams = await context.Teams
                .Select(team => new TeamDetails
                {
                    TeamId = team.Id,
                    TeamName = team.Name,
                    CoachName = team.Coach.Name,
                    TotalHomeGoals = team.HomeMatches.Sum(match => match.HomeTeamScore),
                    TotalAwayGoals = team.AwayMatches.Sum(match => match.AwayTeamScore)
                })
                .ToListAsync();

            foreach (var team in teams)
            {
                System.Console.WriteLine($"{team.TeamName} - {team.CoachName} | Home Goals: {team.TotalHomeGoals} | Away Goals: {team.TotalAwayGoals}");
            }
        }

        public static async Task FileringIncludes()
        {
            //await InsertMoreMatches();
            var teams = await context.Teams
                .Include(t => t.Coach)
                .Include(team => team.HomeMatches.Where(match => match.HomeTeamScore > 0))
                .ToListAsync();

            foreach (var team in teams)
            {
                System.Console.WriteLine($"{team.Name} - {team.Coach.Name}");
                foreach (var match in team.HomeMatches)
                {
                    System.Console.WriteLine($"Score: {match.HomeTeamScore} ");
                }
            }
        }

        public static async Task InsertMoreMatches()
        {
            var match1 = new Match
            {
                AwayTeamId = 2,
                HomeTeamId = 3,
                HomeTeamScore = 1,
                AwayTeamScore = 0,
                MatchDate = new DateTime(2023, 01, 1),
                TicketPrice = 20,
            };
            var match2 = new Match
            {
                AwayTeamId = 2,
                HomeTeamId = 1,
                HomeTeamScore = 1,
                AwayTeamScore = 0,
                MatchDate = new DateTime(2023, 01, 1),
                TicketPrice = 20,
            };
            var match3 = new Match
            {
                AwayTeamId = 1,
                HomeTeamId = 3,
                HomeTeamScore = 1,
                AwayTeamScore = 0,
                MatchDate = new DateTime(2023, 01, 1),
                TicketPrice = 20,
            };
            var match4 = new Match
            {
                AwayTeamId = 4,
                HomeTeamId = 3,
                HomeTeamScore = 0,
                AwayTeamScore = 1,
                MatchDate = new DateTime(2023, 01, 1),
                TicketPrice = 20,
            };
            await context.AddRangeAsync(match1, match2, match3, match4);
            await context.SaveChangesAsync();
        }

        public static async Task LazyLoadingData()
        {
            var league = await context.FindAsync<League>(1);
            foreach (var team in league.Teams)
            {
                System.Console.WriteLine(team.Name);
            }

            // This is why lazy loading is not recommended (Read the output)
            foreach (var leaguee in await context.Leagues.ToListAsync())
            {
                foreach (var team in leaguee.Teams)
                {
                    System.Console.WriteLine($"{team.Name} - {team.Coach.Name}");
                }
            }
        }

        public static async Task ExplicitLoadingData()
        {
            var league = await context.FindAsync<League>(1);
            if (!league.Teams.Any())
            {
                System.Console.WriteLine("No teams found");
            }

            context.Entry(league).Collection(league => league.Teams).Load();
            if (league.Teams.Any())
            {
                foreach (var team in league.Teams)
                {
                    System.Console.WriteLine(team.Name);
                }
            }
        }

        public static async Task EagerLoadingData()
        {
            var league = await context.Leagues
                .Include(league => league.Teams)
                .ThenInclude(team => team.Coach)
                .ToListAsync();
            foreach (var item in league)
            {
                System.Console.WriteLine($"League: {item.Name}");
                foreach (var team in item.Teams)
                {
                    System.Console.WriteLine($"- {team.Name} --> {team.Coach.Name}");
                }
            }
        }

        public static async Task InsertRelatedData()
        {
            // Insert record with FK
            var match1 = new Match
            {
                HomeTeamId = 1,
                AwayTeamId = 2,
                HomeTeamScore = 0,
                AwayTeamScore = 0,
                MatchDate = new DateTime(2025, 10, 10),
                TicketPrice = 20
            };

            await context.AddAsync(match1);
            await context.SaveChangesAsync();

            /* Incorrect reference data - Will give error */
            //var match2 = new Match
            //{
            //    HomeTeamId = 10,
            //    AwayTeamId = 0,
            //    HomeTeamScore = 0,
            //    AwayTeamScore = 0,
            //    MatchDate = new DateTime(2025, 10, 10),
            //    TicketPrice = 20
            //};

            //await context.AddAsync(match2);
            //await context.SaveChangesAsync();

            // Insert Parent/Child
            var team = new Team
            {
                Name = "Manchester United F.C.",
                Coach = new Coach
                {
                    Name = "Roberto Rossini",
                }
            };
            await context.AddAsync(team);
            await context.SaveChangesAsync();

            // Insert Parent with Children
            var league = new League
            {
                Name = "Serie A",
                Teams = new List<Team>
                {
                    new Team
                    {
                        Name = "Juventus",
                        Coach = new Coach
                        {
                            Name = "Juve Coach"
                        }
                    },
                    new Team
                    {
                        Name = "AC Milan",
                        Coach = new Coach
                        {
                            Name = "Milan Coach"
                        }
                    },
                    new Team
                    {
                        Name = "AS Roma",
                        Coach = new Coach
                        {
                            Name = "Roma Coach"
                        }
                    }
                }
            };
            await context.AddAsync(league);
            await context.SaveChangesAsync();
        }

        public static async Task InsertOneRecordWithAudit()
        {
            var newLeague = new League
            {
                Name = "Premier League",
            };

            await context.Leagues.AddAsync(newLeague);
            await context.SaveChangesAsync();
        }

        public static async Task ExecuteOperations()
        {
            // Execute Delete (EF Core >= 7)
            await context.Coaches
                .Where(coach => coach.Name == "Roberto Rossini")
                .ExecuteDeleteAsync();

            // Execute Update (EF Core >= 7)
            await context.Coaches
                .Where(coach => coach.Name == "Roberto Rossi")
                .ExecuteUpdateAsync(coach => coach
                    .SetProperty(prop => prop.Name, "Pep Guardiola")
                    .SetProperty(prop => prop.CreatedDate, DateTime.Now
                ));
        }

        private static async Task DeleteOperations()
        {
            /* DELETE FROM Coaches WHERE ID = 1 */
            var coach = await context.Coaches.FindAsync(9);
            context.Remove(coach);
            //context.Entry(coach).State = EntityState.Deleted; //This produces the same result as the line above
            await context.SaveChangesAsync();
        }


        private static async Task UpdateOperations()
        {
            Coach? coach = await context.Coaches.FindAsync(9);
            coach.Name = "Roberto Rossi";
            await context.SaveChangesAsync();

            // With no tracking
            Coach? coachNoTracking = await context.Coaches.AsNoTracking().FirstOrDefaultAsync(coach => coach.Id == 9);
            coachNoTracking.Name = "Roberto Rossini";

            System.Console.WriteLine(context.ChangeTracker.DebugView.LongView);
            context.Coaches.Update(coachNoTracking);
            //context.Entry(coachNoTracking).State = EntityState.Modified; //This produces the same result as the line above
            System.Console.WriteLine(context.ChangeTracker.DebugView.LongView);
            await context.SaveChangesAsync();
            System.Console.WriteLine(context.ChangeTracker.DebugView.LongView);

        }

        private static async Task InsertOperations()
        {
            /* INSERT INTO Coaches (cols) VALUES (values) */

            // Simple Insert
            var newCoach = new Coach
            {
                Name = "John Doe",
                CreatedDate = DateTime.Now
            };
            await context.Coaches.AddAsync(newCoach);
            await context.SaveChangesAsync();

            // Loop Insert
            var newCoaches = new List<Coach>
            {
                new Coach { Name = "Theodore Whitmore", CreatedDate = DateTime.Now },
                new Coach { Name = "Jose Mourinho", CreatedDate = DateTime.Now },
                new Coach { Name = "Jane Smith", CreatedDate = DateTime.Now }
            };
            foreach (var coach in newCoaches)
            {
                await context.Coaches.AddAsync(coach);
            }
            System.Console.WriteLine(context.ChangeTracker.DebugView.LongView);
            await context.SaveChangesAsync();
            System.Console.WriteLine(context.ChangeTracker.DebugView.LongView);

            foreach (var coach in newCoaches)
            {
                System.Console.WriteLine($"{coach.Id} - {coach.Name}");
            }

            // Batch Insert
            await context.Coaches.AddRangeAsync(newCoaches);
            await context.SaveChangesAsync();
        }

        private static async Task ListVsQeryable()
        {
            System.Console.WriteLine("Press '1' for Team with Id 1 or '2' for teams that contain 'F.C.'");
            int option = Convert.ToInt32(System.Console.ReadLine());
            List<Team> teamsAsList = new List<Team>();

            // After executing to ToListAsync, the records are loaded into memory. Any operations is then done in memory
            teamsAsList = await context.Teams.ToListAsync();
            if (option == 1)
            {
                teamsAsList = teamsAsList.Where(team => team.Id == 1).ToList();
            }
            else if (option == 2)
            {
                teamsAsList = teamsAsList.Where(team => team.Name.Contains("F.C.")).ToList();
            }

            foreach (var team in teamsAsList)
            {
                System.Console.WriteLine(team.Name);
            }

            // Records stay as IQueryable until the ToListAsync is executed, then the final query is performed
            IQueryable<Team> teamsAsQueryable = context.Teams.AsQueryable();
            if (option == 1)
            {
                teamsAsQueryable = teamsAsQueryable.Where(team => team.Id == 1);
            }
            else if (option == 2)
            {
                teamsAsQueryable = teamsAsQueryable.Where(team => team.Name.Contains("F.C."));
            }

            teamsAsList = await teamsAsQueryable.ToListAsync();
            foreach (var team in teamsAsList)
            {
                System.Console.WriteLine(team.Name);
            }
        }

        private static async Task NoTracking()
        {
            var teams = await context.Teams
                .AsNoTracking()
                //.AsTracking() // this line is to track the entity if the context is configured to not track entities
                .ToListAsync();

            foreach (var team in teams)
            {
                System.Console.WriteLine(team.Name);
            }
        }

        private static async Task SelectAndProjections()
        {
            List<string> teams = await context.Teams
                .Select(team => team.Name)
                .ToListAsync();

            foreach (string name in teams)
            {
                System.Console.WriteLine(name);
            }

            var temas = await context.Teams
                .Select(team => new { team.Name, team.CreatedDate })
                .ToListAsync();

            foreach (var teram in temas)
            {
                System.Console.WriteLine($"{teram.Name} - {teram.CreatedDate}");
            }

            List<TeamInfo> teamsInfo = await context.Teams
                .Select(team => new TeamInfo { Name = team.Name, CreatedDate = team.CreatedDate })
                .ToListAsync();

            foreach (var teamInfo in teamsInfo)
            {
                System.Console.WriteLine($"{teamInfo.Name} - {teamInfo.CreatedDate}");
            }
        }

        private static async Task SkipAndTake()
        {
            int recordCount = 3;
            int page = 0;
            bool next = true;
            while (next)
            {
                List<Team> teams = await context.Teams.Skip(page * recordCount).Take(recordCount).ToListAsync();
                System.Console.WriteLine($"Page {page + 1}");
                foreach (Team team in teams)
                {
                    System.Console.WriteLine(team.Name);
                }
                System.Console.WriteLine("Enter 'true' for the next set of records, 'false' to exit");
                next = Convert.ToBoolean(System.Console.ReadLine());

                if (!next) break;
                page++;
            }
        }

        private static async Task OrderByMethods()
        {
            List<Team> orderedTeams = await context.Teams.OrderBy(team => team.Name).ToListAsync();
            foreach (var team in orderedTeams)
            {
                System.Console.WriteLine(team.Name);
            }

            List<Team> descOrderedTeams = await context.Teams.OrderByDescending(team => team.Name).ToListAsync();
            foreach (var team in descOrderedTeams)
            {
                System.Console.WriteLine(team.Name);
            }

            // Gettting the record with a maximum value
            Team? maxByDescedingOrder = await context.Teams
                .OrderByDescending(team => team.Id)
                .FirstOrDefaultAsync();

            // Same as above, but using the MaxBy method
            Team? maxBy = context.Teams.MaxBy(team => team.Id);
        }

        private static async Task GroupByMethod()
        {
            var groupedTeams = context.Teams
                //.Where(team => team.CreatedDate > new DateTime(2021, 1, 1)) // Translates to a WHERE clause
                .GroupBy(team => new { team.Name, team.CreatedDate.Date })
                //.Where(group => group.Count() > 1) // Translates to a HAVING clause
                ;
            // .ToList(); // Use the executing method to load the results into memory before processing

            // EF Core can iterate throught records on demand. Here, is executing method, but EF Core is bringing back records per iteratiom
            // This is convenint, but dangerus when you have several operations to complete per iteration.
            // It is generally better to execute ToList() and then operate on whatever is returned to memory.
            foreach (var group in groupedTeams)
            {
                System.Console.WriteLine($"Group: {group.Key}");
                System.Console.WriteLine($"Sum IDs: {group.Sum(q => q.Id)}");
                foreach (var team in group)
                {
                    System.Console.WriteLine(team.Name);
                }
            }
        }

        private static async Task AggregateMethods()
        {
            // Count
            int numberOfTeams = await context.Teams.CountAsync();
            System.Console.WriteLine($"Number of teams: {numberOfTeams}");

            int numberOfTeamsWithCondition = await context.Teams.CountAsync(q => q.Id == 1);
            System.Console.WriteLine($"Number of teams with condition: {numberOfTeamsWithCondition}");

            // Max
            int maxTeams = await context.Teams.MaxAsync(q => q.Id);
            // Min
            int minTeams = await context.Teams.MinAsync(q => q.Id);
            // Average
            double averageTeams = await context.Teams.AverageAsync(q => q.Id);
            // Sum
            int sumTeams = await context.Teams.SumAsync(q => q.Id);
        }

        private static async Task StampAllTeamsQuerySyntax()
        {
            System.Console.WriteLine("Enter search term");
            string? searchTerm = System.Console.ReadLine();

            List<Team> teams = await (
                from team in context.Teams
                where EF.Functions.Like(team.Name, $"%{searchTerm}%")
                select team
                ).ToListAsync();

            foreach (var team in teams)
            {
                System.Console.WriteLine(team.Name);
            }
        }

        private static async Task StampFilteredTeams()
        {
            System.Console.WriteLine("Enter the name of the team you want to find");
            string? desiredTeam = System.Console.ReadLine();

            List<Team> teamsFiltered = await context.Teams.Where(team => team.Name == desiredTeam).ToListAsync();
            foreach (var team in teamsFiltered)
            {
                System.Console.WriteLine(team.Name);
            }

            System.Console.WriteLine("Enter search term");
            string? searchTerm = System.Console.ReadLine();

            //List<Team> teamsFilteredTwo = await context.Teams.Where(team => team.Name.Contains(searchTerm)).ToListAsync();

            // SELECT * FROM Teams WHERE Name LIKE '%searchTerm%'
            List<Team> teamsFilteredTwo = await context.Teams.Where(team => EF.Functions.Like(team.Name, $"%{searchTerm}%")).ToListAsync();
            foreach (var team in teamsFilteredTwo)
            {
                System.Console.WriteLine(team.Name);
            }

        }

        private static async Task StampAllTeams()
        {
            // select all the teams
            // SELECT * FROM Teams
            List<Team> teams = await context.Teams.ToListAsync();

            foreach (Team team in teams)
            {
                System.Console.WriteLine(team.Name);
            }
        }

        private static async Task StampOneTeam()
        {
            // Selecting a single record - First one in the list
            Team? teamFirst = await context.Teams.FirstAsync();
            if (teamFirst != null)
            {
                System.Console.WriteLine(teamFirst.Name);
            }
            Team? teamFirstOrDefault = await context.Teams.FirstOrDefaultAsync();
            if (teamFirstOrDefault != null)
            {
                System.Console.WriteLine(teamFirstOrDefault.Name);
            }

            // Selecting a single record - First one in the list that meets a condition
            Team? teamFirstWithCondition = await context.Teams.FirstAsync(team => team.Id == 1);
            if (teamFirstWithCondition != null)
            {
                System.Console.WriteLine(teamFirstWithCondition.Name);
            }
            Team? teamFirstOrDefaultWithCondition = await context.Teams.FirstOrDefaultAsync(team => team.Id == 1);
            if (teamFirstOrDefaultWithCondition != null)
            {
                System.Console.WriteLine(teamFirstOrDefaultWithCondition.Name);
            }

            // Selecting a single record - Only one record should be returned, or an exception will be thrown
            //Team? teamSingle = await context.Teams.SingleAsync();
            //if (teamSingle != null)
            //{
            //    System.Console.WriteLine(teamSingle.Name);
            //}
            Team? teamSingleWithCondition = await context.Teams.SingleAsync(team => team.Id == 2);
            if (teamSingleWithCondition != null)
            {
                System.Console.WriteLine(teamSingleWithCondition.Name);
            }
            Team? SingleOrDefault = await context.Teams.SingleOrDefaultAsync(team => team.Id == 2);
            if (SingleOrDefault != null)
            {
                System.Console.WriteLine(SingleOrDefault.Name);
            }

            // Selecting based on Primary Key Id value
            Team? teamBasedOnId = await context.Teams.FindAsync(3);
            if (teamBasedOnId != null)
            {
                System.Console.WriteLine(teamBasedOnId.Name);
            }

        }
    }

    public class TeamInfo
    {
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    class TeamDetails
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public string CoachName { get; set; }

        public int TotalHomeGoals { get; set; }
        public int TotalAwayGoals { get; set; }
    }

}
