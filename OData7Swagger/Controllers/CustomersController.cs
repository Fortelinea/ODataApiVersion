// Copyright saxu@microsoft.com.  All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using OData7Swagger.Models;

namespace OData7Swagger.Controllers
{
    //[ODataRoutePrefix("Customers")]
    public class CustomersController : ODataController
    {
        private Customer[] customers = new Customer[]
        {
            new Customer
            {
                Id = 1,
                Name = "Sam",
                PhoneNumber = "111-222-3333"
            },
            new Customer
            {
                Id = 2,
                Name = "Peter",
                PhoneNumber = "456-ABC-8888"
            }
        };

        [ODataRoute]
        [HttpGet]
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(customers);
        }

        [HttpGet("{key}")]
        [EnableQuery]
        public IActionResult Get(int key)
        {
            var customer = customers.FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound($"Cannot find customer with Id={key}.");
            }

            return Ok(customer);
        }
    }
}
