using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace O2Connect.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Username = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    NormalizedUsername = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Scopes = table.Column<string[]>(type: "text[]", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OwnerId = table.Column<string>(type: "character varying(100)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClientSecret = table.Column<string>(type: "text", nullable: true),
                    JsonWebKeysUri = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RequiresSecret = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresPkce = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresConsent = table.Column<bool>(type: "boolean", nullable: false),
                    AllowPlainPkce = table.Column<bool>(type: "boolean", nullable: false),
                    AllowPar = table.Column<bool>(type: "boolean", nullable: false),
                    RedirectUris = table.Column<string[]>(type: "text[]", nullable: false),
                    AllowedGrantTypes = table.Column<string[]>(type: "text[]", nullable: false),
                    AllowedScopes = table.Column<string[]>(type: "text[]", nullable: false),
                    AllowedAuthenticationMethods = table.Column<string[]>(type: "text[]", nullable: false),
                    AllowedResponseTypes = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerId1 = table.Column<string>(type: "character varying(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clients_users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_clients_users_OwnerId1",
                        column: x => x.OwnerId1,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "authorization_codes",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ClientId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RedirectUri = table.Column<string>(type: "text", nullable: false),
                    CodeChallenge = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CodeChallengeMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Scopes = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SubjectId = table.Column<string>(type: "text", nullable: true),
                    Nonce = table.Column<string>(type: "text", nullable: true),
                    IsConsumed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_authorization_codes", x => x.Code);
                    table.ForeignKey(
                        name: "FK_authorization_codes_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_authorization_codes_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "authorization_sessions",
                columns: table => new
                {
                    SessionId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RequestUriCode = table.Column<string>(type: "text", nullable: true),
                    ClientId = table.Column<string>(type: "character varying(100)", nullable: false),
                    ClientDisplayName = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "character varying(100)", nullable: true),
                    UserDisplayName = table.Column<string>(type: "text", nullable: true),
                    RequestedScopes = table.Column<string[]>(type: "text[]", nullable: true),
                    MissingScopes = table.Column<string[]>(type: "text[]", nullable: true),
                    Request = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_authorization_sessions", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_authorization_sessions_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_authorization_sessions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "device_authorizations",
                columns: table => new
                {
                    DeviceCodeHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserCodeHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ClientId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Scopes = table.Column<string[]>(type: "text[]", nullable: false),
                    UserId = table.Column<string>(type: "character varying(100)", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AuthorizedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConsumedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeniedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PollCount = table.Column<int>(type: "integer", nullable: false),
                    Interval = table.Column<int>(type: "integer", nullable: false),
                    LastPollAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_authorizations", x => x.DeviceCodeHash);
                    table.ForeignKey(
                        name: "FK_device_authorizations_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_device_authorizations_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "par_entries",
                columns: table => new
                {
                    RequestUriCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ClientId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RedirectUri = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Scopes = table.Column<string[]>(type: "text[]", nullable: false),
                    ResponseType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CodeChallenge = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CodeChallengeMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    State = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ConsumedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_par_entries", x => x.RequestUriCode);
                    table.ForeignKey(
                        name: "FK_par_entries_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    Token = table.Column<string>(type: "text", nullable: false),
                    SessionId = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    ReplacedByToken = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClientId = table.Column<string>(type: "character varying(100)", nullable: false),
                    Subject = table.Column<string>(type: "character varying(100)", nullable: false),
                    Scopes = table.Column<string[]>(type: "text[]", nullable: false),
                    ConsumedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Token);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_refresh_tokens_ReplacedByToken",
                        column: x => x.ReplacedByToken,
                        principalTable: "refresh_tokens",
                        principalColumn: "Token",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_Subject",
                        column: x => x.Subject,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_consents",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(100)", nullable: false),
                    ClientId = table.Column<string>(type: "character varying(100)", nullable: false),
                    GrantedScopes = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_consents", x => new { x.UserId, x.ClientId });
                    table.ForeignKey(
                        name: "FK_user_consents_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_consents_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_authorization_codes_ClientId",
                table: "authorization_codes",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_authorization_codes_ExpiresAt",
                table: "authorization_codes",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_authorization_codes_UserId",
                table: "authorization_codes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_authorization_sessions_ClientId",
                table: "authorization_sessions",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_authorization_sessions_ExpiresAt",
                table: "authorization_sessions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_authorization_sessions_Status",
                table: "authorization_sessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_authorization_sessions_UserId",
                table: "authorization_sessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_clients_NormalizedName",
                table: "clients",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clients_OwnerId",
                table: "clients",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_clients_OwnerId1",
                table: "clients",
                column: "OwnerId1");

            migrationBuilder.CreateIndex(
                name: "IX_clients_Status",
                table: "clients",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_device_authorizations_ClientId",
                table: "device_authorizations",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_device_authorizations_ExpiresAtUtc",
                table: "device_authorizations",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_device_authorizations_UserCodeHash",
                table: "device_authorizations",
                column: "UserCodeHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_device_authorizations_UserId",
                table: "device_authorizations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_par_entries_ClientId",
                table: "par_entries",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_par_entries_ExpiresAt",
                table: "par_entries",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_ClientId",
                table: "refresh_tokens",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_ExpiresAt",
                table: "refresh_tokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_ReplacedByToken",
                table: "refresh_tokens",
                column: "ReplacedByToken");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_SessionId",
                table: "refresh_tokens",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_Subject",
                table: "refresh_tokens",
                column: "Subject");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_Subject_ClientId",
                table: "refresh_tokens",
                columns: new[] { "Subject", "ClientId" });

            migrationBuilder.CreateIndex(
                name: "IX_user_consents_ClientId",
                table: "user_consents",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_NormalizedUsername",
                table: "users",
                column: "NormalizedUsername",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "authorization_codes");

            migrationBuilder.DropTable(
                name: "authorization_sessions");

            migrationBuilder.DropTable(
                name: "device_authorizations");

            migrationBuilder.DropTable(
                name: "par_entries");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "user_consents");

            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
