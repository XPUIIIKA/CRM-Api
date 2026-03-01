using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    surname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    patronymic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_companies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entity_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entity_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_status_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_status_histories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: true),
                    current_status_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    access_level = table.Column<int>(type: "integer", nullable: false),
                    permissions = table.Column<int[]>(type: "integer[]", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "statuses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "system_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    login = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    is_root = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_system_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                    table.CheckConstraint("ck_products_price_non_negative", "\"price\" >= 0");
                    table.ForeignKey(
                        name: "fk_products_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "order_products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_products", x => x.id);
                    table.CheckConstraint("ck_order_products_price_non_negative", "\"price\" >= 0");
                    table.CheckConstraint("ck_order_products_quantity_positive", "\"quantity\" > 0");
                    table.ForeignKey(
                        name: "fk_order_products_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "access_level", "company_id", "created_at", "created_by", "name", "permissions", "updated_at" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-e5f6-4a5b-bc6d-7e8f9a0b1c2d"), 100, new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "CompanyOwner", new[] { 1, 2, 3, 10, 11, 12, 13, 14, 20, 21, 22, 26, 27, 30, 31, 40, 50 }, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2c3d4e5-f6a7-5b6c-cd7e-8f9a0b1c2d3e"), 50, new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "Manager", new[] { 10, 11, 21, 22 }, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c3d4e5f6-a7b8-6c7d-de8f-9a0b1c2d3e4f"), 10, new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "Employee", new[] { 10 }, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "statuses",
                columns: new[] { "id", "company_id", "created_at", "created_by", "name", "updated_at" },
                values: new object[,]
                {
                    { new Guid("a4b5c6d7-e8f9-4a5b-1c2d-3e4f5a6b7c8d"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "Cancelled", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d1e2f3a4-b5c6-4d5e-8f9a-0b1c2d3e4f5a"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "Draft", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("e2f3a4b5-c6d7-4e5f-9a0b-1c2d3e4f5a6b"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "Active", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("f3a4b5c6-d7e8-4f5a-0b1c-2d3e4f5a6b7c"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "Completed", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "ix_categories_company_id_name",
                table: "categories",
                columns: new[] { "company_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_clients_company_id_email",
                table: "clients",
                columns: new[] { "company_id", "email" },
                unique: true,
                filter: "\"email\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_companies_name",
                table: "companies",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_entity_tags_company_id",
                table: "entity_tags",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_entity_tags_entity_id_tag_id",
                table: "entity_tags",
                columns: new[] { "entity_id", "tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_products_company_id",
                table: "order_products",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_products_order_id_product_id",
                table: "order_products",
                columns: new[] { "order_id", "product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_status_histories_company_id",
                table: "order_status_histories",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_assigned_manager_id",
                table: "orders",
                column: "assigned_manager_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_client_id",
                table: "orders",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_company_id",
                table: "orders",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_company_id_name",
                table: "products",
                columns: new[] { "company_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_roles_company_id_name",
                table: "roles",
                columns: new[] { "company_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_statuses_company_id_name",
                table: "statuses",
                columns: new[] { "company_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_system_users_email",
                table: "system_users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_system_users_login",
                table: "system_users",
                column: "login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tags_company_id_name",
                table: "tags",
                columns: new[] { "company_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_company_id_email",
                table: "users",
                columns: new[] { "company_id", "email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_role_id",
                table: "users",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "companies");

            migrationBuilder.DropTable(
                name: "entity_tags");

            migrationBuilder.DropTable(
                name: "order_products");

            migrationBuilder.DropTable(
                name: "order_status_histories");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "statuses");

            migrationBuilder.DropTable(
                name: "system_users");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
