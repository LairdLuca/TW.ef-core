﻿using EntityFrameworkCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.Data.Configurations
{
    internal class TeamConfiguration : IEntityTypeConfiguration<Team>
    {
        public void Configure(EntityTypeBuilder<Team> builder)
        {
            builder.Property(t => t.Name)
               .HasMaxLength(100)
               .IsRequired();

            builder.HasIndex(t => t.Name).IsUnique();

            // Composite Key Configuration
            //builder.HasIndex(t => new { t.CoachId, t.LeagueId }).IsUnique();

            // For SQL Server Only
            //builder.Property(t => t.Version)
            //    .IsRowVersion();

            builder.Property(t => t.Version)
                .IsConcurrencyToken();


            builder.ToTable("Teams", b => b.IsTemporal());

            builder.HasMany(t => t.HomeMatches)
                .WithOne(m => m.HomeTeam)
                .HasForeignKey(m => m.HomeTeamId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.AwayMatches)
                .WithOne(m => m.AwayTeam)
                .HasForeignKey(m => m.AwayTeamId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(
                    new Team
                    {
                        Id = 1,
                        Name = "Tivoli Gardens F.C.",
                        CreatedDate = new DateTime(2024, 3, 7),
                        LeagueId = 1,
                        CoachId = 1
                    },
                    new Team
                    {
                        Id = 2,
                        Name = "Waterhouse F.C.",
                        CreatedDate = new DateTime(2024, 3, 7),
                        LeagueId = 1,
                        CoachId = 2
                    },
                    new Team
                    {
                        Id = 3,
                        Name = "Humble Lions F.C.",
                        CreatedDate = new DateTime(2024, 3, 7),
                        LeagueId = 1,
                        CoachId = 3
                    }
                );

        }
    }
}
