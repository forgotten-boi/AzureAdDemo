using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace PocAadClient.Controllers
{
  [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            var userClaim = User.Identity as ClaimsIdentity;
            var userDetail = userClaim.Claims.Where(p=>p.Type.Equals("name")).FirstOrDefault();
            ViewBag.FullName = userDetail.Value;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Error(string message)
        {
            ViewBag.ErrorMessage = message;
            return View();
        }
    }
}