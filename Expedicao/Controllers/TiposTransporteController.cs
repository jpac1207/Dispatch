using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Expedicao.DAL;
using Expedicao.Models;
using Expedicao.Filters;

namespace Expedicao.Controllers
{
    [AdminFilter]
    public class TiposTransporteController : Controller
    {
        private ExpedicaoContext db = new ExpedicaoContext();

        // GET: TiposTransporte
        public ActionResult Index()
        {
            return View(db.TipoTransportes.ToList());
        }

        // GET: TiposTransporte/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoTransporte tipoTransporte = db.TipoTransportes.Find(id);
            if (tipoTransporte == null)
            {
                return HttpNotFound();
            }
            return View(tipoTransporte);
        }

        // GET: TiposTransporte/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TiposTransporte/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Nome")] TipoTransporte tipoTransporte)
        {
            if (ModelState.IsValid)
            {
                db.TipoTransportes.Add(tipoTransporte);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tipoTransporte);
        }

        // GET: TiposTransporte/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoTransporte tipoTransporte = db.TipoTransportes.Find(id);
            if (tipoTransporte == null)
            {
                return HttpNotFound();
            }
            return View(tipoTransporte);
        }

        // POST: TiposTransporte/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nome")] TipoTransporte tipoTransporte)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tipoTransporte).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tipoTransporte);
        }

        // GET: TiposTransporte/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoTransporte tipoTransporte = db.TipoTransportes.Find(id);
            if (tipoTransporte == null)
            {
                return HttpNotFound();
            }
            return View(tipoTransporte);
        }

        // POST: TiposTransporte/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TipoTransporte tipoTransporte = db.TipoTransportes.Find(id);
            db.TipoTransportes.Remove(tipoTransporte);
            db.SaveChanges();
            return RedirectToAction("Index");
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
