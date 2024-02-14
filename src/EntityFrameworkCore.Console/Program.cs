using EntityFrameworkCore.Data;
using EntityFrameworkCore.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Console
{
    internal class Program
    {
        // First we need an instance of the context
        private static FootballLeagueDbContext context = new FootballLeagueDbContext();

        static async Task Main(string[] args)
        {
            System.Console.WriteLine(context.DbPath);

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

            // Rcords stsy as IQueryable until the ToListAsync is executed, then the final query is performed
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
}
