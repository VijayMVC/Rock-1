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

    [BooleanField( "Only Show Requested", "When checked, limits the list to show only assessments that have been requested..", true, order: 0 )]
    [BooleanField( "Hide If No Active Requests", "If enabled, the person can retake the test after the minimum days passes.", false, order: 1 )]
    [BooleanField( "Hide If No Requests", "If enabled, the person can retake the test after the minimum days passes.", false, order: 2 )]


    [CodeEditorField( "Lava Template", "The lava template to use to format the entire block.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", CodeEditorMode.Html, CodeEditorTheme.Rock, 400, true, @"
    <div class='panel panel-default container'>
      <div class='panel-heading'>Assessments</div>
      <div class='panel panel-primary'>
      <div class='panel-heading'>Panel with panel-primary class</div>
    </div>
    <div class='panel panel-success'>
      <div class='panel-heading'>Panel with panel-success class</div>
    </div>
    <div class='panel panel-info'>
      <div class='panel-heading'>Panel with panel-info class</div>
    </div>
</div>
{{Person.NickName}}
{{LastRequestObject.AssessmentTypeID}}
{% for assessment in Assessmentype %}
        {{assessment.LastRequestObject.AssessmentTypeID}}
        {{assessment.LastRequestObject}}
{%endfor%}

Other Loop
{% for assessment in LastRequestObject %}
        {{assessment.AssessmentTypeID}}
        {{assessment.LastRequestObject}}
{%endfor%}

</div>" )]
    public partial class AssessmentList : Rock.Web.UI.RockBlock
    {
        private const string LAVAATTRIBUTEKEY = "LavaTemplate";

        #region Page Events

        /// <summary>
        /// On-Init
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit( EventArgs e )
        {
            // show hide requested
            Boolean showRequested = GetAttributeValue( "OnlyShowRequested" ).AsBoolean();

            //hide if no active requests
            Boolean hideIfNoActiveRequests = GetAttributeValue( "HideIfNoActiveRequests" ).AsBoolean();

            //hide if no requests
            Boolean hideIfNoRequests = GetAttributeValue( "HideIfNoRequests" ).AsBoolean();

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
        /// <summary>
        /// Shows the instructions.
        /// </summary>
        private void MergeLavaFields()
        {
            pnlAssessments.Visible = true;

            RockContext assessmentsTypes = new RockContext();
            Dictionary<string, List<Assessment>> getalltypes = assessmentsTypes.AssessmentTypes.AsNoTracking().ToList().Select( a => new
            {
                Title = a.Title,
                LastRequestObject = a.Assessments.ToList()
            } ).ToDictionary( a => a.Title, a => a.LastRequestObject );

            // Resolve the text field merge fields
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, CurrentPerson );
            if ( getalltypes != null )
            {
                mergeFields.Add( "AssessmentType", getalltypes);
                mergeFields.Add( "Person", CurrentPerson );
            }
            lAssessments.Text = GetAttributeValue( LAVAATTRIBUTEKEY ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
        }
        #endregion
    }
}