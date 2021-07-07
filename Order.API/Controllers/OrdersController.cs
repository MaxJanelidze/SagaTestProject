using Messages;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;
using System.Threading.Tasks;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IBus _bus;

        public OrdersController(IBus bus)
        {
            _bus = bus;
        }

        [HttpPost]
        [Route("neworder")]
        public async Task<IActionResult> NewOrder([FromBody] CreateOrder request)
        {
            await _bus.Send(new OnNewOrder(request.Name, request.Quantity));

            return Ok();
        }
    }

    public class CreateOrder
    {
        public string Name { get; set; }

        public int Quantity { get; set; }
    }
}
