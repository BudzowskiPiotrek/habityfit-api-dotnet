using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Page.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitPage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CV_EXPERIENCE",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DELETED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    COMPANY_NAME = table.Column<string>(type: "text", nullable: false),
                    LOCATION = table.Column<string>(type: "text", nullable: false),
                    EMPLOYMENT_TYPE = table.Column<int>(type: "integer", nullable: false),
                    ROLE = table.Column<string>(type: "text", nullable: false),
                    START_DATE = table.Column<string>(type: "text", nullable: false),
                    END_DATE = table.Column<string>(type: "text", nullable: true),
                    IN_CURRENT_ROLE = table.Column<bool>(type: "boolean", nullable: false),
                    SHORT_SUMMARY = table.Column<string>(type: "text", nullable: false),
                    SKILLS = table.Column<string[]>(type: "text[]", nullable: false),
                    SORT_ORDER = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CV_EXPERIENCE", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CV_PROFILE",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DELETED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FULL_NAME = table.Column<string>(type: "text", nullable: false),
                    ROLE = table.Column<string>(type: "text", nullable: false),
                    SUMMARY = table.Column<string>(type: "text", nullable: false),
                    LOCATION = table.Column<string>(type: "text", nullable: false),
                    EMAIL = table.Column<string>(type: "text", nullable: false),
                    AVAILABILITY = table.Column<string>(type: "text", nullable: true),
                    GITHUB = table.Column<string>(type: "text", nullable: false),
                    LINKEDIN = table.Column<string>(type: "text", nullable: false),
                    CV_ES = table.Column<string>(type: "text", nullable: false),
                    CV_EN = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CV_PROFILE", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "VIDEO_HERO",
                columns: table => new
                {
                    SECTION_TYPE = table.Column<int>(type: "integer", nullable: false),
                    EYEBROW = table.Column<string>(type: "text", nullable: false),
                    TITLE = table.Column<string>(type: "text", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "text", nullable: false),
                    GITHUB = table.Column<string>(type: "text", nullable: false),
                    YOUTUBE = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VIDEO_HERO", x => x.SECTION_TYPE);
                });

            migrationBuilder.CreateTable(
                name: "VIDEO",
                columns: table => new
                {
                    ID = table.Column<string>(type: "text", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DELETED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TITLE = table.Column<string>(type: "text", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "text", nullable: false),
                    DURATION = table.Column<string>(type: "text", nullable: false),
                    PUBLISHED_AT = table.Column<DateOnly>(type: "date", nullable: false),
                    SECTION_TYPE = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VIDEO", x => x.ID);
                    table.ForeignKey(
                        name: "FK_VIDEO_VIDEO_HERO_SECTION_TYPE",
                        column: x => x.SECTION_TYPE,
                        principalTable: "VIDEO_HERO",
                        principalColumn: "SECTION_TYPE",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CV_EXPERIENCE_SORT_ORDER",
                table: "CV_EXPERIENCE",
                column: "SORT_ORDER");

            migrationBuilder.CreateIndex(
                name: "IX_VIDEO_SECTION_TYPE_PUBLISHED_AT",
                table: "VIDEO",
                columns: new[] { "SECTION_TYPE", "PUBLISHED_AT" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CV_EXPERIENCE");

            migrationBuilder.DropTable(
                name: "CV_PROFILE");

            migrationBuilder.DropTable(
                name: "VIDEO");

            migrationBuilder.DropTable(
                name: "VIDEO_HERO");
        }
    }
}
