using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OSHManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToStationAndOrgCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommitteeActions_CommitteeRecommendations_RecommendationId",
                table: "CommitteeActions");

            migrationBuilder.DropForeignKey(
                name: "FK_CommitteeActions_Teams_TeamId",
                table: "CommitteeActions");

            migrationBuilder.DropForeignKey(
                name: "FK_CommitteeIssues_TeamMembers_RaisedByMemberId",
                table: "CommitteeIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_CommitteeIssues_Teams_TeamId",
                table: "CommitteeIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_CommitteeRecommendations_CommitteeIssues_IssueId",
                table: "CommitteeRecommendations");

            migrationBuilder.DropForeignKey(
                name: "FK_CommitteeRecommendations_TeamMembers_RecommendedByMemberId",
                table: "CommitteeRecommendations");

            migrationBuilder.DropForeignKey(
                name: "FK_CommitteeRecommendations_Teams_TeamId",
                table: "CommitteeRecommendations");

            migrationBuilder.DropForeignKey(
                name: "FK_ControlActions_Incidents_IncidentId",
                table: "ControlActions");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Departments_ParentDepartmentId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Stations_StationId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Stations_StationId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Hazards_Sections_SectionId",
                table: "Hazards");

            migrationBuilder.DropForeignKey(
                name: "FK_Hazards_Stations_StationId",
                table: "Hazards");

            migrationBuilder.DropForeignKey(
                name: "FK_Hazards_Teams_TeamId",
                table: "Hazards");

            migrationBuilder.DropForeignKey(
                name: "FK_IncidentCauses_Incidents_IncidentId",
                table: "IncidentCauses");

            migrationBuilder.DropForeignKey(
                name: "FK_IncidentInvestigationConfigs_Teams_TeamId",
                table: "IncidentInvestigationConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_IncidentInvestigations_Incidents_IncidentId",
                table: "IncidentInvestigations");

            migrationBuilder.DropForeignKey(
                name: "FK_IncidentInvestigations_Teams_InvestigationTeamId",
                table: "IncidentInvestigations");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Sections_SectionId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Stations_StationId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonsLearned_Incidents_IncidentId",
                table: "LessonsLearned");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaAccessLogs_MediaFiles_MediaId",
                table: "MediaAccessLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaAssociations_MediaFiles_MediaId",
                table: "MediaAssociations");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaConversionJobs_MediaFiles_MediaId",
                table: "MediaConversionJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaFiles_MediaCollections_CollectionId",
                table: "MediaFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaFiles_MediaFiles_ParentMediaId",
                table: "MediaFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_OrgMetadata_Stations_StationId",
                table: "OrgMetadata");

            migrationBuilder.DropForeignKey(
                name: "FK_OshCommitteeConfigs_Teams_TeamId",
                table: "OshCommitteeConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_OshPolicies_Stations_StationId",
                table: "OshPolicies");

            migrationBuilder.DropForeignKey(
                name: "FK_RiskAssessmentConfigs_Teams_TeamId",
                table: "RiskAssessmentConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_RiskMitigationPlans_Hazards_HazardId",
                table: "RiskMitigationPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Stations_StationId",
                table: "Sections");

            migrationBuilder.DropForeignKey(
                name: "FK_Stations_OrgCategories_OrgCategoryId",
                table: "Stations");

            migrationBuilder.DropForeignKey(
                name: "FK_Stations_Stations_ParentStationId",
                table: "Stations");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_Sections_SectionId",
                table: "TeamMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_Teams_TeamId",
                table: "TeamMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Stations_StationId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Stations_StationCode",
                table: "Stations");

            migrationBuilder.DropIndex(
                name: "IX_RiskAssessmentConfigs_TeamId",
                table: "RiskAssessmentConfigs");

            migrationBuilder.DropIndex(
                name: "IX_OshCommitteeConfigs_TeamId",
                table: "OshCommitteeConfigs");

            migrationBuilder.DropIndex(
                name: "IX_MediaCollections_CollectionName",
                table: "MediaCollections");

            migrationBuilder.DropIndex(
                name: "IX_IncidentInvestigationConfigs_TeamId",
                table: "IncidentInvestigationConfigs");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FormationDate",
                table: "Teams",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Stations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Stations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Stations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OrgCategories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "OrgCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "OrgCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Incidents",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<byte[]>(
                name: "LegacyPassword",
                table: "Employees",
                type: "varbinary(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Employees",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "TeamMemberMemberId",
                table: "CommitteeIssues",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessmentConfigs_TeamId",
                table: "RiskAssessmentConfigs",
                column: "TeamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OshCommitteeConfigs_TeamId",
                table: "OshCommitteeConfigs",
                column: "TeamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentInvestigationConfigs_TeamId",
                table: "IncidentInvestigationConfigs",
                column: "TeamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeIssues_TeamMemberMemberId",
                table: "CommitteeIssues",
                column: "TeamMemberMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommitteeActions_CommitteeRecommendations_RecommendationId",
                table: "CommitteeActions",
                column: "RecommendationId",
                principalTable: "CommitteeRecommendations",
                principalColumn: "RecommendationId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommitteeActions_Teams_TeamId",
                table: "CommitteeActions",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommitteeIssues_TeamMembers_RaisedByMemberId",
                table: "CommitteeIssues",
                column: "RaisedByMemberId",
                principalTable: "TeamMembers",
                principalColumn: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommitteeIssues_TeamMembers_TeamMemberMemberId",
                table: "CommitteeIssues",
                column: "TeamMemberMemberId",
                principalTable: "TeamMembers",
                principalColumn: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommitteeIssues_Teams_TeamId",
                table: "CommitteeIssues",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommitteeRecommendations_CommitteeIssues_IssueId",
                table: "CommitteeRecommendations",
                column: "IssueId",
                principalTable: "CommitteeIssues",
                principalColumn: "IssueId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommitteeRecommendations_TeamMembers_RecommendedByMemberId",
                table: "CommitteeRecommendations",
                column: "RecommendedByMemberId",
                principalTable: "TeamMembers",
                principalColumn: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommitteeRecommendations_Teams_TeamId",
                table: "CommitteeRecommendations",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_ControlActions_Incidents_IncidentId",
                table: "ControlActions",
                column: "IncidentId",
                principalTable: "Incidents",
                principalColumn: "IncidentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Departments_ParentDepartmentId",
                table: "Departments",
                column: "ParentDepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Stations_StationId",
                table: "Departments",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "StationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Stations_StationId",
                table: "Employees",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "StationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Hazards_Sections_SectionId",
                table: "Hazards",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Hazards_Stations_StationId",
                table: "Hazards",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "StationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Hazards_Teams_TeamId",
                table: "Hazards",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentCauses_Incidents_IncidentId",
                table: "IncidentCauses",
                column: "IncidentId",
                principalTable: "Incidents",
                principalColumn: "IncidentId");

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentInvestigationConfigs_Teams_TeamId",
                table: "IncidentInvestigationConfigs",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentInvestigations_Incidents_IncidentId",
                table: "IncidentInvestigations",
                column: "IncidentId",
                principalTable: "Incidents",
                principalColumn: "IncidentId");

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentInvestigations_Teams_InvestigationTeamId",
                table: "IncidentInvestigations",
                column: "InvestigationTeamId",
                principalTable: "Teams",
                principalColumn: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Sections_SectionId",
                table: "Incidents",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Stations_StationId",
                table: "Incidents",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "StationId");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonsLearned_Incidents_IncidentId",
                table: "LessonsLearned",
                column: "IncidentId",
                principalTable: "Incidents",
                principalColumn: "IncidentId");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaAccessLogs_MediaFiles_MediaId",
                table: "MediaAccessLogs",
                column: "MediaId",
                principalTable: "MediaFiles",
                principalColumn: "MediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaAssociations_MediaFiles_MediaId",
                table: "MediaAssociations",
                column: "MediaId",
                principalTable: "MediaFiles",
                principalColumn: "MediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaConversionJobs_MediaFiles_MediaId",
                table: "MediaConversionJobs",
                column: "MediaId",
                principalTable: "MediaFiles",
                principalColumn: "MediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaFiles_MediaCollections_CollectionId",
                table: "MediaFiles",
                column: "CollectionId",
                principalTable: "MediaCollections",
                principalColumn: "CollectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaFiles_MediaFiles_ParentMediaId",
                table: "MediaFiles",
                column: "ParentMediaId",
                principalTable: "MediaFiles",
                principalColumn: "MediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrgMetadata_Stations_StationId",
                table: "OrgMetadata",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "StationId");

            migrationBuilder.AddForeignKey(
                name: "FK_OshCommitteeConfigs_Teams_TeamId",
                table: "OshCommitteeConfigs",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_OshPolicies_Stations_StationId",
                table: "OshPolicies",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "StationId");

            migrationBuilder.AddForeignKey(
                name: "FK_RiskAssessmentConfigs_Teams_TeamId",
                table: "RiskAssessmentConfigs",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_RiskMitigationPlans_Hazards_HazardId",
                table: "RiskMitigationPlans",
                column: "HazardId",
                principalTable: "Hazards",
                principalColumn: "HazardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Stations_StationId",
                table: "Sections",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "StationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stations_OrgCategories_OrgCategoryId",
                table: "Stations",
                column: "OrgCategoryId",
                principalTable: "OrgCategories",
                principalColumn: "OrgCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stations_Stations_ParentStationId",
                table: "Stations",
                column: "ParentStationId",
                principalTable: "Stations",
                principalColumn: "StationId");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_Sections_SectionId",
                table: "TeamMembers",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_Teams_TeamId",
                table: "TeamMembers",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Stations_StationId",
                table: "Teams",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "StationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommitteeActions_CommitteeRecommendations_RecommendationId",
                table: "CommitteeActions");

            migrationBuilder.DropForeignKey(
                name: "FK_CommitteeActions_Teams_TeamId",
                table: "CommitteeActions");

            migrationBuilder.DropForeignKey(
                name: "FK_CommitteeIssues_TeamMembers_RaisedByMemberId",
                table: "CommitteeIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_CommitteeIssues_TeamMembers_TeamMemberMemberId",
                table: "CommitteeIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_CommitteeIssues_Teams_TeamId",
                table: "CommitteeIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_CommitteeRecommendations_CommitteeIssues_IssueId",
                table: "CommitteeRecommendations");

            migrationBuilder.DropForeignKey(
                name: "FK_CommitteeRecommendations_TeamMembers_RecommendedByMemberId",
                table: "CommitteeRecommendations");

            migrationBuilder.DropForeignKey(
                name: "FK_CommitteeRecommendations_Teams_TeamId",
                table: "CommitteeRecommendations");

            migrationBuilder.DropForeignKey(
                name: "FK_ControlActions_Incidents_IncidentId",
                table: "ControlActions");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Departments_ParentDepartmentId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Stations_StationId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Stations_StationId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Hazards_Sections_SectionId",
                table: "Hazards");

            migrationBuilder.DropForeignKey(
                name: "FK_Hazards_Stations_StationId",
                table: "Hazards");

            migrationBuilder.DropForeignKey(
                name: "FK_Hazards_Teams_TeamId",
                table: "Hazards");

            migrationBuilder.DropForeignKey(
                name: "FK_IncidentCauses_Incidents_IncidentId",
                table: "IncidentCauses");

            migrationBuilder.DropForeignKey(
                name: "FK_IncidentInvestigationConfigs_Teams_TeamId",
                table: "IncidentInvestigationConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_IncidentInvestigations_Incidents_IncidentId",
                table: "IncidentInvestigations");

            migrationBuilder.DropForeignKey(
                name: "FK_IncidentInvestigations_Teams_InvestigationTeamId",
                table: "IncidentInvestigations");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Sections_SectionId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Stations_StationId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonsLearned_Incidents_IncidentId",
                table: "LessonsLearned");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaAccessLogs_MediaFiles_MediaId",
                table: "MediaAccessLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaAssociations_MediaFiles_MediaId",
                table: "MediaAssociations");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaConversionJobs_MediaFiles_MediaId",
                table: "MediaConversionJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaFiles_MediaCollections_CollectionId",
                table: "MediaFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaFiles_MediaFiles_ParentMediaId",
                table: "MediaFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_OrgMetadata_Stations_StationId",
                table: "OrgMetadata");

            migrationBuilder.DropForeignKey(
                name: "FK_OshCommitteeConfigs_Teams_TeamId",
                table: "OshCommitteeConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_OshPolicies_Stations_StationId",
                table: "OshPolicies");

            migrationBuilder.DropForeignKey(
                name: "FK_RiskAssessmentConfigs_Teams_TeamId",
                table: "RiskAssessmentConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_RiskMitigationPlans_Hazards_HazardId",
                table: "RiskMitigationPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Stations_StationId",
                table: "Sections");

            migrationBuilder.DropForeignKey(
                name: "FK_Stations_OrgCategories_OrgCategoryId",
                table: "Stations");

            migrationBuilder.DropForeignKey(
                name: "FK_Stations_Stations_ParentStationId",
                table: "Stations");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_Sections_SectionId",
                table: "TeamMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_Teams_TeamId",
                table: "TeamMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Stations_StationId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_RiskAssessmentConfigs_TeamId",
                table: "RiskAssessmentConfigs");

            migrationBuilder.DropIndex(
                name: "IX_OshCommitteeConfigs_TeamId",
                table: "OshCommitteeConfigs");

            migrationBuilder.DropIndex(
                name: "IX_IncidentInvestigationConfigs_TeamId",
                table: "IncidentInvestigationConfigs");

            migrationBuilder.DropIndex(
                name: "IX_CommitteeIssues_TeamMemberMemberId",
                table: "CommitteeIssues");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OrgCategories");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "OrgCategories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "OrgCategories");

            migrationBuilder.DropColumn(
                name: "TeamMemberMemberId",
                table: "CommitteeIssues");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FormationDate",
                table: "Teams",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Incidents",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "LegacyPassword",
                table: "Employees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Employees",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_StationCode",
                table: "Stations",
                column: "StationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessmentConfigs_TeamId",
                table: "RiskAssessmentConfigs",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_OshCommitteeConfigs_TeamId",
                table: "OshCommitteeConfigs",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaCollections_CollectionName",
                table: "MediaCollections",
                column: "CollectionName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentInvestigationConfigs_TeamId",
                table: "IncidentInvestigationConfigs",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommitteeActions_CommitteeRecommendations_RecommendationId",
                table: "CommitteeActions",
                column: "RecommendationId",
                principalTable: "CommitteeRecommendations",
                principalColumn: "RecommendationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommitteeActions_Teams_TeamId",
                table: "CommitteeActions",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommitteeIssues_TeamMembers_RaisedByMemberId",
                table: "CommitteeIssues",
                column: "RaisedByMemberId",
                principalTable: "TeamMembers",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CommitteeIssues_Teams_TeamId",
                table: "CommitteeIssues",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CommitteeRecommendations_CommitteeIssues_IssueId",
                table: "CommitteeRecommendations",
                column: "IssueId",
                principalTable: "CommitteeIssues",
                principalColumn: "IssueId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommitteeRecommendations_TeamMembers_RecommendedByMemberId",
                table: "CommitteeRecommendations",
                column: "RecommendedByMemberId",
                principalTable: "TeamMembers",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CommitteeRecommendations_Teams_TeamId",
                table: "CommitteeRecommendations",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ControlActions_Incidents_IncidentId",
                table: "ControlActions",
                column: "IncidentId",
                principalTable: "Incidents",
                principalColumn: "IncidentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Departments_ParentDepartmentId",
                table: "Departments",
                column: "ParentDepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Stations_StationId",
                table: "Departments",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "StationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Stations_StationId",
                table: "Employees",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "StationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Hazards_Sections_SectionId",
                table: "Hazards",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "SectionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Hazards_Stations_StationId",
                table: "Hazards",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "StationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Hazards_Teams_TeamId",
                table: "Hazards",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentCauses_Incidents_IncidentId",
                table: "IncidentCauses",
                column: "IncidentId",
                principalTable: "Incidents",
                principalColumn: "IncidentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentInvestigationConfigs_Teams_TeamId",
                table: "IncidentInvestigationConfigs",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentInvestigations_Incidents_IncidentId",
                table: "IncidentInvestigations",
                column: "IncidentId",
                principalTable: "Incidents",
                principalColumn: "IncidentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentInvestigations_Teams_InvestigationTeamId",
                table: "IncidentInvestigations",
                column: "InvestigationTeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Sections_SectionId",
                table: "Incidents",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "SectionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Stations_StationId",
                table: "Incidents",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "StationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonsLearned_Incidents_IncidentId",
                table: "LessonsLearned",
                column: "IncidentId",
                principalTable: "Incidents",
                principalColumn: "IncidentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MediaAccessLogs_MediaFiles_MediaId",
                table: "MediaAccessLogs",
                column: "MediaId",
                principalTable: "MediaFiles",
                principalColumn: "MediaId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MediaAssociations_MediaFiles_MediaId",
                table: "MediaAssociations",
                column: "MediaId",
                principalTable: "MediaFiles",
                principalColumn: "MediaId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MediaConversionJobs_MediaFiles_MediaId",
                table: "MediaConversionJobs",
                column: "MediaId",
                principalTable: "MediaFiles",
                principalColumn: "MediaId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MediaFiles_MediaCollections_CollectionId",
                table: "MediaFiles",
                column: "CollectionId",
                principalTable: "MediaCollections",
                principalColumn: "CollectionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MediaFiles_MediaFiles_ParentMediaId",
                table: "MediaFiles",
                column: "ParentMediaId",
                principalTable: "MediaFiles",
                principalColumn: "MediaId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrgMetadata_Stations_StationId",
                table: "OrgMetadata",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "StationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OshCommitteeConfigs_Teams_TeamId",
                table: "OshCommitteeConfigs",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OshPolicies_Stations_StationId",
                table: "OshPolicies",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "StationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RiskAssessmentConfigs_Teams_TeamId",
                table: "RiskAssessmentConfigs",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RiskMitigationPlans_Hazards_HazardId",
                table: "RiskMitigationPlans",
                column: "HazardId",
                principalTable: "Hazards",
                principalColumn: "HazardId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Stations_StationId",
                table: "Sections",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "StationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Stations_OrgCategories_OrgCategoryId",
                table: "Stations",
                column: "OrgCategoryId",
                principalTable: "OrgCategories",
                principalColumn: "OrgCategoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Stations_Stations_ParentStationId",
                table: "Stations",
                column: "ParentStationId",
                principalTable: "Stations",
                principalColumn: "StationId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_Sections_SectionId",
                table: "TeamMembers",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "SectionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_Teams_TeamId",
                table: "TeamMembers",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Stations_StationId",
                table: "Teams",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "StationId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
