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
    public partial class AddConflictProfile : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType( "Global", "Conflict Profile", "", "EE7E089E-DF81-4407-8BFA-AD865FA5427A", @"" );
            RockMigrationHelper.UpdateDefinedValue( "EE7E089E-DF81-4407-8BFA-AD865FA5427A", "Avoiding", "Avoiding is not pursuing your own rights or those of the other person.", "663B0F4A-DE1F-46BE-8BDD-D7C98863DDC4", true );
            RockMigrationHelper.UpdateDefinedValue( "EE7E089E-DF81-4407-8BFA-AD865FA5427A", "Compromising", "Compromising is finding a middle ground in the conflict.", "CF78D6B1-38AA-4FF7-9A4B-E900438FA85A", true );
            RockMigrationHelper.UpdateDefinedValue( "EE7E089E-DF81-4407-8BFA-AD865FA5427A", "Resolving", "Resolving is attempting to work with the other person in depth to find the best solution regardless of where it may lie on the continuum.", "DF7B1EB2-7E7E-4F91-BD26-C6DFD88E38DF", true );
            RockMigrationHelper.UpdateDefinedValue( "EE7E089E-DF81-4407-8BFA-AD865FA5427A", "Winning", "Winning is you believe you have the right answer and you must prove you are right whatever it takes.", "56300095-86AD-43FE-98D2-50829E9223C2", true );
            RockMigrationHelper.UpdateDefinedValue( "EE7E089E-DF81-4407-8BFA-AD865FA5427A", "Yielding", "Yielding is neglecting your own interests and giving in to those of the other person.", "4AB06A6F-F5B1-4385-9365-199EA7969E50", true );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Conflict Approach: Winning", "core_ConflictApproachWinning", "", "", 0, "", SystemGuid.Attribute.PERSON_CONFLICT_APPROACH_WINNING );
            AddSecurityToAttributes( Rock.SystemGuid.Attribute.PERSON_CONFLICT_APPROACH_WINNING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Conflict Approach: Resolving", "core_ConflictApproachResolving", "", "", 0, "", SystemGuid.Attribute.PERSON_CONFLICT_APPROACH_RESOLVING );
            AddSecurityToAttributes( Rock.SystemGuid.Attribute.PERSON_CONFLICT_APPROACH_RESOLVING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Conflict Approach: Compromising", "core_ConflictApproachCompromising", "", "", 0, "", SystemGuid.Attribute.PERSON_CONFLICT_APPROACH_COMPROMISING );
            AddSecurityToAttributes( Rock.SystemGuid.Attribute.PERSON_CONFLICT_APPROACH_COMPROMISING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Conflict Approach: Avoiding", "core_ConflictApproachAvoiding", "", "", 0, "", SystemGuid.Attribute.PERSON_CONFLICT_APPROACH_AVOIDING );
            AddSecurityToAttributes( Rock.SystemGuid.Attribute.PERSON_CONFLICT_APPROACH_AVOIDING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Conflict Approach: Yielding", "core_ConflictApproachYielding", "", "", 0, "", SystemGuid.Attribute.PERSON_CONFLICT_APPROACH_YIELDING );
            AddSecurityToAttributes( Rock.SystemGuid.Attribute.PERSON_CONFLICT_APPROACH_YIELDING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Conflict Engagement: Passive", "core_ConflictEngagementPassive", "", "", 0, "", SystemGuid.Attribute.PERSON_CONFLICT_APPROACH_PASSIVE );
            AddSecurityToAttributes( Rock.SystemGuid.Attribute.PERSON_CONFLICT_APPROACH_PASSIVE );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Conflict Engagement: Aggressive", "core_ConflictEngagementAggressive", "", "", 0, "", SystemGuid.Attribute.PERSON_CONFLICT_APPROACH_AGGRESSIVE );
            AddSecurityToAttributes( Rock.SystemGuid.Attribute.PERSON_CONFLICT_APPROACH_AGGRESSIVE );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Conflict Engagement: Engaged", "core_ConflictEngagementEngaged", "", "", 0, "", SystemGuid.Attribute.PERSON_CONFLICT_APPROACH_ENGAGED );
            AddSecurityToAttributes( Rock.SystemGuid.Attribute.PERSON_CONFLICT_APPROACH_ENGAGED );
        }

        /// <summary>
        /// Add security to attributes
        /// </summary>
        private void AddSecurityToAttributes( string attributeGuid )
        {
            RockMigrationHelper.AddSecurityAuthForAttribute(
               attributeGuid,
               0,
               Rock.Security.Authorization.VIEW,
               true,
               Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS,
               ( int ) Rock.Model.SpecialRole.None,
               Guid.NewGuid().ToString()
               );
            RockMigrationHelper.AddSecurityAuthForAttribute(
               attributeGuid,
               1,
               Rock.Security.Authorization.VIEW,
               true,
               Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
               ( int ) Rock.Model.SpecialRole.None,
               Guid.NewGuid().ToString()
               );
            RockMigrationHelper.AddSecurityAuthForAttribute(
                attributeGuid,
                2,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUsers,
                Guid.NewGuid().ToString()
             );

            RockMigrationHelper.AddSecurityAuthForAttribute(
               attributeGuid,
               0,
               Rock.Security.Authorization.EDIT,
               true,
               Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS,
               ( int ) Rock.Model.SpecialRole.None,
               Guid.NewGuid().ToString()
               );
            RockMigrationHelper.AddSecurityAuthForAttribute(
               attributeGuid,
               1,
               Rock.Security.Authorization.EDIT,
               true,
               Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
               ( int ) Rock.Model.SpecialRole.None,
               Guid.NewGuid().ToString()
               );
            RockMigrationHelper.AddSecurityAuthForAttribute(
                attributeGuid,
                2,
                Rock.Security.Authorization.EDIT,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUsers,
                Guid.NewGuid().ToString()
             );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
