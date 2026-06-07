using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SoTro_BE.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingTypeAndImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BuildingTypeId",
                table: "Buildings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Buildings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BuildingTypes",
                columns: table => new
                {
                    BuildingTypeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TypeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingTypes", x => x.BuildingTypeId);
                });

            migrationBuilder.InsertData(
                table: "BuildingTypes",
                columns: new[] { "BuildingTypeId", "CreatedAt", "Description", "IsDeleted", "TypeName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nhà trọ phòng riêng, thường có chung khu vệ sinh hoặc riêng", false, "Nhà trọ truyền thống", null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Chung cư quy mô nhỏ, mỗi phòng có khu vệ sinh riêng", false, "Chung cư mini", null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Căn hộ đầy đủ tiện nghi, có dịch vụ dọn dẹp và bảo trì", false, "Căn hộ dịch vụ", null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Phòng ở dạng giường tầng, nhiều người ở chung một phòng", false, "Ký túc xá", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_BuildingTypeId",
                table: "Buildings",
                column: "BuildingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingTypes_TypeName",
                table: "BuildingTypes",
                column: "TypeName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Buildings_BuildingTypes_BuildingTypeId",
                table: "Buildings",
                column: "BuildingTypeId",
                principalTable: "BuildingTypes",
                principalColumn: "BuildingTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Buildings_BuildingTypes_BuildingTypeId",
                table: "Buildings");

            migrationBuilder.DropTable(
                name: "BuildingTypes");

            migrationBuilder.DropIndex(
                name: "IX_Buildings_BuildingTypeId",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "BuildingTypeId",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Buildings");
        }
    }
}
