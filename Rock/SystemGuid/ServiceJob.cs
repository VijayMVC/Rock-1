﻿// <copyright>
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

namespace Rock.SystemGuid
{
    /// <summary>
    /// Service Job guids
    /// </summary>
    public static class ServiceJob
    {
        /// <summary>
        /// Gets the Job Pulse guid
        /// </summary>
        public const string JOB_PULSE = "CB24FF2A-5AD3-4976-883F-DAF4EFC1D7C7";

        /// <summary>
        /// The Job for migrating attendance records to occurrence recors (in V8)
        /// </summary>
        public const string MIGRATE_ATTENDANCE_OCCURRENCE = "98A2DCA5-5E2E-482A-A7CA-15DAD5B4EA65";

        /// <summary>
        /// The Job for migrating family check-in identifiers to person alternate ids
        /// </summary>
        public const string MIGRATE_FAMILY_CHECKIN_IDS = "E782C667-EF07-4AD2-86B7-01C1935AAF5B";

        /// <summary>
        /// The Job to get NCOA
        /// </summary>
        public const string GET_NCOA = "D2D6EA6C-F94A-39A0-481B-A23D08B887D6";
    }
}