using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace HayDeStacker
{
    [Produces("application/json")]
    [Route("api/racks")]
    [ApiController]
    public class RackController : Controller
    {
        private readonly Config Config;

        public RackController(Config config) => Config = config;

        [HttpGet]
        public ActionResult<IDictionary<string, RackConfig>> GetAll() => Config.Racks;
    }
}
