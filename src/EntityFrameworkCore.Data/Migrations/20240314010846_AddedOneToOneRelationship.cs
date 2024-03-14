using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityFrameworkCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedOneToOneRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Coaches");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_CoachId",
                table: "Teams",
                column: "CoachId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Coaches_CoachId",
                table: "Teams",
                column: "CoachId",
                principalTable: "Coaches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Coaches_CoachId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_CoachId",
                table: "Teams");

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "Coaches",
                type: "INTEGER",
                nullable: true);
        }
    }
}
