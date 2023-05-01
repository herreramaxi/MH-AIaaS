using AIaaS.WebAPI.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace AIaaS.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictController : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> PredictAsync()
        {

            var accessToken = Request.Headers[HeaderNames.Authorization];
            
            if (!accessToken.ToString().Contains("ZDk0YjUyZDYtZWQxNi00NWQwLTg2ZGUtOGM3NjBhNWM2Njcx"))
                return new StatusCodeResult(401);


            string content = await new StreamReader(Request.Body).ReadToEndAsync();

            //Random random = new();
            var output = new List<double>() { 12345.67, 2345.98 };
            return Ok(output);
            //foreach (var item in parameter.Input)
            //{
            //    var randomValue = random.Next(100) * 0.54;
            //    output.Add(randomValue);
            //}

            //return Ok(output);
        }
    }
}
