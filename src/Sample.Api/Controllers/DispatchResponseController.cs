namespace Sample.Api.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;
    using Microsoft.AspNetCore.Mvc;
    using Models;


    [ApiController]
    [Route("[controller]")]
    public class DispatchResponseController :
        ControllerBase
    {
        readonly IRequestClient<DispatchResponse> _client;

        public DispatchResponseController(IRequestClient<DispatchResponse> client)
        {
            _client = client;
        }

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] DispatchResponseModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Response<DispatchResponseCompleted> response = await _client.GetResponse<DispatchResponseCompleted>(new DispatchResponse
            {
                TransactionId = model.TransactionId,
                Body = model.Body,
                ResponseTimestamp = DateTime.UtcNow
            });

            return Ok(response.Message);
        }
    }
}
