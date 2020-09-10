using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace stranitza.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"SET default_storage_engine=InnoDB;");
            migrationBuilder.Sql(@"ALTER DATABASE CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    Description = table.Column<string>(maxLength: 1024, nullable: true),
                    FirstName = table.Column<string>(maxLength: 255, nullable: true),
                    LastName = table.Column<string>(maxLength: 255, nullable: true),
                    IsAuthor = table.Column<bool>(nullable: true),
                    DisplayNameType = table.Column<int>(nullable: true),
                    DisplayEmail = table.Column<bool>(nullable: true),
                    AvatarType = table.Column<int>(nullable: true),
                    FacebookAvatarPath = table.Column<string>(nullable: true),
                    TwitterAvatarPath = table.Column<string>(nullable: true),
                    GoogleAvatarPath = table.Column<string>(nullable: true),
                    InternalAvatarPath = table.Column<string>(nullable: true),
                    LastUpdated = table.Column<DateTime>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StranitzaCategories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 127, nullable: false),
                    Description = table.Column<string>(maxLength: 1024, nullable: true),
                    LastUpdated = table.Column<DateTime>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StranitzaCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StranitzaFiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FileName = table.Column<string>(maxLength: 127, nullable: false),
                    MimeType = table.Column<string>(maxLength: 128, nullable: false),
                    Title = table.Column<string>(maxLength: 256, nullable: false),
                    Extension = table.Column<string>(maxLength: 32, nullable: false),
                    FilePath = table.Column<string>(maxLength: 512, nullable: false),
                    ThumbPath = table.Column<string>(maxLength: 512, nullable: true),
                    LastUpdated = table.Column<DateTime>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StranitzaFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StranitzaEPages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FirstName = table.Column<string>(maxLength: 255, nullable: false),
                    LastName = table.Column<string>(maxLength: 255, nullable: false),
                    Title = table.Column<string>(maxLength: 512, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    ReleaseNumber = table.Column<int>(nullable: false),
                    ReleaseYear = table.Column<int>(nullable: false),
                    Description = table.Column<string>(maxLength: 1024, nullable: true),
                    Notes = table.Column<string>(maxLength: 1024, nullable: true),
                    IsTranslation = table.Column<bool>(nullable: false),
                    CategoryId = table.Column<int>(nullable: false),
                    AuthorId = table.Column<string>(maxLength: 127, nullable: true),
                    UploaderId = table.Column<string>(maxLength: 127, nullable: false),
                    SourceId = table.Column<int>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StranitzaEPages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StranitzaEPages_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StranitzaEPages_StranitzaCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "StranitzaCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StranitzaEPages_AspNetUsers_UploaderId",
                        column: x => x.UploaderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StranitzaIssues",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IssueNumber = table.Column<int>(nullable: false),
                    ReleaseNumber = table.Column<int>(nullable: false),
                    ReleaseYear = table.Column<int>(nullable: false),
                    Description = table.Column<string>(maxLength: 1024, nullable: true),
                    IsAvailable = table.Column<bool>(nullable: false),
                    PagesCount = table.Column<int>(nullable: false),
                    ViewCount = table.Column<int>(nullable: false),
                    DownloadCount = table.Column<int>(nullable: false),
                    AvailablePages = table.Column<string>(type: "VARCHAR(512)", nullable: true),
                    Tags = table.Column<string>(type: "VARCHAR(256)", nullable: true),
                    PdfFilePreviewId = table.Column<int>(nullable: true),
                    PdfFileReducedId = table.Column<int>(nullable: true),
                    ZipFileId = table.Column<int>(nullable: true),
                    LastUpdated = table.Column<DateTime>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StranitzaIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StranitzaIssues_StranitzaFiles_PdfFilePreviewId",
                        column: x => x.PdfFilePreviewId,
                        principalTable: "StranitzaFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StranitzaIssues_StranitzaFiles_PdfFileReducedId",
                        column: x => x.PdfFileReducedId,
                        principalTable: "StranitzaFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StranitzaIssues_StranitzaFiles_ZipFileId",
                        column: x => x.ZipFileId,
                        principalTable: "StranitzaFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StranitzaPosts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(maxLength: 512, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(maxLength: 1024, nullable: true),
                    EditorsPick = table.Column<bool>(nullable: false),
                    ViewCount = table.Column<int>(nullable: false),
                    Origin = table.Column<string>(maxLength: 1024, nullable: false),
                    UploaderId = table.Column<string>(maxLength: 127, nullable: false),
                    ImageFileId = table.Column<int>(nullable: true),
                    LastUpdated = table.Column<DateTime>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StranitzaPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StranitzaPosts_StranitzaFiles_ImageFileId",
                        column: x => x.ImageFileId,
                        principalTable: "StranitzaFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StranitzaPosts_AspNetUsers_UploaderId",
                        column: x => x.UploaderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StranitzaPages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PageNumber = table.Column<int>(nullable: false),
                    SlideNumber = table.Column<int>(nullable: false),
                    IsAvailable = table.Column<bool>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    PageFileId = table.Column<int>(nullable: false),
                    IssueId = table.Column<int>(nullable: false),
                    LastUpdated = table.Column<DateTime>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StranitzaPages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StranitzaPages_StranitzaIssues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "StranitzaIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StranitzaPages_StranitzaFiles_PageFileId",
                        column: x => x.PageFileId,
                        principalTable: "StranitzaFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StranitzaSources",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FirstName = table.Column<string>(maxLength: 255, nullable: true),
                    LastName = table.Column<string>(maxLength: 255, nullable: true),
                    Origin = table.Column<string>(maxLength: 512, nullable: false),
                    Title = table.Column<string>(maxLength: 512, nullable: true),
                    Description = table.Column<string>(maxLength: 1024, nullable: true),
                    Notes = table.Column<string>(maxLength: 1024, nullable: true),
                    ReleaseNumber = table.Column<int>(nullable: false),
                    ReleaseYear = table.Column<int>(nullable: false),
                    Pages = table.Column<string>(maxLength: 64, nullable: true),
                    StartingPage = table.Column<int>(nullable: false),
                    IsTranslation = table.Column<bool>(nullable: false),
                    Uploader = table.Column<string>(maxLength: 127, nullable: false),
                    CategoryId = table.Column<int>(nullable: false),
                    IssueId = table.Column<int>(nullable: true),
                    EPageId = table.Column<int>(nullable: true),
                    AuthorId = table.Column<string>(maxLength: 127, nullable: true),
                    LastUpdated = table.Column<DateTime>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StranitzaSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StranitzaSources_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StranitzaSources_StranitzaCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "StranitzaCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StranitzaSources_StranitzaEPages_EPageId",
                        column: x => x.EPageId,
                        principalTable: "StranitzaEPages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StranitzaSources_StranitzaIssues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "StranitzaIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "StranitzaComments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Content = table.Column<string>(maxLength: 1024, nullable: false),
                    Note = table.Column<string>(maxLength: 1024, nullable: true),
                    AuthorId = table.Column<string>(maxLength: 127, nullable: false),
                    ModeratorId = table.Column<string>(maxLength: 127, nullable: true),
                    ParentId = table.Column<int>(nullable: true),
                    IssueId = table.Column<int>(nullable: true),
                    PostId = table.Column<int>(nullable: true),
                    EPageId = table.Column<int>(nullable: true),
                    LastUpdated = table.Column<DateTime>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StranitzaComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StranitzaComments_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StranitzaComments_StranitzaEPages_EPageId",
                        column: x => x.EPageId,
                        principalTable: "StranitzaEPages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StranitzaComments_StranitzaIssues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "StranitzaIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StranitzaComments_AspNetUsers_ModeratorId",
                        column: x => x.ModeratorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StranitzaComments_StranitzaComments_ParentId",
                        column: x => x.ParentId,
                        principalTable: "StranitzaComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StranitzaComments_StranitzaPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "StranitzaPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaComments_AuthorId",
                table: "StranitzaComments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaComments_EPageId",
                table: "StranitzaComments",
                column: "EPageId");

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaComments_IssueId",
                table: "StranitzaComments",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaComments_ModeratorId",
                table: "StranitzaComments",
                column: "ModeratorId");

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaComments_ParentId",
                table: "StranitzaComments",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaComments_PostId",
                table: "StranitzaComments",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaEPages_AuthorId",
                table: "StranitzaEPages",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaEPages_CategoryId",
                table: "StranitzaEPages",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaEPages_UploaderId",
                table: "StranitzaEPages",
                column: "UploaderId");

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaIssues_IssueNumber",
                table: "StranitzaIssues",
                column: "IssueNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaIssues_PdfFilePreviewId",
                table: "StranitzaIssues",
                column: "PdfFilePreviewId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaIssues_PdfFileReducedId",
                table: "StranitzaIssues",
                column: "PdfFileReducedId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaIssues_ZipFileId",
                table: "StranitzaIssues",
                column: "ZipFileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaIssues_ReleaseYear_ReleaseNumber",
                table: "StranitzaIssues",
                columns: new[] { "ReleaseYear", "ReleaseNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaPages_IssueId",
                table: "StranitzaPages",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaPages_PageFileId",
                table: "StranitzaPages",
                column: "PageFileId");

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaPages_IssueId_PageNumber",
                table: "StranitzaPages",
                columns: new[] { "IssueId", "PageNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaPosts_ImageFileId",
                table: "StranitzaPosts",
                column: "ImageFileId");

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaPosts_UploaderId",
                table: "StranitzaPosts",
                column: "UploaderId");

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaSources_AuthorId",
                table: "StranitzaSources",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaSources_CategoryId",
                table: "StranitzaSources",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaSources_EPageId",
                table: "StranitzaSources",
                column: "EPageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StranitzaSources_IssueId",
                table: "StranitzaSources",
                column: "IssueId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "StranitzaComments");

            migrationBuilder.DropTable(
                name: "StranitzaPages");

            migrationBuilder.DropTable(
                name: "StranitzaSources");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "StranitzaPosts");

            migrationBuilder.DropTable(
                name: "StranitzaEPages");

            migrationBuilder.DropTable(
                name: "StranitzaIssues");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "StranitzaCategories");

            migrationBuilder.DropTable(
                name: "StranitzaFiles");
        }
    }
}
