using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AliceBlueWrapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AliceBlueAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SharviController : ControllerBase
    {
        private readonly ILogger<SharviController> _logger;
        AliceBlue aliceBlue;
        string token = "f-y0GrDvDh8h_I4kkHd1birxinxHP0kG7bsgo9DxoWM.ZCtx16J6up6b6nmPk-1tQAWhJ_D2PQtWmLDCwpr7qEs";
        public SharviController(ILogger<SharviController> logger)
        {
            _logger = logger;
            aliceBlue = new AliceBlue();
        }

        //[HttpGet]
        //[Route("token")]
        //public async Task<dynamic> GetToken()
        //{
        //    return  await aliceBlue.LoginAndGetToken();           
        //}

        [HttpGet]
        [Route("profile")]
        public async Task<dynamic> GetProfile()
        {
            return await aliceBlue.GetProfile("f-y0GrDvDh8h_I4kkHd1birxinxHP0kG7bsgo9DxoWM.ZCtx16J6up6b6nmPk-1tQAWhJ_D2PQtWmLDCwpr7qEs");
        }

        [HttpGet]
        [Route("balance")]
        public async Task<dynamic> getBalance()
        {
            return await aliceBlue.GetCash("f-y0GrDvDh8h_I4kkHd1birxinxHP0kG7bsgo9DxoWM.ZCtx16J6up6b6nmPk-1tQAWhJ_D2PQtWmLDCwpr7qEs");
        }

      

        //[HttpGet]
        //[Route("order")]
        //public async Task<dynamic> PlaceOrder()
        //{
        //    return await aliceBlue.PlaceOrder("9XeScdQ9r9-m0FBue5cQve0Xo1ObFxbrgClCx9wcv9E.hdm1wkdRcKjhyL-33QWDJ0W8QfdMH0Qhk4PmWK65QD8");
        //}
    }
}
