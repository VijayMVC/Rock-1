//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// SystemEmail Service class
    /// </summary>
    public partial class SystemEmailService : Service<SystemEmail>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemEmailService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public SystemEmailService(RockContext context) : base(context)
        {
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( SystemEmail item, out string errorMessage )
        {
            errorMessage = string.Empty;
 
            if ( new Service<WorkflowActionForm>( Context ).Queryable().Any( a => a.NotificationSystemEmailId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", SystemEmail.FriendlyTypeName, WorkflowActionForm.FriendlyTypeName );
                return false;
            }  
            return true;
        }
    }

    /// <summary>
    /// Generated Extension Methods
    /// </summary>
    public static partial class SystemEmailExtensionMethods
    {
        /// <summary>
        /// Clones this SystemEmail object to a new SystemEmail object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static SystemEmail Clone( this SystemEmail source, bool deepCopy )
        {
            if (deepCopy)
            {
                return source.Clone() as SystemEmail;
            }
            else
            {
                var target = new SystemEmail();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Copies the properties from another SystemEmail object to this SystemEmail object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this SystemEmail target, SystemEmail source )
        {
            target.Id = source.Id;
            target.Bcc = source.Bcc;
            target.Body = source.Body;
            target.CategoryId = source.CategoryId;
            target.Cc = source.Cc;
            target.From = source.From;
            target.FromName = source.FromName;
            target.IsSystem = source.IsSystem;
            target.Subject = source.Subject;
            target.Title = source.Title;
            target.To = source.To;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;

        }
    }
}
