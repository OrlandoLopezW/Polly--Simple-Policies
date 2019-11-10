using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PollyDemo.Controllers
{
    [Route("api/product")]
    public class ProductController : Controller
    {
        private static int countRequest = 0;

        [Route("getprice/{id}")]
        public async Task<IActionResult> GetPrice(int id)
        {
            countRequest += 1;

            if (countRequest % 5 == 0)
            {
                return Ok(15);
            }

            return StatusCode((int)HttpStatusCode.InternalServerError, "internal error");
        }
    }
}