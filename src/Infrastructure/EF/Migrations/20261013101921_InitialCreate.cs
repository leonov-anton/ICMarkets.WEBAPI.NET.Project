using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ICMarkets.Infrastructure.EF.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "chains",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chains", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "coins",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    short_name = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_coins", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "coin_chains",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    coin_id = table.Column<int>(type: "integer", nullable: false),
                    chain_id = table.Column<int>(type: "integer", nullable: false),
                    family = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_coin_chains", x => x.id);
                    table.ForeignKey(
                        name: "fk_coin_chains_chains_chain_id",
                        column: x => x.chain_id,
                        principalTable: "chains",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_coin_chains_coins_coin_id",
                        column: x => x.coin_id,
                        principalTable: "coins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "coin_chain_infos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    coin_chain_id = table.Column<int>(type: "integer", nullable: false),
                    height = table.Column<long>(type: "bigint", nullable: false),
                    hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    previous_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    peer_count = table.Column<int>(type: "integer", nullable: false),
                    unconfirmed_count = table.Column<int>(type: "integer", nullable: false),
                    last_fork_height = table.Column<long>(type: "bigint", nullable: false),
                    last_fork_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    family = table.Column<int>(type: "integer", nullable: false),
                    high_gas_price = table.Column<long>(type: "bigint", nullable: true),
                    medium_gas_price = table.Column<long>(type: "bigint", nullable: true),
                    low_gas_price = table.Column<long>(type: "bigint", nullable: true),
                    high_priority_fee = table.Column<long>(type: "bigint", nullable: true),
                    medium_priority_fee = table.Column<long>(type: "bigint", nullable: true),
                    low_priority_fee = table.Column<long>(type: "bigint", nullable: true),
                    base_fee = table.Column<long>(type: "bigint", nullable: true),
                    high_fee_per_kb = table.Column<long>(type: "bigint", nullable: true),
                    medium_fee_per_kb = table.Column<long>(type: "bigint", nullable: true),
                    low_fee_per_kb = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_coin_chain_infos", x => x.id);
                    table.ForeignKey(
                        name: "fk_coin_chain_infos_coin_chains_coin_chain_id",
                        column: x => x.coin_chain_id,
                        principalTable: "coin_chains",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "chains",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Main" },
                    { 2, "Testnet3" },
                    { 3, "Test" }
                });

            migrationBuilder.InsertData(
                table: "coins",
                columns: new[] { "id", "name", "short_name" },
                values: new object[,]
                {
                    { 1, "Bitcoin", "BTC" },
                    { 2, "Dash", "DASH" },
                    { 3, "Dogecoin", "DOGE" },
                    { 4, "Litecoin", "LTC" },
                    { 5, "BlockCypher", "BCY" },
                    { 6, "Ethereum", "ETH" }
                });

            migrationBuilder.InsertData(
                table: "coin_chains",
                columns: new[] { "id", "chain_id", "coin_id", "family" },
                values: new object[,]
                {
                    { 1, 1, 1, 1 },
                    { 2, 2, 1, 1 },
                    { 3, 1, 2, 1 },
                    { 4, 1, 3, 1 },
                    { 5, 1, 4, 1 },
                    { 6, 3, 5, 1 },
                    { 7, 1, 6, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_chains_name",
                table: "chains",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_coin_chain_infos_coin_chain_id_created_at",
                table: "coin_chain_infos",
                columns: new[] { "coin_chain_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_coin_chains_chain_id",
                table: "coin_chains",
                column: "chain_id");

            migrationBuilder.CreateIndex(
                name: "ix_coin_chains_coin_id_chain_id",
                table: "coin_chains",
                columns: new[] { "coin_id", "chain_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_coins_name",
                table: "coins",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_coins_short_name",
                table: "coins",
                column: "short_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "coin_chain_infos");

            migrationBuilder.DropTable(
                name: "coin_chains");

            migrationBuilder.DropTable(
                name: "chains");

            migrationBuilder.DropTable(
                name: "coins");
        }
    }
}
