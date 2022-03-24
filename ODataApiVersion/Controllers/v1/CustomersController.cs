// Copyright saxu@microsoft.com.  All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataApiVersion.Models.v1;

namespace ODataApiVersion.Controllers.v1
{
    [ApiVersion("1")]
#if !USE_EXTENSIONS
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ODataRouteComponent("api/v1")]
#endif
    public class CustomersController : ODataController
    {
        private Customer[] customers = new Customer[]
        {
            new Customer
            {
                Id = 1,
                ApiVersion = "v1.0",
                Name = "Sam",
                PhoneNumber = "111-222-3333"
            },
            new Customer
            {
                Id = 2,
                ApiVersion = "v1.0",
                Name = "Peter",
                PhoneNumber = "456-ABC-8888"
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
