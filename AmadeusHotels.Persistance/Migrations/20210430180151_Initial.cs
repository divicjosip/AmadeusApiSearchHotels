using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AmadeusHotels.Persistance.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hotel",
                columns: table => new
                {
                    HotelId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hotel", x => x.HotelId);
                });

            migrationBuilder.CreateTable(
                name: "SearchRequest",
                columns: table => new
                {
                    SearchRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CheckInDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2021, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)),
                    Adults = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    NextItemsLink = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchRequest", x => x.SearchRequestId);
                });

            migrationBuilder.CreateTable(
                name: "SearchRequestHotel",
                columns: table => new
                {
                    SearchRequestHotelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SearchRequestId = table.Column<int>(type: "int", nullable: false),
                    HotelId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Available = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PriceTotal = table.Column<float>(type: "real", nullable: true),
                    PriceCurrency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Distance = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchRequestHotel", x => x.SearchRequestHotelId);
                    table.ForeignKey(
                        name: "FK_SearchRequestHotel_Hotel_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotel",
                        principalColumn: "HotelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SearchRequestHotel_SearchRequest_SearchRequestId",
                        column: x => x.SearchRequestId,
                        principalTable: "SearchRequest",
                        principalColumn: "SearchRequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchRequest_CheckInDate",
                table: "SearchRequest",
                column: "CheckInDate");

            migrationBuilder.CreateIndex(
                name: "IX_SearchRequest_CheckOutDate",
                table: "SearchRequest",
                column: "CheckOutDate");

            migrationBuilder.CreateIndex(
                name: "IX_SearchRequest_CityCode",
                table: "SearchRequest",
                column: "CityCode");

            migrationBuilder.CreateIndex(
                name: "IX_SearchRequest_CityCode_CheckInDate_CheckOutDate",
                table: "SearchRequest",
                columns: new[] { "CityCode", "CheckInDate", "CheckOutDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SearchRequestHotel_HotelId",
                table: "SearchRequestHotel",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchRequestHotel_SearchRequestId",
                table: "SearchRequestHotel",
                column: "SearchRequestId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchRequestHotel");

            migrationBuilder.DropTable(
                name: "Hotel");

            migrationBuilder.DropTable(
                name: "SearchRequest");
        }
    }
}
