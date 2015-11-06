// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if EF7
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
#else
using System.Data.Entity.Infrastructure;
#endif
using System.Linq;
using System.Net;
using Microsoft.Restier.Samples.Northwind.Models;
using Microsoft.AspNet.Mvc;

namespace Microsoft.Restier.Samples.Northwind.Controllers
{
    public class NorthwindController : Controller
    {
        private NorthwindApi api;

        private NorthwindApi Api
        {
            get
            {
                if (api == null)
                {
                    api = new NorthwindApi();
                }

                return api;
            }
        }

        private NorthwindContext DbContext
        {
            get
            {
                return Api.Context;
            }
        }

        // OData Attribute Routing
        [HttpPut("Products({key})/UnitPrice")]
        public IActionResult UpdateProductUnitPrice(int key, [FromBody]decimal price)
        {
            var entity = DbContext.Products.FirstOrDefault(e => e.ProductID == key);
            if (entity == null)
            {
                return HttpNotFound();
            }
            entity.UnitPrice = price;

            try
            {
                DbContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DbContext.Products.Any(p => p.ProductID == key))
                {
                    return HttpNotFound();
                }
                else
                {
                    throw;
                }
            }
            return Ok(price);
        }

        [HttpGet("Products/Microsoft.Restier.Samples.Northwind.Models.MostExpensive")]
        public IActionResult MostExpensive()
        {
            var product = DbContext.Products.Max(p => p.UnitPrice);
            return Ok(product);
        }

        [HttpPost("Products({key})/Microsoft.Restier.Samples.Northwind.Models.IncreasePrice")]
        public IActionResult IncreasePrice([FromRoute] int key, [FromHeader] int diff)
        {
            var entity = DbContext.Products.FirstOrDefault(e => e.ProductID == key);
            if (entity == null)
            {
                return HttpNotFound();
            }
            entity.UnitPrice = entity.UnitPrice + diff;

            try
            {
                DbContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DbContext.Products.Any(p => p.ProductID == key))
                {
                    return HttpNotFound();
                }
                else
                {
                    throw;
                }
            }
            return new NoContentResult();
        }

        [HttpPost("ResetDataSource")]
        public IActionResult ResetDataSource()
        {
            DbContext.ResetDataSource();
            return new NoContentResult();
        }

        /// <summary>
        /// Disposes the API and the controller.
        /// </summary>
        /// <param name="disposing">Indicates whether disposing is happening.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.api != null)
                {
                    this.api.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
