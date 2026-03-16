using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobCommandCenter.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    LinkedInJobId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Company = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PayRate = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Found"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRemote = table.Column<bool>(type: "boolean", nullable: false),
                    IsContract = table.Column<bool>(type: "boolean", nullable: false),
                    IsTopApplicant = table.Column<bool>(type: "boolean", nullable: false),
                    IsPromoted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Company",
                table: "Jobs",
                column: "Company");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_IsRemote",
                table: "Jobs",
                column: "IsRemote");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_LinkedInJobId",
                table: "Jobs",
                column: "LinkedInJobId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Score",
                table: "Jobs",
                column: "Score");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Status",
                table: "Jobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Status_CreatedAt",
                table: "Jobs",
                columns: new[] { "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Jobs");
        }
    }
}
