using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace test_2.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__AdminActi__Admin__4BAC3F29",
                table: "AdminActivities");

            migrationBuilder.DropForeignKey(
                name: "FK__Appointme__Garag__32E0915F",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK__Appointme__Servi__33D4B598",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK__Appointme__Techn__34C8D9D1",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK__Appointme__UserI__30F848ED",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK__Appointme__Vehic__31EC6D26",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK__GarageSch__Garag__4F7CD00D",
                table: "GarageSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK__Notificat__UserI__4222D4EF",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK__RepairSta__Appoi__38996AB5",
                table: "RepairStatus");

            migrationBuilder.DropForeignKey(
                name: "FK__Reviews__GarageI__3D5E1FD2",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK__Reviews__UserID__3C69FB99",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK__Services__Garage__2E1BDC42",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK__Technical__Appoi__46E78A0C",
                table: "TechnicalReports");

            migrationBuilder.DropForeignKey(
                name: "FK__Technical__Techn__47DBAE45",
                table: "TechnicalReports");

            migrationBuilder.DropForeignKey(
                name: "FK__Vehicles__UserID__29572725",
                table: "Vehicles");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Vehicles__476B54B285DEB84B",
                table: "Vehicles");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Users__1788CCAC4A2D9ED3",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Technica__D5BD48E5EA433717",
                table: "TechnicalReports");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Services__C51BB0EA81D78BEA",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_GarageID",
                table: "Services");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Reviews__74BC79AE3419A5AC",
                table: "Reviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK__RepairSt__C8EE2043CE0D7330",
                table: "RepairStatus");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Notifica__20CF2E32DE412EF6",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK__GarageSc__9C8A5B690D2A35B7",
                table: "GarageSchedules");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Garages__5D8BEEB1FF3115B3",
                table: "Garages");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Appointm__8ECDFCA2221F534A",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ServiceID",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_VehicleID",
                table: "Appointments");

            migrationBuilder.DropPrimaryKey(
                name: "PK__AdminAct__45F4A7F1428348E4",
                table: "AdminActivities");

            migrationBuilder.DropColumn(
                name: "GarageID",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ServiceID",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "VehicleID",
                table: "Appointments");

            migrationBuilder.RenameIndex(
                name: "UQ__Users__536C85E4B2020C26",
                table: "Users",
                newName: "UQ__Users__536C85E468B7CFB5");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Users",
                type: "decimal(18,2)",
                nullable: true,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<string>(
                name: "image_url",
                table: "Services",
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK__Vehicles__476B54B2980519D1",
                table: "Vehicles",
                column: "VehicleID");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Users__1788CCAC86DE46E3",
                table: "Users",
                column: "UserID");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Technica__D5BD48E57FF39466",
                table: "TechnicalReports",
                column: "ReportID");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Services__C51BB0EAA94B5AB2",
                table: "Services",
                column: "ServiceID");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Reviews__74BC79AE618E1F19",
                table: "Reviews",
                column: "ReviewID");

            migrationBuilder.AddPrimaryKey(
                name: "PK__RepairSt__C8EE204389B34B5D",
                table: "RepairStatus",
                column: "StatusID");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Notifica__20CF2E32CD8684D9",
                table: "Notifications",
                column: "NotificationID");

            migrationBuilder.AddPrimaryKey(
                name: "PK__GarageSc__9C8A5B69CEA0A1A1",
                table: "GarageSchedules",
                column: "ScheduleID");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Garages__5D8BEEB12000B258",
                table: "Garages",
                column: "GarageID");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Appointm__8ECDFCA2FC58BAB1",
                table: "Appointments",
                column: "AppointmentID");

            migrationBuilder.AddPrimaryKey(
                name: "PK__AdminAct__45F4A7F17FD61833",
                table: "AdminActivities",
                column: "ActivityID");

            migrationBuilder.CreateTable(
                name: "AppointmentVehicleDetails",
                columns: table => new
                {
                    AppointmentVehicleDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppointmentID = table.Column<int>(type: "int", nullable: false),
                    VehicleID = table.Column<int>(type: "int", nullable: true),
                    ServiceID = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true, defaultValue: 1),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Appointm__C23A2797927FBB80", x => x.AppointmentVehicleDetailID);
                    table.ForeignKey(
                        name: "FK__Appointme__Appoi__3A81B327",
                        column: x => x.AppointmentID,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Appointme__Servi__3C69FB99",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ServiceID");
                    table.ForeignKey(
                        name: "FK__Appointme__Vehic__3B75D760",
                        column: x => x.VehicleID,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleID");
                });

            migrationBuilder.CreateTable(
                name: "GarageServices",
                columns: table => new
                {
                    GarageID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__GarageSe__E1DA55BF27EE9BCB", x => new { x.GarageID, x.ServiceID });
                    table.ForeignKey(
                        name: "FK__GarageSer__Garag__30F848ED",
                        column: x => x.GarageID,
                        principalTable: "Garages",
                        principalColumn: "GarageID");
                    table.ForeignKey(
                        name: "FK__GarageSer__Servi__31EC6D26",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ServiceID");
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    OrderDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "Pending")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Orders__C3905BAF1B373AAE", x => x.OrderID);
                    table.ForeignKey(
                        name: "FK__Orders__UserID__5DCAEF64",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Products__B40CC6ED1AADDAAF", x => x.ProductID);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    CartItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    ProductID = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CartItem__488B0B2A889DEFAB", x => x.CartItemID);
                    table.ForeignKey(
                        name: "FK__CartItems__Produ__6754599E",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                    table.ForeignKey(
                        name: "FK__CartItems__UserI__66603565",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "FavoriteProducts",
                columns: table => new
                {
                    FavoriteID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    ProductID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Favorite__CE74FAF531B640F2", x => x.FavoriteID);
                    table.ForeignKey(
                        name: "FK__FavoriteP__Produ__6C190EBB",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                    table.ForeignKey(
                        name: "FK__FavoriteP__UserI__6B24EA82",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    OrderItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderID = table.Column<int>(type: "int", nullable: true),
                    ProductID = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__OrderIte__57ED06A1A12E2710", x => x.OrderItemID);
                    table.ForeignKey(
                        name: "FK__OrderItem__Order__628FA481",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID");
                    table.ForeignKey(
                        name: "FK__OrderItem__Produ__6383C8BA",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentVehicleDetails_AppointmentID",
                table: "AppointmentVehicleDetails",
                column: "AppointmentID");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentVehicleDetails_ServiceID",
                table: "AppointmentVehicleDetails",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentVehicleDetails_VehicleID",
                table: "AppointmentVehicleDetails",
                column: "VehicleID");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductID",
                table: "CartItems",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_UserID",
                table: "CartItems",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteProducts_ProductID",
                table: "FavoriteProducts",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteProducts_UserID",
                table: "FavoriteProducts",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_GarageServices_ServiceID",
                table: "GarageServices",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderID",
                table: "OrderItems",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductID",
                table: "OrderItems",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserID",
                table: "Orders",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__AdminActi__Admin__5441852A",
                table: "AdminActivities",
                column: "AdminID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__Appointme__Garag__35BCFE0A",
                table: "Appointments",
                column: "GarageID",
                principalTable: "Garages",
                principalColumn: "GarageID");

            migrationBuilder.AddForeignKey(
                name: "FK__Appointme__Techn__36B12243",
                table: "Appointments",
                column: "TechnicianID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__Appointme__UserI__34C8D9D1",
                table: "Appointments",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__GarageSch__Garag__5812160E",
                table: "GarageSchedules",
                column: "GarageID",
                principalTable: "Garages",
                principalColumn: "GarageID");

            migrationBuilder.AddForeignKey(
                name: "FK__Notificat__UserI__4AB81AF0",
                table: "Notifications",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__RepairSta__Appoi__412EB0B6",
                table: "RepairStatus",
                column: "AppointmentID",
                principalTable: "Appointments",
                principalColumn: "AppointmentID");

            migrationBuilder.AddForeignKey(
                name: "FK__Reviews__GarageI__45F365D3",
                table: "Reviews",
                column: "GarageID",
                principalTable: "Garages",
                principalColumn: "GarageID");

            migrationBuilder.AddForeignKey(
                name: "FK__Reviews__UserID__44FF419A",
                table: "Reviews",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__Technical__Appoi__4F7CD00D",
                table: "TechnicalReports",
                column: "AppointmentID",
                principalTable: "Appointments",
                principalColumn: "AppointmentID");

            migrationBuilder.AddForeignKey(
                name: "FK__Technical__Techn__5070F446",
                table: "TechnicalReports",
                column: "TechnicianID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__Vehicles__UserID__2A4B4B5E",
                table: "Vehicles",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__AdminActi__Admin__5441852A",
                table: "AdminActivities");

            migrationBuilder.DropForeignKey(
                name: "FK__Appointme__Garag__35BCFE0A",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK__Appointme__Techn__36B12243",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK__Appointme__UserI__34C8D9D1",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK__GarageSch__Garag__5812160E",
                table: "GarageSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK__Notificat__UserI__4AB81AF0",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK__RepairSta__Appoi__412EB0B6",
                table: "RepairStatus");

            migrationBuilder.DropForeignKey(
                name: "FK__Reviews__GarageI__45F365D3",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK__Reviews__UserID__44FF419A",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK__Technical__Appoi__4F7CD00D",
                table: "TechnicalReports");

            migrationBuilder.DropForeignKey(
                name: "FK__Technical__Techn__5070F446",
                table: "TechnicalReports");

            migrationBuilder.DropForeignKey(
                name: "FK__Vehicles__UserID__2A4B4B5E",
                table: "Vehicles");

            migrationBuilder.DropTable(
                name: "AppointmentVehicleDetails");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "FavoriteProducts");

            migrationBuilder.DropTable(
                name: "GarageServices");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Vehicles__476B54B2980519D1",
                table: "Vehicles");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Users__1788CCAC86DE46E3",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Technica__D5BD48E57FF39466",
                table: "TechnicalReports");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Services__C51BB0EAA94B5AB2",
                table: "Services");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Reviews__74BC79AE618E1F19",
                table: "Reviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK__RepairSt__C8EE204389B34B5D",
                table: "RepairStatus");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Notifica__20CF2E32CD8684D9",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK__GarageSc__9C8A5B69CEA0A1A1",
                table: "GarageSchedules");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Garages__5D8BEEB12000B258",
                table: "Garages");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Appointm__8ECDFCA2FC58BAB1",
                table: "Appointments");

            migrationBuilder.DropPrimaryKey(
                name: "PK__AdminAct__45F4A7F17FD61833",
                table: "AdminActivities");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "image_url",
                table: "Services");

            migrationBuilder.RenameIndex(
                name: "UQ__Users__536C85E468B7CFB5",
                table: "Users",
                newName: "UQ__Users__536C85E4B2020C26");

            migrationBuilder.AddColumn<int>(
                name: "GarageID",
                table: "Services",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Services",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceID",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VehicleID",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK__Vehicles__476B54B285DEB84B",
                table: "Vehicles",
                column: "VehicleID");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Users__1788CCAC4A2D9ED3",
                table: "Users",
                column: "UserID");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Technica__D5BD48E5EA433717",
                table: "TechnicalReports",
                column: "ReportID");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Services__C51BB0EA81D78BEA",
                table: "Services",
                column: "ServiceID");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Reviews__74BC79AE3419A5AC",
                table: "Reviews",
                column: "ReviewID");

            migrationBuilder.AddPrimaryKey(
                name: "PK__RepairSt__C8EE2043CE0D7330",
                table: "RepairStatus",
                column: "StatusID");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Notifica__20CF2E32DE412EF6",
                table: "Notifications",
                column: "NotificationID");

            migrationBuilder.AddPrimaryKey(
                name: "PK__GarageSc__9C8A5B690D2A35B7",
                table: "GarageSchedules",
                column: "ScheduleID");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Garages__5D8BEEB1FF3115B3",
                table: "Garages",
                column: "GarageID");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Appointm__8ECDFCA2221F534A",
                table: "Appointments",
                column: "AppointmentID");

            migrationBuilder.AddPrimaryKey(
                name: "PK__AdminAct__45F4A7F1428348E4",
                table: "AdminActivities",
                column: "ActivityID");

            migrationBuilder.CreateIndex(
                name: "IX_Services_GarageID",
                table: "Services",
                column: "GarageID");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ServiceID",
                table: "Appointments",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_VehicleID",
                table: "Appointments",
                column: "VehicleID");

            migrationBuilder.AddForeignKey(
                name: "FK__AdminActi__Admin__4BAC3F29",
                table: "AdminActivities",
                column: "AdminID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__Appointme__Garag__32E0915F",
                table: "Appointments",
                column: "GarageID",
                principalTable: "Garages",
                principalColumn: "GarageID");

            migrationBuilder.AddForeignKey(
                name: "FK__Appointme__Servi__33D4B598",
                table: "Appointments",
                column: "ServiceID",
                principalTable: "Services",
                principalColumn: "ServiceID");

            migrationBuilder.AddForeignKey(
                name: "FK__Appointme__Techn__34C8D9D1",
                table: "Appointments",
                column: "TechnicianID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__Appointme__UserI__30F848ED",
                table: "Appointments",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__Appointme__Vehic__31EC6D26",
                table: "Appointments",
                column: "VehicleID",
                principalTable: "Vehicles",
                principalColumn: "VehicleID");

            migrationBuilder.AddForeignKey(
                name: "FK__GarageSch__Garag__4F7CD00D",
                table: "GarageSchedules",
                column: "GarageID",
                principalTable: "Garages",
                principalColumn: "GarageID");

            migrationBuilder.AddForeignKey(
                name: "FK__Notificat__UserI__4222D4EF",
                table: "Notifications",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__RepairSta__Appoi__38996AB5",
                table: "RepairStatus",
                column: "AppointmentID",
                principalTable: "Appointments",
                principalColumn: "AppointmentID");

            migrationBuilder.AddForeignKey(
                name: "FK__Reviews__GarageI__3D5E1FD2",
                table: "Reviews",
                column: "GarageID",
                principalTable: "Garages",
                principalColumn: "GarageID");

            migrationBuilder.AddForeignKey(
                name: "FK__Reviews__UserID__3C69FB99",
                table: "Reviews",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__Services__Garage__2E1BDC42",
                table: "Services",
                column: "GarageID",
                principalTable: "Garages",
                principalColumn: "GarageID");

            migrationBuilder.AddForeignKey(
                name: "FK__Technical__Appoi__46E78A0C",
                table: "TechnicalReports",
                column: "AppointmentID",
                principalTable: "Appointments",
                principalColumn: "AppointmentID");

            migrationBuilder.AddForeignKey(
                name: "FK__Technical__Techn__47DBAE45",
                table: "TechnicalReports",
                column: "TechnicianID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__Vehicles__UserID__29572725",
                table: "Vehicles",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");
        }
    }
}
