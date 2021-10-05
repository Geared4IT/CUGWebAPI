using cugonlineWebAPI.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace cugonlineWebAPI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            //return View();
            return Redirect("~/index.html");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(EmailModelDTO model)
        {
            using (var client = new HttpClient())
            {
                //Passing service base url    
                client.BaseAddress = new Uri("https://cugonline.co.za/");

                client.DefaultRequestHeaders.Clear();
                //Define request data format    
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Sending request to find web api REST service using HttpClient    
                HttpResponseMessage Res = await client.PostAsJsonAsync("api/email/send-email", model);

                //Checking the response is successful or not which is sent using HttpClient    
                if (Res.IsSuccessStatusCode)
                {
                    return View("Success");
                }
                else
                {
                    return View("Error");
                }
            }
        }
    }
}
