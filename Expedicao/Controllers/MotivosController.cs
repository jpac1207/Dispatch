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
    public class MotivosController : Controller
    {
        private ExpedicaoContext db = new ExpedicaoContext();

        // GET: Motivos
        public ActionResult Index()
        {
            return View(db.Motivos.ToList());
        }

        // GET: Motivos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Motivo motivo = db.Motivos.Find(id);
            if (motivo == null)
            {
                return HttpNotFound();
            }
            return View(motivo);
        }

        // GET: Motivos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Motivos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Nome")] Motivo motivo)
        {
            if (ModelState.IsValid)
            {
                db.Motivos.Add(motivo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(motivo);
        }

        // GET: Motivos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Motivo motivo = db.Motivos.Find(id);
            if (motivo == null)
            {
                return HttpNotFound();
            }
            return View(motivo);
        }

        // POST: Motivos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nome")] Motivo motivo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(motivo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(motivo);
        }

        // GET: Motivos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Motivo motivo = db.Motivos.Find(id);
            if (motivo == null)
            {
                return HttpNotFound();
            }
            return View(motivo);
        }

        // POST: Motivos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Motivo motivo = db.Motivos.Find(id);
            db.Motivos.Remove(motivo);
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
