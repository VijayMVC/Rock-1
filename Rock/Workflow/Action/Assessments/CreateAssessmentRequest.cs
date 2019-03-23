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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Security;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Creates a new connection request.
    /// </summary>
    [ActionCategory( "Assessments" )]
    [Description( "Creates a new assessment request." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Assessment Request Create" )]

    #region Block Atttributes

    [WorkflowAttribute( "Person Attribute", "The Assessment attribute.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.AssessmentsFieldType" } )]
    [WorkflowAttribute( "Person Taking Assessment", "The attribute that contains the type of connection opportunity to create.", true, "", "", 1, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [WorkflowAttribute( "Person Requesting", "The attribute that contains the connection status to use for the new request.", false, "", "", 2, null,
        new string[] { "Rock.Field.Types.DateTimeFieldType" } )]

    #endregion



    public class CreateAssessmentRequest : ActionComponent
    {
        #region Attribute Strings

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        protected static class AttributeKey
        {
            /// <summary>
            /// The assessments
            /// </summary>
            public const string AssessmentTypesKey = "AssessmentTypesKey";

            /// <summary>
            /// The person taking the assessment(s)
            /// </summary>
            public const string PersonTakingAssessments = "PersonTakingAssessments";

            /// <summary>
            /// The person requesting the assessment(s)
            /// </summary>
            public const string PersonRequestingAssessments = "PersonRequestingAssessments";

            /// <summary>
            /// The due date for the assessment(s)
            /// </summary>
            public const string DueDate = "DueDate";
        }

        #endregion

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            // Get the person
            PersonAlias personAlias = null;
            Guid personAliasGuid = action.GetWorklowAttributeValue(GetAttributeValue( action, "PersonAttribute" ).AsGuid()).AsGuid();
            personAlias = new PersonAliasService( rockContext ).Get( personAliasGuid );
            if ( personAlias == null )
            {
                errorMessages.Add( "Invalid Person Attribute or Value!" );
                return false;
            }

            // Get the opportunity
            ConnectionOpportunity opportunity = null;
            Guid opportunityTypeGuid = action.GetWorklowAttributeValue( GetAttributeValue( action, "ConnectionOpportunityAttribute" ).AsGuid() ).AsGuid();
            opportunity = new ConnectionOpportunityService( rockContext ).Get( opportunityTypeGuid );
            if ( opportunity == null )
            {
                errorMessages.Add( "Invalid Connection Opportunity Attribute or Value!" );
                return false;
            }

            // Get connection status
            ConnectionStatus status = null;
            Guid? connectionStatusGuid = null;
            Guid? connectionStatusAttributeGuid = GetAttributeValue( action, "ConnectionStatusAttribute" ).AsGuidOrNull();
            if ( connectionStatusAttributeGuid.HasValue )
            {
                connectionStatusGuid = action.GetWorklowAttributeValue( connectionStatusAttributeGuid.Value ).AsGuidOrNull();
                if ( connectionStatusGuid.HasValue )
                {
                    status = opportunity.ConnectionType.ConnectionStatuses
                        .Where( s => s.Guid.Equals( connectionStatusGuid.Value ) )
                        .FirstOrDefault();
                }
            }
            if ( status == null )
            {
                connectionStatusGuid = GetAttributeValue( action, "ConnectionStatus" ).AsGuidOrNull();
                if ( connectionStatusGuid.HasValue )
                {
                    status = opportunity.ConnectionType.ConnectionStatuses
                        .Where( s => s.Guid.Equals( connectionStatusGuid.Value ) )
                        .FirstOrDefault();
                }
            }
            if ( status == null )
            {
                status = opportunity.ConnectionType.ConnectionStatuses
                    .Where( s => s.IsDefault )
                    .FirstOrDefault();
            }

            // Get Campus
            int? campusId = null;
            Guid? campusAttributeGuid = GetAttributeValue( action, "CampusAttribute" ).AsGuidOrNull();
            if ( campusAttributeGuid.HasValue )
            {
                Guid? campusGuid = action.GetWorklowAttributeValue( campusAttributeGuid.Value ).AsGuidOrNull();
                if ( campusGuid.HasValue )
                {
                    var campus = CampusCache.Get( campusGuid.Value );
                    if ( campus != null )
                    {
                        campusId = campus.Id;
                    }
                }
            }

            // Get the Comment
            String comment = action.GetWorklowAttributeValue(GetAttributeValue(action, "ConnectionCommentAttribute").AsGuid());

            var connectionRequestService = new ConnectionRequestService( rockContext );

            var connectionRequest = new ConnectionRequest();
            connectionRequest.PersonAliasId = personAlias.Id;
            connectionRequest.ConnectionOpportunityId = opportunity.Id;
            connectionRequest.ConnectionState = ConnectionState.Active;
            connectionRequest.ConnectionStatusId = status.Id;
            connectionRequest.CampusId = campusId;
            connectionRequest.ConnectorPersonAliasId = opportunity.GetDefaultConnectorPersonAliasId( campusId );
            connectionRequest.Comments = comment;

            connectionRequestService.Add( connectionRequest );
            rockContext.SaveChanges();

            // If request attribute was specified, requery the request and set the attribute's value
            Guid? connectionRequestAttributeGuid = GetAttributeValue( action, "ConnectionRequestAttribute" ).AsGuidOrNull();
            if ( connectionRequestAttributeGuid.HasValue )
            {
                connectionRequest = connectionRequestService.Get( connectionRequest.Id );
                if ( connectionRequest != null )
                {
                    SetWorkflowAttributeValue( action, connectionRequestAttributeGuid.Value, connectionRequest.Guid.ToString() );
                }
            }

            return true;
        }
    }
}