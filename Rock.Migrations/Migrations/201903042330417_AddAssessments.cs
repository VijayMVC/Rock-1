// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class AddAssessments : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.Assessment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonAliasId = c.Int(nullable: false),
                        AssessmentTypeId = c.Int(nullable: false),
                        RequesterPersonAliasId = c.Int(),
                        RequestedDateTime = c.DateTime(),
                        RequestedDueDate = c.DateTime(),
                        Status = c.Int(nullable: false),
                        CompletedDateTime = c.DateTime(),
                        AssessmentResultData = c.String(),
                        LastReminderDate = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                        AssessmentType_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AssessmentType", t => t.AssessmentType_Id)
                .ForeignKey("dbo.AssessmentType", t => t.AssessmentTypeId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.RequesterPersonAliasId)
                .Index(t => t.PersonAliasId)
                .Index(t => t.AssessmentTypeId)
                .Index(t => t.RequesterPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.AssessmentType_Id);
            
            CreateTable(
                "dbo.AssessmentType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 100),
                        Description = c.String(nullable: false),
                        AssessmentPath = c.String(nullable: false, maxLength: 250),
                        AssessmentResultsPath = c.String(maxLength: 250),
                        IsActive = c.Boolean(nullable: false),
                        RequiresRequest = c.Boolean(nullable: false),
                        MinimumDaysToRetake = c.Int(nullable: false),
                        ValidDuration = c.Int(nullable: false),
                        IsSystem = c.Boolean(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Assessment", "RequesterPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Assessment", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Assessment", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Assessment", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Assessment", "AssessmentTypeId", "dbo.AssessmentType");
            DropForeignKey("dbo.AssessmentType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.AssessmentType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Assessment", "AssessmentType_Id", "dbo.AssessmentType");
            DropIndex("dbo.AssessmentType", new[] { "Guid" });
            DropIndex("dbo.AssessmentType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.AssessmentType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.Assessment", new[] { "AssessmentType_Id" });
            DropIndex("dbo.Assessment", new[] { "Guid" });
            DropIndex("dbo.Assessment", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.Assessment", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.Assessment", new[] { "RequesterPersonAliasId" });
            DropIndex("dbo.Assessment", new[] { "AssessmentTypeId" });
            DropIndex("dbo.Assessment", new[] { "PersonAliasId" });
            DropTable("dbo.AssessmentType");
            DropTable("dbo.Assessment");
        }
    }
}
