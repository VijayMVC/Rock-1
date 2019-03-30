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
using System.Linq;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class FinancialBatchesController
    {
        /// <summary>
        /// Gets the control totals.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/FinancialBatches/GetControlTotals/{id}" )]
        public ControlTotalResult GetControlTotals( int id )
        {
            var financialTransactionsQuery = new FinancialTransactionService( this.Service.Context as Rock.Data.RockContext ).Queryable().Where( a => a.BatchId == id );
            var controlTotalCount = financialTransactionsQuery.Count();
            var controlTotalAmount = financialTransactionsQuery.SelectMany( a => a.TransactionDetails ).Sum( a => ( decimal? ) a.Amount ) ?? 0.00M;

            return new ControlTotalResult
            {
                ControlTotalCount = controlTotalCount,
                ControlTotalAmount = controlTotalAmount
            };
        }
    }
}
