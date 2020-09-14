CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(95) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
);

SET default_storage_engine=InnoDB;

ALTER DATABASE CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE `AspNetRoles` (
    `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Name` varchar(256) CHARACTER SET utf8mb4 NULL,
    `NormalizedName` varchar(256) CHARACTER SET utf8mb4 NULL,
    `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_AspNetRoles` PRIMARY KEY (`Id`)
);

CREATE TABLE `AspNetUsers` (
    `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `UserName` varchar(256) CHARACTER SET utf8mb4 NULL,
    `NormalizedUserName` varchar(256) CHARACTER SET utf8mb4 NULL,
    `Email` varchar(256) CHARACTER SET utf8mb4 NULL,
    `NormalizedEmail` varchar(256) CHARACTER SET utf8mb4 NULL,
    `EmailConfirmed` tinyint(1) NOT NULL,
    `PasswordHash` longtext CHARACTER SET utf8mb4 NULL,
    `SecurityStamp` longtext CHARACTER SET utf8mb4 NULL,
    `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 NULL,
    `PhoneNumber` longtext CHARACTER SET utf8mb4 NULL,
    `PhoneNumberConfirmed` tinyint(1) NOT NULL,
    `TwoFactorEnabled` tinyint(1) NOT NULL,
    `LockoutEnd` datetime(6) NULL,
    `LockoutEnabled` tinyint(1) NOT NULL,
    `AccessFailedCount` int NOT NULL,
    `Discriminator` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(1024) CHARACTER SET utf8mb4 NULL,
    `FirstName` varchar(255) CHARACTER SET utf8mb4 NULL,
    `LastName` varchar(255) CHARACTER SET utf8mb4 NULL,
    `IsAuthor` tinyint(1) NULL,
    `DisplayNameType` int NULL,
    `DisplayEmail` tinyint(1) NULL,
    `AvatarType` int NULL,
    `FacebookAvatarPath` longtext CHARACTER SET utf8mb4 NULL,
    `TwitterAvatarPath` longtext CHARACTER SET utf8mb4 NULL,
    `GoogleAvatarPath` longtext CHARACTER SET utf8mb4 NULL,
    `InternalAvatarPath` longtext CHARACTER SET utf8mb4 NULL,
    `LastUpdated` datetime(6) NULL,
    `DateCreated` datetime(6) NULL,
    CONSTRAINT `PK_AspNetUsers` PRIMARY KEY (`Id`)
);

CREATE TABLE `StranitzaCategories` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(127) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(1024) CHARACTER SET utf8mb4 NULL,
    `LastUpdated` datetime(6) NOT NULL,
    `DateCreated` datetime(6) NOT NULL,
    CONSTRAINT `PK_StranitzaCategories` PRIMARY KEY (`Id`)
);

CREATE TABLE `StranitzaFiles` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `FileName` varchar(127) CHARACTER SET utf8mb4 NOT NULL,
    `MimeType` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `Title` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
    `Extension` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `FilePath` varchar(512) CHARACTER SET utf8mb4 NOT NULL,
    `ThumbPath` varchar(512) CHARACTER SET utf8mb4 NULL,
    `LastUpdated` datetime(6) NOT NULL,
    `DateCreated` datetime(6) NOT NULL,
    CONSTRAINT `PK_StranitzaFiles` PRIMARY KEY (`Id`)
);

CREATE TABLE `AspNetRoleClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `RoleId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ClaimType` longtext CHARACTER SET utf8mb4 NULL,
    `ClaimValue` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_AspNetRoleClaims` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AspNetRoleClaims_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ClaimType` longtext CHARACTER SET utf8mb4 NULL,
    `ClaimValue` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_AspNetUserClaims` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AspNetUserClaims_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserLogins` (
    `LoginProvider` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ProviderKey` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ProviderDisplayName` longtext CHARACTER SET utf8mb4 NULL,
    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_AspNetUserLogins` PRIMARY KEY (`LoginProvider`, `ProviderKey`),
    CONSTRAINT `FK_AspNetUserLogins_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserRoles` (
    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `RoleId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_AspNetUserRoles` PRIMARY KEY (`UserId`, `RoleId`),
    CONSTRAINT `FK_AspNetUserRoles_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AspNetUserRoles_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserTokens` (
    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `LoginProvider` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Name` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Value` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_AspNetUserTokens` PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
    CONSTRAINT `FK_AspNetUserTokens_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `StranitzaEPages` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `FirstName` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `LastName` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Title` varchar(512) CHARACTER SET utf8mb4 NOT NULL,
    `Content` TEXT NOT NULL,
    `ReleaseNumber` int NOT NULL,
    `ReleaseYear` int NOT NULL,
    `Description` varchar(1024) CHARACTER SET utf8mb4 NULL,
    `Notes` varchar(1024) CHARACTER SET utf8mb4 NULL,
    `IsTranslation` tinyint(1) NOT NULL,
    `CategoryId` int NOT NULL,
    `AuthorId` varchar(127) CHARACTER SET utf8mb4 NULL,
    `UploaderId` varchar(127) CHARACTER SET utf8mb4 NOT NULL,
    `SourceId` int NULL,
    `DateCreated` datetime(6) NOT NULL,
    CONSTRAINT `PK_StranitzaEPages` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_StranitzaEPages_AspNetUsers_AuthorId` FOREIGN KEY (`AuthorId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_StranitzaEPages_StranitzaCategories_CategoryId` FOREIGN KEY (`CategoryId`) REFERENCES `StranitzaCategories` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_StranitzaEPages_AspNetUsers_UploaderId` FOREIGN KEY (`UploaderId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `StranitzaIssues` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `IssueNumber` int NOT NULL,
    `ReleaseNumber` int NOT NULL,
    `ReleaseYear` int NOT NULL,
    `Description` varchar(1024) CHARACTER SET utf8mb4 NULL,
    `IsAvailable` tinyint(1) NOT NULL,
    `PagesCount` int NOT NULL,
    `ViewCount` int NOT NULL,
    `DownloadCount` int NOT NULL,
    `AvailablePages` VARCHAR(512) NULL,
    `Tags` VARCHAR(256) NULL,
    `PdfFilePreviewId` int NULL,
    `PdfFileReducedId` int NULL,
    `ZipFileId` int NULL,
    `LastUpdated` datetime(6) NOT NULL,
    `DateCreated` datetime(6) NOT NULL,
    CONSTRAINT `PK_StranitzaIssues` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_StranitzaIssues_StranitzaFiles_PdfFilePreviewId` FOREIGN KEY (`PdfFilePreviewId`) REFERENCES `StranitzaFiles` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_StranitzaIssues_StranitzaFiles_PdfFileReducedId` FOREIGN KEY (`PdfFileReducedId`) REFERENCES `StranitzaFiles` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_StranitzaIssues_StranitzaFiles_ZipFileId` FOREIGN KEY (`ZipFileId`) REFERENCES `StranitzaFiles` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `StranitzaPosts` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Title` varchar(512) CHARACTER SET utf8mb4 NOT NULL,
    `Content` TEXT NOT NULL,
    `Description` varchar(1024) CHARACTER SET utf8mb4 NULL,
    `EditorsPick` tinyint(1) NOT NULL,
    `ViewCount` int NOT NULL,
    `Origin` varchar(1024) CHARACTER SET utf8mb4 NOT NULL,
    `UploaderId` varchar(127) CHARACTER SET utf8mb4 NOT NULL,
    `ImageFileId` int NULL,
    `LastUpdated` datetime(6) NOT NULL,
    `DateCreated` datetime(6) NOT NULL,
    CONSTRAINT `PK_StranitzaPosts` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_StranitzaPosts_StranitzaFiles_ImageFileId` FOREIGN KEY (`ImageFileId`) REFERENCES `StranitzaFiles` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_StranitzaPosts_AspNetUsers_UploaderId` FOREIGN KEY (`UploaderId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `StranitzaPages` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `PageNumber` int NOT NULL,
    `SlideNumber` int NOT NULL,
    `IsAvailable` tinyint(1) NOT NULL,
    `Type` int NOT NULL,
    `PageFileId` int NOT NULL,
    `IssueId` int NOT NULL,
    `LastUpdated` datetime(6) NOT NULL,
    `DateCreated` datetime(6) NOT NULL,
    CONSTRAINT `PK_StranitzaPages` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_StranitzaPages_StranitzaIssues_IssueId` FOREIGN KEY (`IssueId`) REFERENCES `StranitzaIssues` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_StranitzaPages_StranitzaFiles_PageFileId` FOREIGN KEY (`PageFileId`) REFERENCES `StranitzaFiles` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `StranitzaSources` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `FirstName` varchar(255) CHARACTER SET utf8mb4 NULL,
    `LastName` varchar(255) CHARACTER SET utf8mb4 NULL,
    `Origin` varchar(512) CHARACTER SET utf8mb4 NOT NULL,
    `Title` varchar(512) CHARACTER SET utf8mb4 NULL,
    `Description` varchar(1024) CHARACTER SET utf8mb4 NULL,
    `Notes` varchar(1024) CHARACTER SET utf8mb4 NULL,
    `ReleaseNumber` int NOT NULL,
    `ReleaseYear` int NOT NULL,
    `Pages` varchar(64) CHARACTER SET utf8mb4 NULL,
    `StartingPage` int NOT NULL,
    `IsTranslation` tinyint(1) NOT NULL,
    `Uploader` varchar(127) CHARACTER SET utf8mb4 NOT NULL,
    `CategoryId` int NOT NULL,
    `IssueId` int NULL,
    `EPageId` int NULL,
    `AuthorId` varchar(127) CHARACTER SET utf8mb4 NULL,
    `LastUpdated` datetime(6) NOT NULL,
    `DateCreated` datetime(6) NOT NULL,
    CONSTRAINT `PK_StranitzaSources` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_StranitzaSources_AspNetUsers_AuthorId` FOREIGN KEY (`AuthorId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_StranitzaSources_StranitzaCategories_CategoryId` FOREIGN KEY (`CategoryId`) REFERENCES `StranitzaCategories` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_StranitzaSources_StranitzaEPages_EPageId` FOREIGN KEY (`EPageId`) REFERENCES `StranitzaEPages` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_StranitzaSources_StranitzaIssues_IssueId` FOREIGN KEY (`IssueId`) REFERENCES `StranitzaIssues` (`Id`) ON DELETE SET NULL
);

CREATE TABLE `StranitzaComments` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Content` varchar(1024) CHARACTER SET utf8mb4 NOT NULL,
    `Note` varchar(1024) CHARACTER SET utf8mb4 NULL,
    `AuthorId` varchar(127) CHARACTER SET utf8mb4 NOT NULL,
    `ModeratorId` varchar(127) CHARACTER SET utf8mb4 NULL,
    `ParentId` int NULL,
    `IssueId` int NULL,
    `PostId` int NULL,
    `EPageId` int NULL,
    `LastUpdated` datetime(6) NOT NULL,
    `DateCreated` datetime(6) NOT NULL,
    CONSTRAINT `PK_StranitzaComments` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_StranitzaComments_AspNetUsers_AuthorId` FOREIGN KEY (`AuthorId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_StranitzaComments_StranitzaEPages_EPageId` FOREIGN KEY (`EPageId`) REFERENCES `StranitzaEPages` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_StranitzaComments_StranitzaIssues_IssueId` FOREIGN KEY (`IssueId`) REFERENCES `StranitzaIssues` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_StranitzaComments_AspNetUsers_ModeratorId` FOREIGN KEY (`ModeratorId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_StranitzaComments_StranitzaComments_ParentId` FOREIGN KEY (`ParentId`) REFERENCES `StranitzaComments` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_StranitzaComments_StranitzaPosts_PostId` FOREIGN KEY (`PostId`) REFERENCES `StranitzaPosts` (`Id`) ON DELETE CASCADE
);

CREATE INDEX `IX_AspNetRoleClaims_RoleId` ON `AspNetRoleClaims` (`RoleId`);

CREATE UNIQUE INDEX `RoleNameIndex` ON `AspNetRoles` (`NormalizedName`);

CREATE INDEX `IX_AspNetUserClaims_UserId` ON `AspNetUserClaims` (`UserId`);

CREATE INDEX `IX_AspNetUserLogins_UserId` ON `AspNetUserLogins` (`UserId`);

CREATE INDEX `IX_AspNetUserRoles_RoleId` ON `AspNetUserRoles` (`RoleId`);

CREATE INDEX `EmailIndex` ON `AspNetUsers` (`NormalizedEmail`);

CREATE UNIQUE INDEX `UserNameIndex` ON `AspNetUsers` (`NormalizedUserName`);

CREATE INDEX `IX_StranitzaComments_AuthorId` ON `StranitzaComments` (`AuthorId`);

CREATE INDEX `IX_StranitzaComments_EPageId` ON `StranitzaComments` (`EPageId`);

CREATE INDEX `IX_StranitzaComments_IssueId` ON `StranitzaComments` (`IssueId`);

CREATE INDEX `IX_StranitzaComments_ModeratorId` ON `StranitzaComments` (`ModeratorId`);

CREATE INDEX `IX_StranitzaComments_ParentId` ON `StranitzaComments` (`ParentId`);

CREATE INDEX `IX_StranitzaComments_PostId` ON `StranitzaComments` (`PostId`);

CREATE INDEX `IX_StranitzaEPages_AuthorId` ON `StranitzaEPages` (`AuthorId`);

CREATE INDEX `IX_StranitzaEPages_CategoryId` ON `StranitzaEPages` (`CategoryId`);

CREATE INDEX `IX_StranitzaEPages_UploaderId` ON `StranitzaEPages` (`UploaderId`);

CREATE UNIQUE INDEX `IX_StranitzaIssues_IssueNumber` ON `StranitzaIssues` (`IssueNumber`);

CREATE UNIQUE INDEX `IX_StranitzaIssues_PdfFilePreviewId` ON `StranitzaIssues` (`PdfFilePreviewId`);

CREATE UNIQUE INDEX `IX_StranitzaIssues_PdfFileReducedId` ON `StranitzaIssues` (`PdfFileReducedId`);

CREATE UNIQUE INDEX `IX_StranitzaIssues_ZipFileId` ON `StranitzaIssues` (`ZipFileId`);

CREATE UNIQUE INDEX `IX_StranitzaIssues_ReleaseYear_ReleaseNumber` ON `StranitzaIssues` (`ReleaseYear`, `ReleaseNumber`);

CREATE INDEX `IX_StranitzaPages_IssueId` ON `StranitzaPages` (`IssueId`);

CREATE INDEX `IX_StranitzaPages_PageFileId` ON `StranitzaPages` (`PageFileId`);

CREATE UNIQUE INDEX `IX_StranitzaPages_IssueId_PageNumber` ON `StranitzaPages` (`IssueId`, `PageNumber`);

CREATE INDEX `IX_StranitzaPosts_ImageFileId` ON `StranitzaPosts` (`ImageFileId`);

CREATE INDEX `IX_StranitzaPosts_UploaderId` ON `StranitzaPosts` (`UploaderId`);

CREATE INDEX `IX_StranitzaSources_AuthorId` ON `StranitzaSources` (`AuthorId`);

CREATE INDEX `IX_StranitzaSources_CategoryId` ON `StranitzaSources` (`CategoryId`);

CREATE UNIQUE INDEX `IX_StranitzaSources_EPageId` ON `StranitzaSources` (`EPageId`);

CREATE INDEX `IX_StranitzaSources_IssueId` ON `StranitzaSources` (`IssueId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20200910123701_InitialCreate', '3.1.8');

ALTER TABLE `AspNetUsers` DROP COLUMN `Discriminator`;

ALTER TABLE `AspNetUserTokens` MODIFY COLUMN `Name` varchar(127) CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `AspNetUserTokens` MODIFY COLUMN `LoginProvider` varchar(127) CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `AspNetUsers` MODIFY COLUMN `LastUpdated` datetime(6) NOT NULL;

ALTER TABLE `AspNetUsers` MODIFY COLUMN `IsAuthor` tinyint(1) NOT NULL;

ALTER TABLE `AspNetUsers` MODIFY COLUMN `DisplayNameType` int NOT NULL;

ALTER TABLE `AspNetUsers` MODIFY COLUMN `DisplayEmail` tinyint(1) NOT NULL;

ALTER TABLE `AspNetUsers` MODIFY COLUMN `DateCreated` datetime(6) NOT NULL;

ALTER TABLE `AspNetUsers` MODIFY COLUMN `AvatarType` int NOT NULL;

ALTER TABLE `AspNetUserLogins` MODIFY COLUMN `ProviderKey` varchar(127) CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `AspNetUserLogins` MODIFY COLUMN `LoginProvider` varchar(127) CHARACTER SET utf8mb4 NOT NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20200910140712_Discriminator', '3.1.8');


DELIMITER $$
CREATE PROCEDURE `CountByReleaseYear`(IN queryType INT)
BEGIN
	CASE queryType
		WHEN 1 THEN -- Sources
			SELECT x.ReleaseYear as Year, COUNT(*) as Count 
			FROM StranitzaSources x
			GROUP BY x.ReleaseYear
			ORDER BY x.ReleaseYear DESC;
		WHEN 2 THEN -- EPages
			SELECT x.ReleaseYear as Year, COUNT(*) as Count 
			FROM StranitzaEPages x
			GROUP BY x.ReleaseYear
			ORDER BY x.ReleaseYear DESC;
		WHEN 3 THEN -- Issues (all)
			SELECT x.ReleaseYear as Year, COUNT(*) as Count 
			FROM StranitzaIssues x
			GROUP BY x.ReleaseYear
			ORDER BY x.ReleaseYear DESC;
		WHEN 4 THEN -- Issues (available)
			SELECT x.ReleaseYear as Year, COUNT(*) as Count 
			FROM StranitzaIssues x
			WHERE x.IsAvailable = 1
			GROUP BY x.ReleaseYear
			ORDER BY x.ReleaseYear DESC;
	END CASE;
END$$

DELIMITER ;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20200912155437_CountByReleaseYearStoredProcedure', '3.1.8');

