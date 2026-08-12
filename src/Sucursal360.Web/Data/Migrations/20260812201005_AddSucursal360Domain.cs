using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Sucursal360.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSucursal360Domain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedBranchId",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    Provider = table.Column<int>(type: "INTEGER", nullable: false),
                    ExternalPlaceId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReviewCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SimulatedDataImports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    RowCount = table.Column<int>(type: "INTEGER", nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ImportedByUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    ImportedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulatedDataImports", x => x.Id);
                    table.CheckConstraint("CK_Imports_Period", "PeriodEnd >= PeriodStart");
                    table.ForeignKey(
                        name: "FK_SimulatedDataImports_AspNetUsers_ImportedByUserId",
                        column: x => x.ImportedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CorrelationId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Provider = table.Column<int>(type: "INTEGER", nullable: false),
                    BranchId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    FinishedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    HttpStatusCode = table.Column<int>(type: "INTEGER", nullable: true),
                    RecordsReceived = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    RecordsStored = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    ErrorCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    UserMessage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TechnicalMessage = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    TriggeredByUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationRuns", x => x.Id);
                    table.CheckConstraint("CK_IntegrationRuns_Counts", "RecordsReceived >= 0 AND RecordsStored >= 0");
                    table.ForeignKey(
                        name: "FK_IntegrationRuns_AspNetUsers_TriggeredByUserId",
                        column: x => x.TriggeredByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IntegrationRuns_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BranchId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Provider = table.Column<int>(type: "INTEGER", nullable: false),
                    ExternalReviewId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Rating = table.Column<byte>(type: "INTEGER", nullable: true),
                    Text = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    AuthorDisplayName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    Language = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    SourceUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    RetrievedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.CheckConstraint("CK_Reviews_Rating", "Rating IS NULL OR Rating BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_Reviews_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SimulatedOperationalMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BranchId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BusinessDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    NetSales = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TransactionCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 3, nullable: false, defaultValue: "NIO"),
                    DataOrigin = table.Column<int>(type: "INTEGER", nullable: false),
                    ImportId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulatedOperationalMetrics", x => x.Id);
                    table.CheckConstraint("CK_SimulatedMetrics_NetSales", "NetSales >= 0");
                    table.CheckConstraint("CK_SimulatedMetrics_Origin", "DataOrigin = 1");
                    table.CheckConstraint("CK_SimulatedMetrics_Transactions", "TransactionCount >= 0");
                    table.ForeignKey(
                        name: "FK_SimulatedOperationalMetrics_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SimulatedOperationalMetrics_SimulatedDataImports_ImportId",
                        column: x => x.ImportId,
                        principalTable: "SimulatedDataImports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BranchSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BranchId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Provider = table.Column<int>(type: "INTEGER", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Latitude = table.Column<decimal>(type: "TEXT", precision: 9, scale: 6, nullable: true),
                    Longitude = table.Column<decimal>(type: "TEXT", precision: 9, scale: 6, nullable: true),
                    BusinessStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    OpeningHoursJson = table.Column<string>(type: "TEXT", nullable: true),
                    Rating = table.Column<decimal>(type: "TEXT", precision: 2, scale: 1, nullable: true),
                    ReviewCount = table.Column<int>(type: "INTEGER", nullable: true),
                    RetrievedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    IntegrationRunId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchSnapshots", x => x.Id);
                    table.CheckConstraint("CK_BranchSnapshots_Latitude", "Latitude IS NULL OR Latitude BETWEEN -90 AND 90");
                    table.CheckConstraint("CK_BranchSnapshots_Longitude", "Longitude IS NULL OR Longitude BETWEEN -180 AND 180");
                    table.CheckConstraint("CK_BranchSnapshots_Rating", "Rating IS NULL OR Rating BETWEEN 1.0 AND 5.0");
                    table.CheckConstraint("CK_BranchSnapshots_ReviewCount", "ReviewCount IS NULL OR ReviewCount >= 0");
                    table.ForeignKey(
                        name: "FK_BranchSnapshots_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchSnapshots_IntegrationRuns_IntegrationRunId",
                        column: x => x.IntegrationRunId,
                        principalTable: "IntegrationRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReviewCategoryAssignments",
                columns: table => new
                {
                    ReviewId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReviewCategoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    AssignedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewCategoryAssignments", x => new { x.ReviewId, x.ReviewCategoryId });
                    table.ForeignKey(
                        name: "FK_ReviewCategoryAssignments_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReviewCategoryAssignments_ReviewCategories_ReviewCategoryId",
                        column: x => x.ReviewCategoryId,
                        principalTable: "ReviewCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReviewCategoryAssignments_Reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "Reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReviewCategoryAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReviewId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReviewCategoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Action = table.Column<int>(type: "INTEGER", nullable: false),
                    ChangedByUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    ChangedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewCategoryAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewCategoryAudits_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReviewCategoryAudits_ReviewCategories_ReviewCategoryId",
                        column: x => x.ReviewCategoryId,
                        principalTable: "ReviewCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReviewCategoryAudits_Reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "Reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "443f48d3-df72-45b9-84a1-1d28dc9f6bb4", "443f48d3-df72-45b9-84a1-1d28dc9f6bb4", "GerenteCorporativo", "GERENTECORPORATIVO" },
                    { "531e9b7f-d5b9-4695-bad0-034525ef5f64", "531e9b7f-d5b9-4695-bad0-034525ef5f64", "GerenteSucursal", "GERENTESUCURSAL" },
                    { "7b6f7be6-4d47-4b68-a3fa-7a104369b36a", "7b6f7be6-4d47-4b68-a3fa-7a104369b36a", "Administrador", "ADMINISTRADOR" }
                });

            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "ExternalPlaceId", "IsActive", "Name", "Provider", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-4111-8111-111111111111"), "SUC-001", new DateTimeOffset(new DateTime(2026, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "DEMO-SUC-001", true, "Cafe Horizonte Centro", 1, new DateTimeOffset(new DateTime(2026, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22222222-2222-4222-8222-222222222222"), "SUC-002", new DateTimeOffset(new DateTime(2026, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "DEMO-SUC-002", true, "Cafe Horizonte Carretera Sur", 1, new DateTimeOffset(new DateTime(2026, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("33333333-3333-4333-8333-333333333333"), "SUC-003", new DateTimeOffset(new DateTime(2026, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "DEMO-SUC-003", true, "Cafe Horizonte Galerias", 1, new DateTimeOffset(new DateTime(2026, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("44444444-4444-4444-8444-444444444444"), "SUC-004", new DateTimeOffset(new DateTime(2026, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "DEMO-SUC-004", true, "Cafe Horizonte Metrocentro", 1, new DateTimeOffset(new DateTime(2026, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("55555555-5555-4555-8555-555555555555"), "SUC-005", new DateTimeOffset(new DateTime(2026, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "DEMO-SUC-005", true, "Cafe Horizonte Las Colinas", 1, new DateTimeOffset(new DateTime(2026, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "ReviewCategories",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa1"), "SERVICIO", "Atencion, cortesia, conocimiento o actitud del personal", true, "Servicio" },
                    { new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa2"), "ESPERA", "Filas, demora en ordenar, preparacion o entrega", true, "Tiempo de espera" },
                    { new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa3"), "CALIDAD", "Sabor, temperatura, presentacion o consistencia", true, "Calidad del producto" },
                    { new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa4"), "LIMPIEZA", "Mesas, banos, utensilios o percepcion de higiene", true, "Limpieza" },
                    { new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa5"), "PRECIO", "Valor percibido, promociones o relacion precio-calidad", true, "Precio" },
                    { new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa6"), "INSTALACIONES", "Ambiente, espacio, estacionamiento, comodidad o ruido", true, "Instalaciones" },
                    { new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa7"), "OTROS", "Tema relevante que no corresponde a las categorias anteriores", true, "Otros" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AssignedBranchId",
                table: "AspNetUsers",
                column: "AssignedBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_Code",
                table: "Branches",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_IsActive",
                table: "Branches",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "UX_Branches_Provider_ExternalPlaceId",
                table: "Branches",
                columns: new[] { "Provider", "ExternalPlaceId" },
                unique: true,
                filter: "ExternalPlaceId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BranchSnapshots_Branch_Date",
                table: "BranchSnapshots",
                columns: new[] { "BranchId", "RetrievedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BranchSnapshots_IntegrationRunId",
                table: "BranchSnapshots",
                column: "IntegrationRunId");

            migrationBuilder.CreateIndex(
                name: "UX_BranchSnapshots_Branch_Provider_Date",
                table: "BranchSnapshots",
                columns: new[] { "BranchId", "Provider", "RetrievedAtUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationRuns_Branch_Date",
                table: "IntegrationRuns",
                columns: new[] { "BranchId", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationRuns_CorrelationId",
                table: "IntegrationRuns",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationRuns_Status_Date",
                table: "IntegrationRuns",
                columns: new[] { "Status", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationRuns_TriggeredByUserId",
                table: "IntegrationRuns",
                column: "TriggeredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewCategories_Code",
                table: "ReviewCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewCategoryAssignments_AssignedByUserId",
                table: "ReviewCategoryAssignments",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewCategoryAssignments_Category",
                table: "ReviewCategoryAssignments",
                column: "ReviewCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewCategoryAudits_ChangedByUserId",
                table: "ReviewCategoryAudits",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewCategoryAudits_Review_Date",
                table: "ReviewCategoryAudits",
                columns: new[] { "ReviewId", "ChangedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewCategoryAudits_ReviewCategoryId",
                table: "ReviewCategoryAudits",
                column: "ReviewCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_Branch_Published",
                table: "Reviews",
                columns: new[] { "BranchId", "PublishedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_Branch_Rating",
                table: "Reviews",
                columns: new[] { "BranchId", "Rating" });

            migrationBuilder.CreateIndex(
                name: "UX_Reviews_Provider_ExternalId",
                table: "Reviews",
                columns: new[] { "Provider", "ExternalReviewId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SimulatedDataImports_ImportedByUserId",
                table: "SimulatedDataImports",
                column: "ImportedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SimulatedMetrics_Import",
                table: "SimulatedOperationalMetrics",
                column: "ImportId");

            migrationBuilder.CreateIndex(
                name: "UX_SimulatedMetrics_Branch_Date",
                table: "SimulatedOperationalMetrics",
                columns: new[] { "BranchId", "BusinessDate" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Branches_AssignedBranchId",
                table: "AspNetUsers",
                column: "AssignedBranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Branches_AssignedBranchId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "BranchSnapshots");

            migrationBuilder.DropTable(
                name: "ReviewCategoryAssignments");

            migrationBuilder.DropTable(
                name: "ReviewCategoryAudits");

            migrationBuilder.DropTable(
                name: "SimulatedOperationalMetrics");

            migrationBuilder.DropTable(
                name: "IntegrationRuns");

            migrationBuilder.DropTable(
                name: "ReviewCategories");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "SimulatedDataImports");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_AssignedBranchId",
                table: "AspNetUsers");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "443f48d3-df72-45b9-84a1-1d28dc9f6bb4");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "531e9b7f-d5b9-4695-bad0-034525ef5f64");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7b6f7be6-4d47-4b68-a3fa-7a104369b36a");

            migrationBuilder.DropColumn(
                name: "AssignedBranchId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");
        }
    }
}
