using EntityFrameworkCore.Data;
using EntityFrameworkCore.Domain;

namespace EntityFrameworkCore.Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //first we need an instance of the context
            var context = new FootballLeagueDbContext();

            //select all the teams
            //SELECT * FROM Teams
            List<Team> teams = context.Teams.ToList();

            foreach (Team team in teams)
            {
                System.Console.WriteLine(team.Name);
            }
        }
    }
}
