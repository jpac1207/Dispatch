using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Expedicao.DAL;
using Expedicao.Filters;
using Expedicao.Extensions;
using Expedicao.Models;

namespace Expedicao.Controllers
{
    public class ReportController : Controller
    {
        ExpedicaoContext db = new ExpedicaoContext();
        // GET: Report
        [AutenticationFilter]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetSends(string year)
        {
            int lvYear = DateTime.Now.Year;

            if (!string.IsNullOrEmpty(year))
            {
                if (!Int32.TryParse(year, out lvYear))
                    lvYear = DateTime.Now.Year;
            }

            var transactionsInYear = db.Transacoes.Where(x => x.Data.Year == lvYear).ToList();
            int[] sendsByMonth = new Int32[12];
            int[] receipts = new int[12];
            string[] monthNames = new String[] { "JAN", "FEV", "MAR", "ABR", "MAI", "JUN", "JUL", "AGO", "SET", "OUT", "NOV", "DEZ" };

            for (int i = 0; i < 12; i++)
            {
                int month = i + 1;
                sendsByMonth[i] = transactionsInYear.Where(x => x.StatusId == 2 && x.Data.Month == month).Count();
                receipts[i] = transactionsInYear.Where(x => (x.StatusId == 3) && x.Data.Month == month).Count();
            }

            return Json(new object[] { monthNames, sendsByMonth, receipts });
        }

        [HttpPost]
        public ActionResult GetSendsByMonth(string date)
        {
            try
            {
                DateTime lvDate = DateTime.Now;

                if (!string.IsNullOrEmpty(date))
                {
                    if (!DateTime.TryParse(date, out lvDate))
                        lvDate = DateTime.Now;
                }

                int daysInMonth = DateTime.DaysInMonth(lvDate.Year, lvDate.Month);
                var transactionsInMonth = db.Transacoes.Where(x => x.Data.Year == lvDate.Year && x.Data.Month == lvDate.Month).ToList();
                int[] sendsByDays = new Int32[daysInMonth];
                int[] receipts = new int[daysInMonth];
                string[] dayNames = new String[daysInMonth];

                for (int i = 0; i < daysInMonth; i++)
                {
                    int day = i + 1;
                    dayNames[i] = day.ToString();
                    sendsByDays[i] = transactionsInMonth.Where(x => x.StatusId == 2 && x.Data.Day == day).Count();
                    receipts[i] = transactionsInMonth.Where(x => x.StatusId == 3 && x.Data.Day == day).Count();
                }

                return Json(new object[] { dayNames, sendsByDays, receipts });
            }
            catch (Exception e)
            {
                DebugLog.Logar(e.Message);
                DebugLog.Logar(e.StackTrace);
                return Json(null);
            }
        }

        [HttpPost]
        public ActionResult GetSedeSends(string year)
        {
            int lvYear = DateTime.Now.Year;

            if (!string.IsNullOrEmpty(year))
            {
                if (!Int32.TryParse(year, out lvYear))
                    lvYear = DateTime.Now.Year;
            }

            var transactionsInYear = db.Transacoes.Where(x => x.Data.Year == lvYear).ToList();
            var sedes = db.Sedes.ToList();
            int[] sends = new int[sedes.Count()];
            int[] receipts = new int[sedes.Count()];
            string[] sedesNames = new string[sedes.Count()];
            int i = 0;

            foreach (Sede sede in sedes)
            {
                var sendsId = transactionsInYear.Where(x => x.StatusId == 2).Select(x => x.EnvioId).ToList();
                var receiptsId = transactionsInYear.Where(x => x.StatusId == 3).Select(x => x.EnvioId).ToList();

                sedesNames[i] = sede.Sigla;
                sends[i] = db.Envios.Where(x => sendsId.Contains(x.Id) && x.SedeOrigemId == sede.Id).Count();
                receipts[i] = db.Envios.Where(x => receiptsId.Contains(x.Id) && x.SedeDestinoId == sede.Id).Count();
                i++;
            }

            return Json(new Object[] { sedesNames, sends, receipts });
        }

        [HttpPost]
        public ActionResult GetSedeSendsByMonth(string date)
        {
            DateTime lvDate = DateTime.Now;

            if (!string.IsNullOrEmpty(date))
            {
                if (!DateTime.TryParse(date, out lvDate))
                    lvDate = DateTime.Now;
            }

            var transactionsInMonth = db.Transacoes.Where(x => x.Data.Month == lvDate.Month &&
                                                            x.Data.Year == lvDate.Year).ToList();
            var sedes = db.Sedes.ToList();
            var daysInMonth = DateTime.DaysInMonth(lvDate.Year, lvDate.Month);
            int[] sends = new int[sedes.Count()];
            int[] receipts = new int[sedes.Count()];
            string[] sedesNames = new string[sedes.Count()];
            int i = 0;

            foreach (Sede sede in sedes)
            {
                sedesNames[i] = sede.Sigla;
                var sendsId = transactionsInMonth.Where(x => x.StatusId == 2).Select(x => x.EnvioId).ToList();
                var receiptsId = transactionsInMonth.Where(x => x.StatusId == 3).Select(x => x.EnvioId).ToList();

                sends[i] = db.Envios.Where(x => sendsId.Contains(x.Id) && x.SedeOrigemId == sede.Id).Count();
                receipts[i] = db.Envios.Where(x => receiptsId.Contains(x.Id) && x.SedeDestinoId == sede.Id).Count();
                i++;
            }

            return Json(new Object[] { sedesNames, sends, receipts });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}