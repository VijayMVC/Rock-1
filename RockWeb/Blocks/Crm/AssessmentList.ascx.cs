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
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rockweb.Blocks.Crm
{
    /// <summary>
    /// Lists all avalable assesments for the individual.
    /// </summary>
    [DisplayName( "Assessment List" )]
    [Category( "CRM" )]
    [Description( "Allows you to view and take any available assessments." )]

    #region Block Attributes

    [BooleanField(
        "Only Show Requested",
        "When checked, limits the list to show only assessments that have been requested..",
        true,
        order: 0 )]

    [BooleanField(
        "Hide If No Active Requests",
        "If enabled, the person can retake the test after the minimum days passes.",
        false,
        order: 1 )]

    [BooleanField(
        "Hide If No Requests",
        "If enabled, the person can retake the test after the minimum days passes.",
        false,
        order: 2 )]

    [CodeEditorField(
        "Lava Template",
        "The lava template to use to format the entire block.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        CodeEditorMode.Html,
        CodeEditorTheme.Rock,
        400,
        true,
        @"<div class='panel-heading panel-default rollover-container clearfix'>
    <div class='panel-heading'>Assessments</div>
    <div class='panel-body'>
            {% for assessmenttype in AssessmentTypes %}
                {% if assessmenttype.LastRequestObject %}
                    {% if assessmenttype.LastRequestObject.Status == 'Complete' %}
                        <div class='panel panel-success'>
                            <div class='panel-heading'> {{ assessmenttype.Title }}</br>
                                Completed: {{ assessmenttype.LastRequestObject.CompletedDate | Date:'M/d/yyyy'}} </br>
                                <a href='{{ assessmenttype.AssessmentResultsPath}}'>View Results</a>
                            </div>
                        </div>
                    {% elseif assessmenttype.LastRequestObject.Status == 'Pending' %}
                        <div class='panel panel-primary'>
                            <div class='panel-heading'> {{ assessmenttype.Title }}</br>
                                Requested: {{assessmenttype.LastRequestObject.Requester}} ({{ assessmenttype.LastRequestObject.RequestedDate | Date:'M/d/yyyy'}})</br>
                                <a href='{{ assessmenttype.AssessmentPath}}'>Start Assessment</a>
                            </div>
                        </div>
                    {% endif %}   
                    {% else %}
                        <div class='panel panel-default'>
                            <div class='panel-heading'> {{ assessmenttype.Title }}</br>
                                Available</br>
                                <a href='{{ assessmenttype.AssessmentPath}}'>Start Assessment</a>
                            </div>
                        </div>
                {% endif %}
            {% endfor %}
    </div>
</div>" )]

#endregion

    public partial class AssessmentList : Rock.Web.UI.RockBlock
    {
        #region Control Events

        private bool _onlyShowRequested = true;
        private bool _hideIfNoActiveRequests = false;
        private bool _hideIfNoRequests = false;

        /// <summary>
        /// On-Init
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit( EventArgs e )
        {
            // show hide requested
            _onlyShowRequested = GetAttributeValue( "OnlyShowRequested" ).AsBoolean();

            // hide if no active requests
            _hideIfNoActiveRequests = GetAttributeValue( "HideIfNoActiveRequests" ).AsBoolean();

            // hide if no requests
            _hideIfNoRequests = GetAttributeValue( "HideIfNoRequests" ).AsBoolean();

            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                MergeLavaFields();
            }
        }

        #endregion

        #region Methods
        Boolean _areThereAnyActiveRequests = false;
        Boolean _areThereAnyRequests = false;
        /// <summary>
        /// Merges the Lavafields to the control template.
        /// </summary>
        private void MergeLavaFields()
        {
            lAssessments.Visible = true;
            nbAssessmentWarning.Visible = false;

            // Gets Assessment types and assessments for each
            RockContext rc = new RockContext();
            AssessmentTypeService aService = new AssessmentTypeService( rc );
            
        var getallAssessmentTypes = aService.Queryable().Where(x=>x.IsActive == true).Select( a => new
            {
                Title = a.Title,
                AssessmentPath = a.AssessmentPath,
                AssessmentResultsPath = a.AssessmentResultsPath,
                LastRequestObject = a.Assessments
                    .Where( r => r.PersonAlias.Person.Id == CurrentPersonId )
                    .OrderByDescending( b => b.CreatedDateTime)
                    .Select( r => new
                    {
                        RequestedDate = r.RequestedDateTime,
                        CompletedDate = r.CompletedDateTime,
                        Status = r.Status,
                        Requester = r.RequesterPersonAlias.Person.NickName + " " + r.RequesterPersonAlias.Person.LastName
                    } ).OrderBy(x=>x.Status).FirstOrDefault()
            } ).OrderByDescending( x=>x.LastRequestObject.Status ).ToList();
            
            // Checks Current Request Types to use against the settings
            foreach ( var item in getallAssessmentTypes )
            {
                if (item.LastRequestObject!=null && item.LastRequestObject.Status==AssessmentRequestStatus.Pending )
                {
                    _areThereAnyActiveRequests = true;
                }

                if ( item.LastRequestObject != null && item.LastRequestObject.Requester!=null )
                {
                    _areThereAnyRequests = true;
                }

            }
            
            // Resolve the text field merge fields
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, CurrentPerson );

            CheckForRequestTypes();

            // Shows all assessments if Only Show Requested is set to false and only requested if set to true, on the requester
            if ( !_onlyShowRequested )
            {
                mergeFields.Add( "AssessmentTypes", getallAssessmentTypes );
            }
            else if ( _onlyShowRequested )
            {
                var onlyRequested = getallAssessmentTypes.Where( x => x.LastRequestObject != null )
                    .Where(x=>x.LastRequestObject.Requester != null
                    && x.LastRequestObject.Status == AssessmentRequestStatus.Pending );
                
                mergeFields.Add( "AssessmentTypes", onlyRequested );
            }

            lAssessments.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
            
        }

        /// <summary>
        /// Sets controls based on settings.
        /// </summary>
        private void CheckForRequestTypes()
        {
            // Checks for setting to hide if no active requests based on if there are any PENDING requests
            if ( _hideIfNoActiveRequests && !_areThereAnyActiveRequests )
            {
                nbAssessmentWarning.Visible = true;
                nbAssessmentWarning.Text = "There are no active requests assigned to you.";
                lAssessments.Visible = false;
            }

            // Checks for setting to hide if no requests based on if there are any Requesters associated with the assessments
            if ( _hideIfNoRequests && !_areThereAnyRequests )
            {
                nbAssessmentWarning.Visible = true;
                nbAssessmentWarning.Text = "There are no requests assigned to you.";
                lAssessments.Visible = false;
            }
        }

        #endregion
    }
}