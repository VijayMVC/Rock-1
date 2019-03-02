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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Web;

using Rock.Data;
using Rock.UniversalSearch;
using Rock.UniversalSearch.IndexModels;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents an assessment.
    /// </summary>
    [RockDomain( "CRM" )]
    [Table( "AssessmentType" )]
    [DataContract]
    public class AssessmentType : Model<AssessmentType>, IRockEntity
    {
        #region Entity Properties

        /// <summary>
        ///Title  
        /// /// </summary>
        /// <value>
        /// A <see cref="System.String"/> <c>false</c>.
        /// </value>
        [Required]
        [MaxLength(100)]
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        ///Description  
        /// /// </summary>
        /// <value>
        /// A <see cref="System.String"/> <c>false</c>.
        /// </value>
        [Required]
        [StringLength( 100, MinimumLength = 3 )]
        [MaxLength]
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        ///AssessmentResultsPath  
        /// /// </summary>
        /// <value>
        /// A <see cref="System.String"/> <c>false</c>.
        /// </value>
        [Required]
        [MaxLength( 250 )]
        [DataMember]
        public string AssessmentPath { get; set; }

        /// <summary>
        ///AssessmentResultsPath  
        /// /// </summary>
        /// <value>
        /// A <see cref="System.String"/> <c>false</c>.
        /// </value>
        [MaxLength( 250 )]
        [DataMember]
        public string AssessmentResultsPath { get; set; }

        /// <summary>
        ///IsActive  
        /// /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> <c>false</c>.
        /// </value>
        [Required]
        [DataMember]
        public Boolean IsActive { get; set; }

        /// <summary>
        ///RequiresRequest  
        /// /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> <c>false</c>.
        /// </value>
        [Required]
        [DataMember]
        public Boolean RequiresRequest { get; set; }

        /// <summary>
        ///MinimumDaysToRetake
        /// /// </summary>
        /// <value>
        /// A <see cref="System.int"/> <c>false</c>.
        /// </value>
        [DataMember]
        public int MinimumDaysToRetake { get; set; }

        /// <summary>
        ///ValidDuration
        /// /// </summary>
        /// <value>
        /// A <see cref="System.int"/> <c>false</c>.
        /// </value>
        [DataMember]
        public int ValidDuration { get; set; }

        #endregion
    }
}
