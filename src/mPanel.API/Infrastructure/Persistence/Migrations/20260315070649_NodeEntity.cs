using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mPanel.API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NodeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TokenPrefix = table.Column<string>(type: "text", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    Alias = table.Column<string>(type: "text", nullable: true),
                    SftpPort = table.Column<int>(type: "integer", nullable: false),
                    SftpAlias = table.Column<string>(type: "text", nullable: true),
                    MaxMemoryMb = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    MaxDiskMb = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    OsName = table.Column<string>(type: "text", nullable: true),
                    Architecture = table.Column<string>(type: "text", nullable: true),
                    CpuCores = table.Column<int>(type: "integer", nullable: true),
                    TotalMemoryMb = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    TotalDiskMb = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    IsMaintenanceMode = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    HandshakeError = table.Column<string>(type: "text", nullable: true),
                    LastHeartbeat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastHeartbeatCpuUsage = table.Column<double>(type: "double precision", nullable: true),
                    LastHeartbeatMemoryMb = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    LastHeartbeatDiskMb = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_Name",
                table: "Nodes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_TokenPrefix",
                table: "Nodes",
                column: "TokenPrefix",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Nodes");
        }
    }
}
