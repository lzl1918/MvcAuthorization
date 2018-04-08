using AuthorizationCore.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AuthorizationTest.Controllers
{
    public class TestController : Controller
    {
        public string Index([FromServices] IAuthorizationService<User> authorization)
        {
            var result = authorization.TryAuthorize("IsGreaterThanTree");
            return $"hello, index, {result}";
        }

    }
}