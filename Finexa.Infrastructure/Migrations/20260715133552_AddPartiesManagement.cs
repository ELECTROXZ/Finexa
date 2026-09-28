using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finexa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPartiesManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Aadhaar",
                table: "Parties",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                table: "Parties",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                table: "Parties",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AlternativePhone",
                table: "Parties",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BusinessRegistrationNumber",
                table: "Parties",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Parties",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactPerson",
                table: "Parties",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Parties",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Parties",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Parties",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Parties",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Parties",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Parties",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Pincode",
                table: "Parties",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreferredPaymentMethod",
                table: "Parties",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "ProfileImage",
                table: "Parties",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Parties",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Parties",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Parties",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Parties",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Parties",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PartyActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    ChangedBy = table.Column<string>(type: "TEXT", nullable: false),
                    Details = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartyActivityLogs_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartyAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartyId = table.Column<int>(type: "INTEGER", nullable: false),
                    AddressLine1 = table.Column<string>(type: "TEXT", nullable: false),
                    AddressLine2 = table.Column<string>(type: "TEXT", nullable: false),
                    City = table.Column<string>(type: "TEXT", nullable: false),
                    State = table.Column<string>(type: "TEXT", nullable: false),
                    Country = table.Column<string>(type: "TEXT", nullable: false),
                    Pincode = table.Column<string>(type: "TEXT", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartyAddresses_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartyContacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Designation = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartyContacts_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartyDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartyId = table.Column<int>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartyDocuments_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartyNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartyNotes_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartyTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReferenceType = table.Column<string>(type: "TEXT", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    TransactionType = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartyTransactions_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartyActivityLogs_PartyId",
                table: "PartyActivityLogs",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyAddresses_PartyId",
                table: "PartyAddresses",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContacts_PartyId",
                table: "PartyContacts",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyDocuments_PartyId",
                table: "PartyDocuments",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyNotes_PartyId",
                table: "PartyNotes",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyTransactions_PartyId",
                table: "PartyTransactions",
                column: "PartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartyActivityLogs");

            migrationBuilder.DropTable(
                name: "PartyAddresses");

            migrationBuilder.DropTable(
                name: "PartyContacts");

            migrationBuilder.DropTable(
                name: "PartyDocuments");

            migrationBuilder.DropTable(
                name: "PartyNotes");

            migrationBuilder.DropTable(
                name: "PartyTransactions");

            migrationBuilder.DropColumn(
                name: "Aadhaar",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "AddressLine1",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "AlternativePhone",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "BusinessRegistrationNumber",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "ContactPerson",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "Pincode",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "PreferredPaymentMethod",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "ProfileImage",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Parties");
        }
    }
}
