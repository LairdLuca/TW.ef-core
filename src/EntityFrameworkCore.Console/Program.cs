﻿using EntityFrameworkCore.Data;
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
            //await  StampAllTeams();
            //await StampAllTeamsQuerySyntax();

            //Select one team
            //await StampOneTeam();

            //Select all record thast meet a condition
            //await StampFilteredTeams();

            // Aggregate Methods
            //await AggregateMethods();

            // Grouping and Aggregating
            var groupedTeams = context.Teams
                //.Where(team => team.CreatedDate > new DateTime(2021, 1, 1)) // Translates to a WHERE clause
                .GroupBy(team => new { team.Name, team.CreatedDate.Date })
                //.Where(group => group.Count() > 1) // Translates to a HAVING clause
                ;

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
}
