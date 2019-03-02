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
    [Table( "Assessment" )]
    [DataContract]
    public class Assessment : Model<Assessment>, IRockEntity
    {
        #region Entity Properties
        /// <summary>
        ///PersonAliasID
        /// /// </summary>
        /// <value>
        /// A <see cref="System.int"/> <c>false</c>.
        /// </value>
        [Required]
        [DataMember]
        public int PersonAliasID { get; set; }

        /// <summary>
        ///AssessmentTypeID
        /// /// </summary>
        /// <value>
        /// A <see cref="System.int"/> <c>false</c>.
        /// </value>
        [Required]
        [DataMember]
        public int AssessmentTypeID { get; set; }

        /// <summary>
        ///RequestorPersonAliasID
        /// /// </summary>
        /// <value>
        /// A <see cref="System.int"/> <c>false</c>.
        /// </value>
        [DataMember]
        public int? RequestorPersonAliasID { get; set; }


        /// <summary>
        ///RequestedDateTime
        /// /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> <c>false</c>.
        /// </value>
        [DataMember]
        public DateTime? RequestedDateTime { get; set; }


        /// <summary>
        ///RequestedDueDate
        /// /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> <c>false</c>.
        /// </value>
        [DataMember]
        public DateTime? RequestedDueDate { get; set; }


        /// <summary>
        ///Status
        /// /// </summary>
        /// <value>
        /// A <see cref="System.Enum"/> <c>false</c>.
        /// </value>
        [Required]
        [DataMember]
        public Status Status { get; set; }

        /// <summary>
        ///CompletedDateTime 
        /// /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> <c>false</c>.
        /// </value>
        [DataMember]
        public DateTime? CompletedDateTime { get; set; }

        /// <summary>
        ///AssessmentResultData  
        /// /// </summary>
        /// <value>
        /// A <see cref="System.String"/> <c>false</c>.
        /// </value>
        [StringLength( 100, MinimumLength = 3 )]
        [MaxLength]
        [DataMember]
        public string AssessmentResultData { get; set; }

        /// <summary>
        ///LastReminderDateTime  
        /// /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> <c>false</c>.
        /// </value>
        [DataMember]
        public DateTime? LastReminderDate { get; set; }

        #endregion
    }
    #region Enums
    /// <summary>
    ///Enums for Assessment Status
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// Pending Status
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Complete Status
        /// </summary>
        Complete = 1,
    }
    #endregion
}
