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
    public class DispatchRequestController :
        ControllerBase
    {
        readonly IRequestClient<DispatchRequest> _client;

        public DispatchRequestController(IRequestClient<DispatchRequest> client)
        {
            _client = client;
        }

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] DispatchRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Response<DispatchRequestCompleted> response = await _client.GetResponse<DispatchRequestCompleted>(new DispatchRequest
            {
                TransactionId = model.TransactionId,
                RoutingKey = model.RoutingKey,
                Body = model.Body,
                ReceiveTimestamp = DateTime.UtcNow
            });

            return Ok(response.Message);
        }
    }
}