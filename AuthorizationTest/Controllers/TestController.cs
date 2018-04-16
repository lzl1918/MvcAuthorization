using AuthorizationCore;
using AuthorizationCore.Attributes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AuthorizationTest.Controllers
{
    [UserAuthorizationRequired("IsAgeGreaterThan18", AuthorizationFailedAction.KeepUnauthorized)]
    public class TestController : Controller
    {
        [UserAuthorizationRequired("IsGreaterThanTree", AuthorizationFailedAction.KeepUnauthorized)]
        public string Index([FromServices] IAuthorizationResult result)
        {
            return $"hello, index, {result}";
        }

    }
}