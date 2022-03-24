// Copyright saxu@microsoft.com.  All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataApiVersion.Models.v2;

namespace ODataApiVersion.Controllers.v2
{
    [ApiVersion("2")]
#if !USE_EXTENSIONS
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ODataRouteComponent("api/v2")]
#endif
    public class CustomersController : ODataController
    {
        private Customer[] customers = new Customer[]
        {
            new Customer
            {
                Id = 11,
                ApiVersion = "v2.0",
                FirstName = "YXS",
                LastName = "WU",
                Email = "yxswu@abc.com"
            },
            new Customer
            {
                Id = 12,
                ApiVersion = "v2.0",
                FirstName = "KIO",
                LastName = "XU",
                Email = "kioxu@efg.com"
            }
        };

        [HttpGet]
        [EnableQuery]
        public IQueryable<Customer> Get()
        {
            return customers.AsQueryable();
        }

        [HttpGet("{key}")]
        [EnableQuery]
        public IQueryable<Customer> Get(int key)
        {
            var customerQuery = customers.Where(c => c.Id == key)
                                         .AsQueryable();

            return customerQuery;
        }
    }
}
