using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContosoDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class DocumentRemainingFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Documents",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Announcements",
                keyColumn: "AnnouncementId",
                keyValue: 1,
                columns: new[] { "ExpiryDate", "PublishDate" },
                values: new object[] { new DateTime(2026, 5, 1, 16, 28, 12, 218, DateTimeKind.Utc).AddTicks(6864), new DateTime(2026, 4, 1, 16, 28, 12, 218, DateTimeKind.Utc).AddTicks(6863) });

            migrationBuilder.UpdateData(
                table: "ProjectMembers",
                keyColumn: "ProjectMemberId",
                keyValue: 1,
                column: "AssignedDate",
                value: new DateTime(2026, 3, 2, 16, 28, 12, 218, DateTimeKind.Utc).AddTicks(6836));

            migrationBuilder.UpdateData(
                table: "ProjectMembers",
                keyColumn: "ProjectMemberId",
                keyValue: 2,
                column: "AssignedDate",
                value: new DateTime(2026, 3, 2, 16, 28, 12, 218, DateTimeKind.Utc).AddTicks(6838));

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "ProjectId",
                keyValue: 1,
                columns: new[] { "CreatedDate", "StartDate", "TargetCompletionDate", "UpdatedDate" },
                values: new object[] { new DateTime(2026, 3, 2, 16, 28, 12, 218, DateTimeKind.Utc).AddTicks(6769), new DateTime(2026, 3, 2, 16, 28, 12, 218, DateTimeKind.Utc).AddTicks(6760), new DateTime(2026, 5, 31, 16, 28, 12, 218, DateTimeKind.Utc).AddTicks(6766), new DateTime(2026, 4, 1, 16, 28, 12, 218, DateTimeKind.Utc).AddTicks(6769) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "TaskId",
                keyValue: 1,
                columns: new[] { "CreatedDate", "DueDate", "UpdatedDate" },
                values: new object[] { new DateTime(2026, 3, 2, 16, 28, 12, 218, DateTimeKind.Utc).AddTicks(6799), new DateTime(2026, 3, 12, 16, 28, 12, 218, DateTimeKind.Utc).AddTicks(6797), new DateTime(2026, 3, 12, 16, 28, 12, 218, DateTimeKind.Utc).AddTicks(6800) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "TaskId",
                keyValue: 2,
                columns: new[] { "CreatedDate", "DueDate", "UpdatedDate" },
                values: new object[] { new DateTime(2026, 3, 7, 16, 28, 12, 218, DateTimeKind.Utc).AddTicks(6804), new DateTime(2026, 4, 6, 16, 28, 12, 218, DateTimeKind.Utc).AddTicks(6803), new DateTime(2026, 4, 1, 16, 28, 12, 218, DateTimeKind.Utc).AddTicks(6805) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "TaskId",
                keyValue: 3,
                columns: new[] { "CreatedDate", "DueDate", "UpdatedDate" },
                values: new object[] { new DateTime(2026, 3, 12, 16, 28, 12, 218, DateTimeKind.Utc).AddTicks(6809), new DateTime(2026, 4, 11, 16, 28, 12, 218, DateTimeKind.Utc).AddTicks(6808), new DateTime(2026, 3, 12, 16, 28, 12, 218, DateTimeKind.Utc).AddTicks(6810) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 4, 1, 16, 28, 12, 218, DateTimeKind.Utc).AddTicks(6565));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 4, 1, 16, 28, 12, 218, DateTimeKind.Utc).AddTicks(6570));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 4, 1, 16, 28, 12, 218, DateTimeKind.Utc).AddTicks(6574));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 4, 1, 16, 28, 12, 218, DateTimeKind.Utc).AddTicks(6577));

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Category_CreatedDateUtc",
                table: "Documents",
                columns: new[] { "Category", "CreatedDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Title_CreatedDateUtc",
                table: "Documents",
                columns: new[] { "Title", "CreatedDateUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_Category_CreatedDateUtc",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_Title_CreatedDateUtc",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Documents");

            migrationBuilder.UpdateData(
                table: "Announcements",
                keyColumn: "AnnouncementId",
                keyValue: 1,
                columns: new[] { "ExpiryDate", "PublishDate" },
                values: new object[] { new DateTime(2026, 5, 1, 15, 26, 28, 977, DateTimeKind.Utc).AddTicks(5013), new DateTime(2026, 4, 1, 15, 26, 28, 977, DateTimeKind.Utc).AddTicks(5012) });

            migrationBuilder.UpdateData(
                table: "ProjectMembers",
                keyColumn: "ProjectMemberId",
                keyValue: 1,
                column: "AssignedDate",
                value: new DateTime(2026, 3, 2, 15, 26, 28, 977, DateTimeKind.Utc).AddTicks(4985));

            migrationBuilder.UpdateData(
                table: "ProjectMembers",
                keyColumn: "ProjectMemberId",
                keyValue: 2,
                column: "AssignedDate",
                value: new DateTime(2026, 3, 2, 15, 26, 28, 977, DateTimeKind.Utc).AddTicks(4988));

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "ProjectId",
                keyValue: 1,
                columns: new[] { "CreatedDate", "StartDate", "TargetCompletionDate", "UpdatedDate" },
                values: new object[] { new DateTime(2026, 3, 2, 15, 26, 28, 977, DateTimeKind.Utc).AddTicks(4921), new DateTime(2026, 3, 2, 15, 26, 28, 977, DateTimeKind.Utc).AddTicks(4911), new DateTime(2026, 5, 31, 15, 26, 28, 977, DateTimeKind.Utc).AddTicks(4918), new DateTime(2026, 4, 1, 15, 26, 28, 977, DateTimeKind.Utc).AddTicks(4922) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "TaskId",
                keyValue: 1,
                columns: new[] { "CreatedDate", "DueDate", "UpdatedDate" },
                values: new object[] { new DateTime(2026, 3, 2, 15, 26, 28, 977, DateTimeKind.Utc).AddTicks(4948), new DateTime(2026, 3, 12, 15, 26, 28, 977, DateTimeKind.Utc).AddTicks(4947), new DateTime(2026, 3, 12, 15, 26, 28, 977, DateTimeKind.Utc).AddTicks(4950) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "TaskId",
                keyValue: 2,
                columns: new[] { "CreatedDate", "DueDate", "UpdatedDate" },
                values: new object[] { new DateTime(2026, 3, 7, 15, 26, 28, 977, DateTimeKind.Utc).AddTicks(4955), new DateTime(2026, 4, 6, 15, 26, 28, 977, DateTimeKind.Utc).AddTicks(4953), new DateTime(2026, 4, 1, 15, 26, 28, 977, DateTimeKind.Utc).AddTicks(4956) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "TaskId",
                keyValue: 3,
                columns: new[] { "CreatedDate", "DueDate", "UpdatedDate" },
                values: new object[] { new DateTime(2026, 3, 12, 15, 26, 28, 977, DateTimeKind.Utc).AddTicks(4959), new DateTime(2026, 4, 11, 15, 26, 28, 977, DateTimeKind.Utc).AddTicks(4958), new DateTime(2026, 3, 12, 15, 26, 28, 977, DateTimeKind.Utc).AddTicks(4960) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 4, 1, 15, 26, 28, 977, DateTimeKind.Utc).AddTicks(4699));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 4, 1, 15, 26, 28, 977, DateTimeKind.Utc).AddTicks(4704));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 4, 1, 15, 26, 28, 977, DateTimeKind.Utc).AddTicks(4707));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 4, 1, 15, 26, 28, 977, DateTimeKind.Utc).AddTicks(4710));
        }
    }
}
