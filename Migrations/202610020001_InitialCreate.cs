using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MyChat.Data;

namespace MyChat.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202610020001_InitialCreate")]
public class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder m) => m.Sql("""
CREATE TABLE Roles (Id TEXT NOT NULL CONSTRAINT PK_Roles PRIMARY KEY, Name TEXT NULL, NormalizedName TEXT NULL, ConcurrencyStamp TEXT NULL);
CREATE TABLE UserRoles (UserId TEXT NOT NULL, RoleId TEXT NOT NULL, CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId));
CREATE TABLE Users (Id TEXT NOT NULL CONSTRAINT PK_Users PRIMARY KEY, BirthDate TEXT NOT NULL, AvatarPath TEXT NOT NULL, IsBlocked INTEGER NOT NULL, UserName TEXT NULL, NormalizedUserName TEXT NULL, Email TEXT NULL, NormalizedEmail TEXT NULL, EmailConfirmed INTEGER NOT NULL, PasswordHash TEXT NULL, SecurityStamp TEXT NULL, ConcurrencyStamp TEXT NULL, PhoneNumber TEXT NULL, PhoneNumberConfirmed INTEGER NOT NULL, TwoFactorEnabled INTEGER NOT NULL, LockoutEnd TEXT NULL, LockoutEnabled INTEGER NOT NULL, AccessFailedCount INTEGER NOT NULL);
CREATE TABLE ChatMessages (Id INTEGER NOT NULL CONSTRAINT PK_ChatMessages PRIMARY KEY AUTOINCREMENT, Text TEXT NOT NULL, SentAt TEXT NOT NULL, UserId TEXT NOT NULL, CONSTRAINT FK_ChatMessages_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE);
CREATE INDEX IX_ChatMessages_UserId ON ChatMessages (UserId);
CREATE UNIQUE INDEX IX_Roles_NormalizedName ON Roles (NormalizedName);
CREATE UNIQUE INDEX IX_Users_NormalizedEmail ON Users (NormalizedEmail);
CREATE UNIQUE INDEX IX_Users_NormalizedUserName ON Users (NormalizedUserName);
""");

    protected override void Down(MigrationBuilder m) => m.Sql("DROP TABLE ChatMessages; DROP TABLE UserRoles; DROP TABLE Roles; DROP TABLE Users;");
}
