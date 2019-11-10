using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PollyDemo.Utility.Constants;

namespace PollyDemo.Controllers
{
    [Route("api/catalog")]
    public class CatalogController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CatalogController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [Route("getinfoproduct/{id}")]
        public async Task<IActionResult> GetInfoProduct(int id)
        {
            string requestEndpoint = $"product/getprice/{id}";

            var httpClient = _httpClientFactory.CreateClient(PollyConstants.RemoteServer);
            HttpResponseMessage response = await httpClient.GetAsync(requestEndpoint);

            if (response.IsSuccessStatusCode)
            {
                int productPrice = await response.Content.ReadAsAsync<int>();
                return Ok(productPrice);
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
        }
    }
}