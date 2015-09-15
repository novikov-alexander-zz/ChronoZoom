using System;
using System.Web.Mvc;

namespace Chronozoom.UI.UnitTests.Controllers
{
    public class JasmineController : Controller
    {
        public ViewResult Run()
        {
            return View("SpecRunner");
        }
    }
}
