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
            // Select all teams
            StampAllTeams();

            // Selectiong a single record - First one in the list
            //Team teamOne = await context.Teams.FirstAsync();
            Team? teamOne = await context.Teams.FirstOrDefaultAsync();

            // Selectiong a single record - First one in the list that meets a condition
            //Team teamTwo = await context.Teams.FirstAsync(q => q.Id == 1);
            Team? teamTwo = await context.Teams.FirstOrDefaultAsync(q => q.Id == 1);

            // Selectiong a single record - Only one record should be returned
            //Team teamThree = await context.Teams.SingleAsync(q => q.Id == 2);
            Team? teamThree = await context.Teams.SingleOrDefaultAsync(q => q.Id == 2);

            // Selectiong based on Id
            Team? teamBasedOnId = await context.Teams.FindAsync(5);
            if(teamBasedOnId != null)
            {
                System.Console.WriteLine(teamBasedOnId.Name);
            }

        }

        public static void StampAllTeams()
        {
            // select all the teams
            // SELECT * FROM Teams
            List<Team> teams = context.Teams.ToList();

            foreach (Team team in teams)
            {
                System.Console.WriteLine(team.Name);
            }
        }
    }
}
