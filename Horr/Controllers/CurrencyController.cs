using Microsoft.AspNetCore.Mvc;
using ServiceContracts.Currency;
using System.Threading.Tasks;

namespace Horr.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyConverterService _currencyConverter;

        public CurrencyController(ICurrencyConverterService currencyConverter)
        {
            _currencyConverter = currencyConverter;
        }

        [HttpGet("convert")]
        public async Task<IActionResult> ConvertCurrency([FromQuery] decimal amount, [FromQuery] string from, [FromQuery] string to)
        {
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
                return BadRequest("From and To currencies are required.");

            try
            {
                var converted = await _currencyConverter.ConvertAsync(amount, from, to);
                return Ok(new { amount = converted, currency = to.ToUpper() });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
