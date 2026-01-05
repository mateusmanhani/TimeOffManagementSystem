using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAG.TOF.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameStatusIdToStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StatusId",
                table: "Requests",
                newName: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Requests",
                newName: "StatusId");
        }
    }
}
