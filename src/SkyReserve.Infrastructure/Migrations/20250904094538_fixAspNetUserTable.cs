using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyReserve.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixAspNetUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key constraints
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshToken_AspNetUsers_ApplicationUserId",
                table: "RefreshToken");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_AspNetUsers_UserId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_UserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_AspNetUsers_UserId",
                table: "Reviews");

            // Drop primary key constraint first
            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers");

            // Drop indexes
            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "AspNetUsers");

            // Create temporary table with correct column order
            migrationBuilder.Sql(@"
                CREATE TABLE AspNetUsers_temp (
                    ""Id"" text NOT NULL,
                    ""AccessFailedCount"" integer NOT NULL,
                    ""ConcurrencyStamp"" text,
                    ""Email"" character varying(256),
                    ""EmailConfirmed"" boolean NOT NULL,
                    ""LockoutEnabled"" boolean NOT NULL,
                    ""LockoutEnd"" timestamp with time zone,
                    ""NormalizedEmail"" character varying(256),
                    ""NormalizedUserName"" character varying(256),
                    ""PasswordHash"" text,
                    ""PhoneNumber"" text,
                    ""PhoneNumberConfirmed"" boolean NOT NULL,
                    ""SecurityStamp"" text,
                    ""TwoFactorEnabled"" boolean NOT NULL,
                    ""UserName"" character varying(256),
                    ""FirstName"" text NOT NULL,
                    ""LastName"" text NOT NULL,
                    ""DateOfBirth"" timestamp with time zone NOT NULL,
                    ""ProfilePictureUrl"" text,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone NOT NULL,
                    ""IsDisabled"" boolean NOT NULL,
                    ""RoleType"" text NOT NULL,
                    CONSTRAINT ""PK_AspNetUsers_temp"" PRIMARY KEY (""Id"")
                );
            ");

            // Copy data from old table to new table
            migrationBuilder.Sql(@"
                INSERT INTO AspNetUsers_temp (
                    ""Id"", ""AccessFailedCount"", ""ConcurrencyStamp"", ""Email"", ""EmailConfirmed"",
                    ""LockoutEnabled"", ""LockoutEnd"", ""NormalizedEmail"", ""NormalizedUserName"",
                    ""PasswordHash"", ""PhoneNumber"", ""PhoneNumberConfirmed"", ""SecurityStamp"",
                    ""TwoFactorEnabled"", ""UserName"", ""FirstName"", ""LastName"", ""DateOfBirth"",
                    ""ProfilePictureUrl"", ""CreatedAt"", ""UpdatedAt"", ""IsDisabled"", ""RoleType""
                )
                SELECT 
                    ""Id"", ""AccessFailedCount"", ""ConcurrencyStamp"", ""Email"", ""EmailConfirmed"",
                    ""LockoutEnabled"", ""LockoutEnd"", ""NormalizedEmail"", ""NormalizedUserName"",
                    ""PasswordHash"", ""PhoneNumber"", ""PhoneNumberConfirmed"", ""SecurityStamp"",
                    ""TwoFactorEnabled"", ""UserName"", ""FirstName"", ""LastName"", ""DateOfBirth"",
                    ""ProfilePictureUrl"", ""CreatedAt"", ""UpdatedAt"", ""IsDisabled"", ""RoleType""
                FROM ""AspNetUsers"";
            ");

            // Drop old table
            migrationBuilder.DropTable("AspNetUsers");

            // Rename temp table to original name and drop temp constraint
            migrationBuilder.Sql(@"
                ALTER TABLE AspNetUsers_temp RENAME TO ""AspNetUsers"";
                ALTER TABLE ""AspNetUsers"" RENAME CONSTRAINT ""PK_AspNetUsers_temp"" TO ""PK_AspNetUsers"";
            ");

            // Recreate indexes
            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            // Recreate foreign key constraints
            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshToken_AspNetUsers_ApplicationUserId",
                table: "RefreshToken",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_AspNetUsers_UserId",
                table: "Bookings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_AspNetUsers_UserId",
                table: "Reviews",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
