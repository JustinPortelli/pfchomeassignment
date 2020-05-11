using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.DataAccess;

namespace WebApplication1.Controllers
{
    public class CronController : Controller
    {
        // GET: Cron
        public ActionResult RunEveryMinute()
        {
            try
            {
                PubSubRepository psr = new PubSubRepository();
                psr.DownloadEmailFromQueueAndSend();
                ViewBag.Message = "Email sent";
                LogsRepository lr = new LogsRepository();
                lr.WriteLogEntry("CRON was added");
            }
            catch (Exception ex)
            {
                LogsRepository lr = new LogsRepository();
                lr.LogError(ex);
            }
            return Content("Sent");
        }
    }
}