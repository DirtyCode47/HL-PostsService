using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostsService.Migrations
{
    /// <inheritdoc />
    public partial class AddedMessagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsKafkaMessageSended",
                table: "Posts");

            migrationBuilder.CreateTable(
                name: "PostMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    River = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    postStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostMessages", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostMessages");

            migrationBuilder.AddColumn<bool>(
                name: "IsKafkaMessageSended",
                table: "Posts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
