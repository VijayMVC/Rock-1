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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents an assessment.
    /// </summary>
    [RockDomain( "CRM" )]
    [Table( "Assessment" )]
    [DataContract]
    public partial class Assessment : Model<Assessment>
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
        public int PersonAliasId { get; set; }

        /// <summary>
        ///AssessmentTypeID
        /// /// </summary>
        /// <value>
        /// A <see cref="System.int"/> <c>false</c>.
        /// </value>
        [Required]
        [DataMember]
        public int AssessmentTypeId { get; set; }

        /// <summary>
        ///RequestorPersonAliasID
        /// /// </summary>
        /// <value>
        /// A <see cref="System.int"/> <c>false</c>.
        /// </value>
        [DataMember]
        public int? RequesterPersonAliasId { get; set; }

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
        public AssessmentRequestStatus Status { get; set; }

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

        #region Virtual Properties
        
        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.AssessmentType"/> that represents the type of the assessment.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.AssessmentType"/> that represents the type of the assessment.
        /// </value>
        [DataMember]
        public virtual AssessmentType AssessmentType { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the requester person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [LavaInclude]
        public virtual PersonAlias RequesterPersonAlias { get; set; }

        #endregion

        #region Public Methods

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Assessment Configuration class.
    /// </summary>
    public partial class AssessmentConfiguration : EntityTypeConfiguration<Assessment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssessmentConfiguration" /> class.
        /// </summary>
        public AssessmentConfiguration()
        {
            this.HasRequired( a => a.AssessmentType ).WithMany().HasForeignKey( a => a.AssessmentTypeId ).WillCascadeOnDelete( true );
            this.HasRequired( a => a.PersonAlias ).WithMany().HasForeignKey( a => a.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.RequesterPersonAlias ).WithMany().HasForeignKey( a => a.RequesterPersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations
    /// <summary>
    ///Enums for Assessment Status
    /// </summary>
    public enum AssessmentRequestStatus
    {
        /// <summary>
        /// Pending Status
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Complete Status
        /// </summary>
        Complete = 1,

        /// <summary>
        /// Available Status
        /// </summary>
        Available = 2,
    }
    #endregion
}
