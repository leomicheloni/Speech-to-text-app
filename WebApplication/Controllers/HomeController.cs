namespace WebApplication.Controllers
{
    using CognitiveLib;
    using System.Configuration;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;

    public class HomeController : Controller
    {
        string path;
        static SpeechToTextService service = new SpeechToTextService(ConfigurationManager.AppSettings["SuscriptionKey"]);
        public ActionResult Index()
        {
            path = Server.MapPath("~/App_Data");

            ViewBag.Title = "Home Page";

            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            path = Server.MapPath("~/App_Data");
            // Verify that the user selected a file
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);

                foreach (var fileToDelete in System.IO.Directory.GetFiles(path))
                {
                    System.IO.File.Delete(fileToDelete);
                }

                file.SaveAs(Path.Combine(path, fileName));
                return RedirectToAction("Transcript", new { fileName = fileName });
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Transcript(string fileName)
        {
            path = Server.MapPath("~/App_Data");

            service.Start(Path.Combine(path, fileName));

            return View();
        }

        [HttpGet]
        public ActionResult CurrentText()
        {
            return Json(service.LastMessage, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Download()
        {
            return base.File(Encoding.UTF8.GetBytes(service.LastMessage.Text), "text/plain", "result.txt");
        }
    }
}
