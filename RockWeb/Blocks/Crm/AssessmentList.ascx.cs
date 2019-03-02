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
    [BooleanField( "Hide If No Active Requests", "If enabled, the person can retake the test after the minimum days passes.", true, order: 1 )]
    [BooleanField( "Hide If No Requests", "If enabled, the person can retake the test after the minimum days passes.", true, order: 2 )]


    [CodeEditorField( "Lava Template", "The lava template to use to format the entire block..  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", CodeEditorMode.Html, CodeEditorTheme.Rock, 400, true, @"
<h2>Welcome to Your Spiritual Gifts Assessment</h2>
<p>
    {{ Person.NickName }}, the purpose of this assessment is to help you identify spiritual gifts that are most naturally
    used in the life of the local church. This survey does not include all spiritual gifts, just those that are often
    seen in action for most churches and most people.
</p>
<p>
    In churches it’s not uncommon to see 90% of the work being done by a weary 10%. Why does this happen?
    Partially due to ignorance and partially due to avoidance of spiritual gifts. Here’s the process:
</p>
<ol>
    <li>Discover the primary gifts given to us at our Spiritual birth.</li>
    <li>Learn what these gifts are and what they are not.</li>
    <li>See where these gifts fit into the functioning of the body. </li>
</ol>
<p>
    When you are working within your Spirit-given gifts, you will be most effective for the body
    of Christ in your local setting.
</p>
<p>
    Before you begin, please take a moment and pray that the Holy Spirit would guide your thoughts,
    calm your mind, and help you respond to each item as honestly as you can. Don't spend much time
    on each item. Your first instinct is probably your best response.
</p>" )]
    
    public partial class AssessmentList : Rock.Web.UI.RockBlock
    {

        private const string INSTRUCTIONS = "LavaTemplate";
        #region Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            RockContext assessmentsTypes = new RockContext();
            var getalltypes = assessmentsTypes.AssessmentTypes.AsNoTracking().ToList();

            RockContext assessments = new RockContext();
            var getallAssesments = assessments.Assessments.AsNoTracking().Where(x=>x.PersonAliasID==CurrentPersonAliasId).ToList();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            

            if ( !Page.IsPostBack )
            {
                ShowInstructions();
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Shows the instructions.
        /// </summary>
        private void ShowInstructions()
        {
            pnlInstructions.Visible = true;
           
            // Resolve the text field merge fields
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, CurrentPerson );
            if ( CurrentPerson != null )
            {
                mergeFields.Add( "Person", CurrentPerson );
            }
            lInstructions.Text = GetAttributeValue( INSTRUCTIONS ).ResolveMergeFields( mergeFields );
        }
        #endregion
    }
}