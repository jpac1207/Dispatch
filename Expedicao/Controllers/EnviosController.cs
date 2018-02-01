using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Expedicao.DAL;
using Expedicao.Models;
using Expedicao.Extensions;
using System.Web.ModelBinding;
using System.Web.UI.WebControls;
using Expedicao.Filters;
using System.IO;
using System.Web.UI;
using Newtonsoft.Json;

namespace Expedicao.Controllers
{
    [AutenticationFilter]
    public class EnviosController : Controller
    {
        private ExpedicaoContext db = new ExpedicaoContext();
        private UserContext dbUser = new UserContext();

        // GET: Envios       
        public ActionResult Index([Form] QueryOptions queryOptions)
        {
            queryOptions.SortOrder = SortOrder.DESC;
            List<Envio> sends = ApplyFilters(null);
            var start = (queryOptions.CurrentPage - 1) * queryOptions.PageSize;
            queryOptions.TotalPages = (int)Math.Ceiling((double)sends.Count() / queryOptions.PageSize);
            ViewBag.QueryOptions = queryOptions;

            if (TempData["message"] != null)
                ViewBag.Message = TempData["message"].ToString();
            if (TempData["class"] != null)
                ViewBag.Class = TempData["class"].ToString();

            sends = sends.OrderBy(queryOptions.Sort).Skip(start).Take(queryOptions.PageSize).ToList();
            GetCurrentStatus(sends);
            FillSelects();

            return View("Index", sends);
        }

        public ActionResult Received([Form] QueryOptions queryOptions)
        {
            try
            {
                var userId = Session["userId"];

                if (userId == null)
                {
                    Response.Flush();
                    Response.Redirect("http://172.20.15.19/gasag", true);
                }

                int lvUserId = Int32.Parse(userId.ToString());
                User user = dbUser.Users.Where(x => x.Id == lvUserId).FirstOrDefault();
                var sedeId = user.SedeId;
                var sends = db.Envios.Where(x => x.SedeDestinoId == sedeId).ToList();

                GetCurrentStatus(sends);
                sends = sends.Where(x => x.CurrentStatus.Equals("Enviado") || x.CurrentStatus.Equals("Recebido")).ToList();
                sends = ApplyFilters(sends);

                var start = (queryOptions.CurrentPage - 1) * queryOptions.PageSize;
                queryOptions.TotalPages = (int)Math.Ceiling((double)sends.Count() / queryOptions.PageSize);
                ViewBag.QueryOptions = queryOptions;

                if (TempData["message"] != null)
                    ViewBag.Message = TempData["message"].ToString();
                if (TempData["class"] != null)
                    ViewBag.Class = TempData["class"].ToString();

                sends = sends.OrderBy(queryOptions.Sort).Skip(start).Take(queryOptions.PageSize).ToList();
                FillSelects();

                return View("Received", sends);
            }
            catch (Exception e)
            {
                DebugLog.Logar(e.Message);
                DebugLog.Logar(e.StackTrace);
                Response.Redirect("http://172.20.15.19/gasag", true);

                return View(new List<Envio>());
            }

        }

        // GET: Envios/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Envio envio = db.Envios.Find(id);
            if (envio == null)
            {
                return HttpNotFound();
            }
            return View(envio);
        }

        // GET: Envios/Create     
        public ActionResult Create()
        {
            ViewBag.SedeOrigemId = new SelectList(db.Sedes, "Id", "Nome");
            ViewBag.SedeDestinoId = new SelectList(db.Sedes, "Id", "Nome");
            ViewBag.MotivoId = new SelectList(db.Motivos, "Id", "Nome");
            ViewBag.TipoTransporteId = new SelectList(db.TipoTransportes, "Id", "Nome");
            return View();
        }

        // POST: Envios/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,OrdemManutencao,Descricao,Quantidade,NumeroSerie,TipoTransporteId,"+
                                                   "NotaFiscal,MotivoId,NumeroImpressaoNota,NotaTransferenciaSap,IdSolicitacao,"+
                                                   "SedeOrigemId, SedeDestinoId")] Envio envio)
        {
            if (ModelState.IsValid)
            {
                string message = "";
                string cssClass = "";

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.Envios.Add(envio);
                        /*Creating transaction. In this case, the expediction will waiting the send confirmation*/
                        Transacao createTransaction = new Transacao();
                        createTransaction.Data = DateTime.Now;
                        createTransaction.EnvioId = envio.Id;
                        createTransaction.StatusId = db.Status.Where(x => x.Id == 1).FirstOrDefault().Id;

                        if (ConfigurationManager.AppSettings["SECURITY_USER"].Equals("true"))
                        {
                            var userId = Session["userId"].ToString();
                            createTransaction.MatriculaModificador = userId;
                        }

                        db.Transacoes.Add(createTransaction);
                        db.SaveChanges();
                        transaction.Commit();

                        message = "Envio Cadastrado com Sucesso!";
                        cssClass = "alert-success";
                    }
                    catch (Exception e)
                    {
                        message = "Não Foi Possível Cadastrar o Envio!";
                        cssClass = "alert-danger";
                        transaction.Rollback();
                        DebugLog.Logar(e.Message);
                        DebugLog.Logar(e.StackTrace);
                    }
                }
                TempData["message"] = message;
                TempData["class"] = cssClass;

                return RedirectToAction("Index", TempData);
            }

            return View(envio);
        }

        // GET: Envios/Edit/5
        [AdminFilter]
        public ActionResult Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Envio envio = db.Envios.Find(id);
                if (envio == null)
                {
                    return HttpNotFound();
                }

                if (ConfigurationManager.AppSettings["SECURITY_USER"].ToString().Equals("true"))
                {
                    var userId = Session["userId"];
                    var appId = ConfigurationManager.AppSettings["APP_ID"].ToString();

                    if (userId == null)
                    {
                        Response.Headers.Clear();
                        Response.Redirect("http://172.20.15.19/gasag", true);
                    }
                    else
                    {
                        int lvUserId = Int32.Parse(userId.ToString());
                        int lvAppId = Int32.Parse(appId);
                        dbUser = new UserContext();

                        GrantedUser grantedUser = dbUser.GrantedUsers.Where(x => x.UserId == lvUserId &&
                                                                            x.AppId == lvAppId && x.GrantId == 2).FirstOrDefault();

                        if (grantedUser == null)
                        {
                            return View("Index");
                        }
                        else if (grantedUser.GrantId == 1)
                        {
                            //only allows edition if status is created or sended
                            var lastTransaction = db.Transacoes.Where(x => x.EnvioId == envio.Id).OrderByDescending(x => x.Id).FirstOrDefault();

                            if (lastTransaction.StatusId != 1 && lastTransaction.StatusId != 2)
                            {
                                TempData["message"] = "Não é possível editar materiais que já foram recebidos!";
                                TempData["class"] = "alert-danger";

                                return View("Index");
                            }
                        }
                    }
                }

                ViewBag.SedeOrigemId = new SelectList(db.Sedes, "Id", "Nome", envio.SedeOrigemId);
                ViewBag.SedeDestinoId = new SelectList(db.Sedes, "Id", "Nome", envio.SedeDestinoId);
                ViewBag.MotivoId = new SelectList(db.Motivos, "Id", "Nome", envio.MotivoId);
                ViewBag.TipoTransporteId = new SelectList(db.TipoTransportes, "Id", "Nome", envio.TipoTransporteId);
                return View(envio);
            }
            catch (Exception e)
            {
                DebugLog.Logar(e.Message);
                DebugLog.Logar(e.StackTrace);
                TempData["message"] = "Ocorreu um erro na solicitação!";
                TempData["class"] = "alert-danger";

                return View("Index");
            }
        }

        // POST: Envios/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,OrdemManutencao,Descricao,Quantidade,NumeroSerie,TipoTransporteId,"+
                                                 "NotaFiscal,MotivoId,NumeroImpressaoNota,NotaTransferenciaSap,IdSolicitacao,"+
                                                 "SedeOrigemId,SedeDestinoId")] Envio envio)
        {
            if (ModelState.IsValid)
            {
                db.Entry(envio).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(envio);
        }

        // GET: Envios/Delete/5
        [AdminFilter]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Envio envio = db.Envios.Where(x => x.Id == id).Include(x => x.SedeOrigem).
                                    Include(x => x.SedeDestino).Include(x => x.Motivo).
                                    Include(x => x.TipoTransporte).FirstOrDefault();
            if (envio == null)
            {
                return HttpNotFound();
            }
            return View(envio);
        }

        // POST: Envios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            string message = "";
            string cssClass = "";

            using (var dbTransactions = db.Database.BeginTransaction())
            {
                try
                {
                    Envio envio = db.Envios.Find(id);

                    /*Get and exclude all transactions for this send*/
                    var transactions = db.Transacoes.Where(x => x.EnvioId == envio.Id).ToList();
                    transactions.ForEach(x => db.Transacoes.Remove(x));

                    db.Envios.Remove(envio);
                    db.SaveChanges();
                    dbTransactions.Commit();
                    message = "Envio excluido com sucesso!";
                    cssClass = "alert-success";
                }
                catch (Exception e)
                {
                    dbTransactions.Rollback();
                    DebugLog.Logar(e.Message);
                    DebugLog.Logar(e.StackTrace);
                    message = "Erro ao excluir envio!";
                    cssClass = "alert-danger";
                }
            }

            TempData["message"] = message;
            TempData["class"] = cssClass;
            return RedirectToAction("Index");
        }

        public ActionResult SendAll([Form] QueryOptions queryOptions)
        {
            string message = "";
            string cssClass = "";
            List<Envio> sends = new List<Envio>();

            try
            {
                var start = (queryOptions.CurrentPage - 1) * queryOptions.PageSize;
                queryOptions.TotalPages = (int)Math.Ceiling((double)db.Envios.Count() / queryOptions.PageSize);
                ViewBag.QueryOptions = queryOptions;

                sends = db.Envios.OrderBy(queryOptions.Sort).Skip(start).Take(queryOptions.PageSize).
                   Include(x => x.SedeDestino).Include(x => x.TipoTransporte).Include(x => x.Motivo).ToList();

                sends.ForEach(x =>
                {
                    var lastTransaction = db.Transacoes.Where(y => y.EnvioId == x.Id).Select(y => y).
                                          OrderByDescending(y => y.Id).FirstOrDefault();

                    if (lastTransaction != null)
                    {
                        if (lastTransaction.StatusId == 1)
                        {
                            var newStatus = db.Status.Where(z => z.Id == 2).FirstOrDefault();

                            if (newStatus != null)
                            {
                                Transacao t = new Transacao();
                                t.EnvioId = x.Id;
                                t.Data = DateTime.Now;
                                t.StatusId = newStatus.Id;

                                if (ConfigurationManager.AppSettings["SECURITY_USER"].Equals("true"))
                                {
                                    var userId = Session["userId"].ToString();
                                    t.MatriculaModificador = userId;
                                }

                                db.Transacoes.Add(t);
                                x.CurrentStatus = newStatus.Nome;
                            }
                        }
                        else
                        {
                            var currentStatus = db.Status.Where(z => z.Id == lastTransaction.StatusId).FirstOrDefault();
                            x.CurrentStatus = currentStatus.Nome;
                        }
                    }
                });

                db.SaveChanges();
                message = "Os materiais dessa página foram enviados com sucesso!";
                cssClass = "alert-success";
            }
            catch (Exception e)
            {
                message = "Não foi possível enviar os materiais dessa página";
                cssClass = "alert-danger";
                DebugLog.Logar(e.Message);
                DebugLog.Logar(e.StackTrace);
            }

            ViewBag.Message = message;
            ViewBag.Class = cssClass;
            FillSelects();

            return View("Index", sends);
        }

        public ActionResult Confirm([Form] QueryOptions queryOptions, int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                Envio envio = db.Envios.Find(id);

                if (envio == null)
                {
                    return HttpNotFound();
                }

                if (id.HasValue)
                {
                    var lastTransaction = db.Transacoes.Where(y => y.EnvioId == envio.Id).Select(y => y).
                                                            OrderByDescending(y => y.Id).FirstOrDefault();

                    if (lastTransaction != null)
                    {
                        //Material Enviado
                        if (lastTransaction.StatusId == 2)
                        {
                            var newStatus = db.Status.Where(z => z.Id == 3).FirstOrDefault();

                            if (newStatus != null)
                            {
                                Transacao t = new Transacao();
                                t.EnvioId = envio.Id;
                                t.Data = DateTime.Now;
                                t.StatusId = newStatus.Id;

                                if (ConfigurationManager.AppSettings["SECURITY_USER"].Equals("true"))
                                {
                                    var userId = Session["userId"].ToString();
                                    t.MatriculaModificador = userId;
                                }

                                db.Transacoes.Add(t);
                                envio.CurrentStatus = newStatus.Nome;
                                db.SaveChanges();
                                TempData["message"] = "Material confirmado com sucesso!";
                                TempData["class"] = "alert-success";
                            }

                            var path = Request.UrlReferrer.AbsolutePath;

                            if (path != null)
                            {
                                if (path.Contains("Received"))
                                {
                                    //requisition is of Received page
                                    return Received(queryOptions);
                                }
                            }
                            return Index(queryOptions);
                        }
                        else
                        {
                            TempData["message"] = "Não há detalhes a modificar!";
                            TempData["class"] = "alert-success";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DebugLog.Logar(e.Message);
                DebugLog.Logar(e.StackTrace);
            }

            return Received(queryOptions);
        }

        public ActionResult Send([Form] QueryOptions queryOptions, int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                Envio envio = db.Envios.Find(id);

                if (envio == null)
                {
                    return HttpNotFound();
                }

                if (id.HasValue)
                {
                    var lastTransaction = db.Transacoes.Where(y => y.EnvioId == envio.Id).Select(y => y).
                                                            OrderByDescending(y => y.Id).FirstOrDefault();

                    if (lastTransaction != null)
                    {
                        //Material Enviado
                        //aguardando envio
                        if (lastTransaction.StatusId == 1)
                        {
                            var newStatus = db.Status.Where(z => z.Id == 2).FirstOrDefault();

                            if (newStatus != null)
                            {
                                Transacao t = new Transacao();
                                t.EnvioId = envio.Id;
                                t.Data = DateTime.Now;
                                t.StatusId = newStatus.Id;

                                if (ConfigurationManager.AppSettings["SECURITY_USER"].Equals("true"))
                                {
                                    var userId = Session["userId"].ToString();
                                    t.MatriculaModificador = userId;
                                }

                                db.Transacoes.Add(t);
                                envio.CurrentStatus = newStatus.Nome;
                                db.SaveChanges();
                                TempData["message"] = "Material enviado com sucesso!";
                                TempData["class"] = "alert-success";
                            }

                            return Index(queryOptions);
                        }
                        else
                        {
                            TempData["message"] = "Não há detalhes a modificar!";
                            TempData["class"] = "alert-success";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DebugLog.Logar(e.Message);
                DebugLog.Logar(e.StackTrace);
            }

            return Index(queryOptions);
        }

        public ActionResult ExportAll()
        {
            //configure to get all sends
            QueryOptions opt = new QueryOptions();
            opt.CurrentPage = 1;
            opt.PageSize = db.Envios.Count();

            return Export(opt, null);
        }

        public ActionResult Export([Form] QueryOptions queryOptions, int? id)
        {
            try
            {
                var start = (queryOptions.CurrentPage - 1) * queryOptions.PageSize;
                queryOptions.TotalPages = (int)Math.Ceiling((double)db.Envios.Count() / queryOptions.PageSize);
                var sends = db.Envios.ToList();
                sends = ApplyFilters(sends);
                DataTable dt = null;

                if (id != null && id.HasValue)
                {
                    sends = sends.Where(x => x.SedeDestinoId == id).ToList();
                    GetCurrentStatus(sends);
                    sends = sends.Where(x => x.CurrentStatus.Equals("Enviado") || x.CurrentStatus.Equals("Recebido")).ToList();
                }
                else
                {
                    GetCurrentStatus(sends);
                }

                sends = sends.OrderBy(queryOptions.Sort).Skip(start).Take(queryOptions.PageSize).ToList();
                dt = Utility.ExportListToDataTable(sends);
                TreatingSendsData(dt);

                var gridView = new GridView();
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                string fileName = "Export_" + DateTime.Now.ToString("dd.MM.yyyy") + ".xls";

                gridView.DataSource = dt;
                gridView.DataBind();
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + fileName);
                Response.ContentType = "application/ms-excel";
                Response.Charset = "";
                gridView.RenderControl(htw);
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();

            }
            catch (Exception e)
            {
                DebugLog.Logar(e.Message);
                DebugLog.Logar(e.StackTrace);
            }

            return Index(new QueryOptions());
        }

        public ActionResult MultipleSend()
        {
            ViewBag.SedeOrigemId = new SelectList(db.Sedes, "Id", "Nome");
            ViewBag.SedeDestinoId = new SelectList(db.Sedes, "Id", "Nome");
            ViewBag.TipoTransporteId = new SelectList(db.TipoTransportes, "Id", "Nome");
            ViewBag.MotivoId = new SelectList(db.Motivos, "Id", "Nome");
            return View();
        }

        [HttpPost]
        public ActionResult SendInBatch(List<Envio> sends, DateTime? sendDate)
        {
            try
            {
                //validating sends
                int qtdOfValidSends = sends.Where(x => !string.IsNullOrEmpty(x.Descricao) &&
                                                       x.Quantidade > 0).ToList().Count();

                if (sends.Count != qtdOfValidSends)
                {
                    return Json(new object[] { "Alguns envios não são válidos, verifique os campos!", "alert-danger" });
                }
                else
                {
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            foreach (Envio send in sends)
                            {
                                db.Envios.Add(send);
                                db.SaveChanges();

                                Transacao createTransaction = new Transacao();
                                createTransaction.EnvioId = send.Id;
                                createTransaction.Data = sendDate.HasValue ? sendDate.Value : DateTime.Now;
                                createTransaction.StatusId = db.Status.Where(x => x.Nome.Equals("Aguardando Envio")).FirstOrDefault().Id;

                                Transacao sendTransaction = new Transacao();
                                sendTransaction.EnvioId = send.Id;
                                sendTransaction.Data = sendDate.HasValue ? sendDate.Value : DateTime.Now;
                                sendTransaction.StatusId = db.Status.Where(x => x.Nome.Equals("Enviado")).FirstOrDefault().Id;

                                if (ConfigurationManager.AppSettings["SECURITY_USER"].Equals("true"))
                                {
                                    var userId = Session["userId"].ToString();
                                    createTransaction.MatriculaModificador = userId;
                                    sendTransaction.MatriculaModificador = userId;
                                }

                                db.Transacoes.Add(createTransaction);
                                db.Transacoes.Add(sendTransaction);
                                db.SaveChanges();
                            }
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            DebugLog.Logar(ex.Message);
                            DebugLog.Logar(ex.StackTrace);
                            DebugLog.Logar(Utility.Details(ex));
                            return Json(new object[] { "Erro ao Enviar!", "alert-danger" });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DebugLog.Logar(e.Message);
                DebugLog.Logar(e.StackTrace);
                Utility.Details(e);
                return Json(new object[] { "Erro ao Enviar!", "alert-danger" });
            }
            return Json(new object[] { "Sucesso ao enviar!", "alert-success" });
        }

        private void FillSelects()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            SelectListItem first = new SelectListItem();
            SelectListItem selectedItem = null;

            var status = db.Status.ToList();
            status.ForEach(x =>
            {
                SelectListItem item = new SelectListItem();
                item.Text = x.Nome;
                item.Value = x.Id.ToString();
                items.Add(item);
            });

            first.Text = "Status";
            first.Value = "0";
            items.Insert(0, first);

            if (TempData["status"] != null)
            {
                selectedItem = items.Where(x => x.Value.Equals(TempData["status"].ToString())).FirstOrDefault();
                selectedItem.Selected = true;
                ViewBag.StatusList = new SelectList(items, "Value", "Text", selectedItem.Value);
            }
            else
            {
                ViewBag.StatusList = new SelectList(items, "Value", "Text");
            }

            //Sedes
            //Origem           
            var sedes = db.Sedes.ToList();
            items = new List<SelectListItem>();
            selectedItem = new SelectListItem();

            sedes.ForEach(x =>
            {
                SelectListItem item = new SelectListItem();
                item.Text = x.Sigla;
                item.Value = x.Id.ToString();
                items.Add(item);
            });

            first = new SelectListItem();
            first.Text = "Sede Origem";
            first.Value = "0";
            items.Insert(0, first);

            if (TempData["sedeOrigem"] != null)
            {
                selectedItem = items.Where(x => x.Value.Equals(TempData["sedeOrigem"].ToString())).FirstOrDefault();
                selectedItem.Selected = true;
                ViewBag.SedesOrigem = new SelectList(items, "Value", "Text", selectedItem.Value);
            }
            else
            {
                ViewBag.SedesOrigem = new SelectList(items, "Value", "Text");
            }

            //destino

            items = new List<SelectListItem>();
            selectedItem = new SelectListItem();

            sedes.ForEach(x =>
            {
                SelectListItem item = new SelectListItem();
                item.Text = x.Sigla;
                item.Value = x.Id.ToString();
                items.Add(item);
            });

            first = new SelectListItem();
            first.Text = "Sede Destino";
            first.Value = "0";
            items.Insert(0, first);

            if (TempData["sedeDestino"] != null)
            {
                selectedItem = items.Where(x => x.Value.Equals(TempData["sedeDestino"].ToString())).FirstOrDefault();
                selectedItem.Selected = true;
                ViewBag.SedesDestino = new SelectList(items, "Value", "Text", selectedItem.Value);
            }
            else
            {
                ViewBag.SedesDestino = new SelectList(items, "Value", "Text");
            }

            //Motivos

            var motivos = db.Motivos.ToList();
            items = new List<SelectListItem>();
            selectedItem = new SelectListItem();

            motivos.ForEach(x =>
            {
                SelectListItem item = new SelectListItem();
                item.Text = x.Nome;
                item.Value = x.Id.ToString();
                items.Add(item);
            });

            first = new SelectListItem();
            first.Text = "Motivo";
            first.Value = "0";
            items.Insert(0, first);

            if (TempData["motivo"] != null)
            {
                selectedItem = items.Where(x => x.Value.Equals(TempData["motivo"].ToString())).FirstOrDefault();
                selectedItem.Selected = true;
                ViewBag.MotivosList = new SelectList(items, "Value", "Text", selectedItem.Value);
            }
            else
            {
                ViewBag.MotivosList = new SelectList(items, "Value", "Text");
            }

            //Tipos Transporte

            var tiposTransporte = db.TipoTransportes.ToList();
            items = new List<SelectListItem>();
            selectedItem = new SelectListItem();

            tiposTransporte.ForEach(x =>
            {
                SelectListItem item = new SelectListItem();
                item.Text = x.Nome;
                item.Value = x.Id.ToString();
                items.Add(item);
            });

            first = new SelectListItem();
            first.Text = "Transporte";
            first.Value = "0";
            items.Insert(0, first);

            if (TempData["tipoTransporte"] != null)
            {
                selectedItem = items.Where(x => x.Value.Equals(TempData["tipoTransporte"].ToString())).FirstOrDefault();
                selectedItem.Selected = true;
                ViewBag.TiposTransporteList = new SelectList(items, "Value", "Text", selectedItem.Value);
            }
            else
            {
                ViewBag.TiposTransporteList = new SelectList(items, "Value", "Text");
            }
        }

        private List<Envio> ApplyFilters(List<Envio> currentData)
        {
            string ordem = Request.QueryString["ordemManutencao"];
            string descricao = Request.QueryString["descricao"];
            string numeroSerie = Request.QueryString["numeroSerie"];
            string tiposTransporte = Request.QueryString["tiposTransportes"];
            string notaFiscal = Request.QueryString["notaFiscal"];
            string motivo = Request.QueryString["motivos"];
            string numeroImpressao = Request.QueryString["numeroImpressaoNota"];
            string notaSap = Request.QueryString["notaSap"];
            string idSolicitacao = Request.QueryString["idSolicitacao"]; //material system id 
            string sedeOrigem = Request.QueryString["sedeOrigem"];
            string sedeDestino = Request.QueryString["sedeDestino"];
            string status = Request.QueryString["status"];
            string dataCriacao = Request.QueryString["dataCriacao"];
            string dataEnvio = Request.QueryString["dataEnvio"];
            string dataRecebimento = Request.QueryString["dataRecebimento"];
            string matriculaDespachante = Request.QueryString["matriculaDespachante"];
            string matriculaRecebedor = Request.QueryString["matriculaRecebedor"];
            string dataEnvioInicio = Request.QueryString["dataEnvioInicio"];
            string dataEnvioFim = Request.QueryString["dataEnvioFim"];

            var sends = currentData == null ? db.Envios.Include(x => x.SedeDestino).Include(x => x.SedeOrigem).
                                              Include(x => x.TipoTransporte).Include(x => x.Motivo).ToList() :
                                              currentData;

            if (!string.IsNullOrEmpty(ordem))
            {
                sends = sends.Where(x => x.OrdemManutencao.ToUpper().Contains(ordem.ToUpper())).ToList();
            }
            if (!string.IsNullOrEmpty(descricao))
            {
                sends = sends.Where(x => x.Descricao.ToUpper().Contains(descricao.ToUpper())).ToList();
            }
            if (!string.IsNullOrEmpty(numeroSerie))
            {
                sends = sends.Where(x => x.NumeroSerie.ToUpper().Contains(numeroSerie.ToUpper())).ToList();
            }
            if (!string.IsNullOrEmpty(tiposTransporte))
            {
                int lvtipoTransporte = 0;

                if (Int32.TryParse(tiposTransporte, out lvtipoTransporte) && lvtipoTransporte != 0)
                {
                    sends = sends.Where(x => x.TipoTransporteId == lvtipoTransporte).ToList();
                    TempData["tipoTransporte"] = lvtipoTransporte;
                }
            }
            if (!string.IsNullOrEmpty(notaFiscal))
            {
                sends = sends.Where(x => x.NotaFiscal.ToUpper().Contains(notaFiscal.ToUpper())).ToList();
            }

            if (!string.IsNullOrEmpty(motivo))
            {
                int lvMotivo = 0;

                if (Int32.TryParse(motivo, out lvMotivo) && lvMotivo != 0)
                {
                    sends = sends.Where(x => x.MotivoId == lvMotivo).ToList();
                    TempData["motivo"] = lvMotivo;
                }
            }

            if (!string.IsNullOrEmpty(numeroImpressao))
            {
                sends = sends.Where(x => x.NumeroImpressaoNota.ToUpper().Contains(numeroImpressao.ToUpper())).ToList();
            }
            if (!string.IsNullOrEmpty(notaSap))
            {
                sends = sends.Where(x => x.NotaTransferenciaSap.ToUpper().Contains(notaSap.ToUpper())).ToList();
            }
            if (!string.IsNullOrEmpty(idSolicitacao))
            {
                int lvIdSolicitacao = 0;

                if (Int32.TryParse(idSolicitacao, out lvIdSolicitacao) && lvIdSolicitacao != 0)
                {
                    sends = sends.Where(x => x.IdSolicitacao == lvIdSolicitacao).ToList();
                }
            }
            if (!string.IsNullOrEmpty(sedeOrigem))
            {
                int lvIdSede = 0;

                if (Int32.TryParse(sedeOrigem, out lvIdSede) && lvIdSede != 0)
                {
                    sends = sends.Where(x => x.SedeOrigemId == lvIdSede).ToList();
                    TempData["sedeOrigem"] = lvIdSede;
                }
            }
            if (!string.IsNullOrEmpty(sedeDestino))
            {
                int lvIdSede = 0;

                if (Int32.TryParse(sedeDestino, out lvIdSede) && lvIdSede != 0)
                {
                    sends = sends.Where(x => x.SedeDestinoId == lvIdSede).ToList();
                    TempData["sedeDestino"] = lvIdSede;
                }
            }
            //this attibuttes are linked with sends by their transactions
            if (!string.IsNullOrEmpty(dataEnvioInicio))
            {
                DateTime lvDate = DateTime.MinValue;

                if (DateTime.TryParse(dataEnvioInicio, out lvDate))
                {
                    sends.ForEach(x =>
                    {
                        var sendTransaction = db.Transacoes.Where(y => y.EnvioId == x.Id && y.StatusId == 2).FirstOrDefault();

                        if (sendTransaction != null)
                        {
                            x.DataEnvio = (sendTransaction.Data.Date >= lvDate.Date) ?
                                            sendTransaction.Data.ToString("dd/MM/yyyy") : "-";
                        }
                        else
                        {
                            x.DataEnvio = "-";
                        }

                    });

                    sends.RemoveAll(x => x.DataEnvio.Equals("-"));
                    TempData["dataEnvioInicio"] = lvDate;
                }
                else
                {
                    TempData["message"] = "Data inicio não é válida!";
                    TempData["class"] = "alert-danger";
                }

            }
            if (!string.IsNullOrEmpty(dataEnvioFim))
            {
                DateTime lvDate = DateTime.MinValue;

                if (DateTime.TryParse(dataEnvioFim, out lvDate))
                {
                    lvDate = lvDate.AddHours(23).AddMinutes(59).AddSeconds(59);
                    sends.ForEach(x =>
                    {
                        var sendTransaction = db.Transacoes.Where(y => y.EnvioId == x.Id && y.StatusId == 2).FirstOrDefault();

                        if (sendTransaction != null)
                        {
                            x.DataEnvio = (sendTransaction.Data.Date <= lvDate.Date) ?
                                            sendTransaction.Data.ToString("dd/MM/yyyy") : "-";
                        }
                        else
                        {
                            x.DataEnvio = "-";
                        }

                    });

                    sends.RemoveAll(x => x.DataEnvio.Equals("-"));
                    TempData["dataEnvioFim"] = lvDate;
                }
                else
                {
                    TempData["message"] = "Data fim não é válida!";
                    TempData["class"] = "alert-danger";
                }
            }
            if (!string.IsNullOrEmpty(dataCriacao))
            {
                DateTime lvDate = DateTime.MinValue;

                if (DateTime.TryParse(dataCriacao, out lvDate))
                {
                    sends.ForEach(x =>
                    {
                        var createTransaction = db.Transacoes.Where(y => y.EnvioId == x.Id && y.StatusId == 1).FirstOrDefault();

                        if (createTransaction != null)
                        {
                            x.DataCriacao = (createTransaction.Data.Date == lvDate.Date) ?
                                            createTransaction.Data.ToString("dd/MM/yyyy") : "-";
                        }
                        else
                        {
                            x.DataCriacao = "-";
                        }

                    });

                    sends.RemoveAll(x => x.DataCriacao.Equals("-"));
                    TempData["dataCriacao"] = lvDate;
                }
            }
            if (!string.IsNullOrEmpty(dataEnvio))
            {
                DateTime lvDate = DateTime.MinValue;

                if (DateTime.TryParse(dataEnvio, out lvDate))
                {
                    sends.ForEach(x =>
                    {
                        var sendTransaction = db.Transacoes.Where(y => y.EnvioId == x.Id && y.StatusId == 2).FirstOrDefault();

                        if (sendTransaction != null)
                        {
                            x.DataEnvio = (sendTransaction.Data.Date == lvDate.Date) ?
                                            sendTransaction.Data.ToString("dd/MM/yyyy") : "-";
                        }
                        else
                        {
                            x.DataEnvio = "-";
                        }

                    });

                    sends.RemoveAll(x => x.DataEnvio.Equals("-"));
                    TempData["dataEnvio"] = lvDate;
                }
            }
            if (!string.IsNullOrEmpty(dataRecebimento))
            {
                DateTime lvDate = DateTime.MinValue;

                if (DateTime.TryParse(dataRecebimento, out lvDate))
                {
                    sends.ForEach(x =>
                    {
                        var receiveTransaction = db.Transacoes.Where(y => y.EnvioId == x.Id && y.StatusId == 3).FirstOrDefault();

                        if (receiveTransaction != null)
                        {
                            x.DataRecebimento = (receiveTransaction.Data.Date == lvDate.Date) ?
                                            receiveTransaction.Data.ToString("dd/MM/yyyy") : "-";
                        }
                        else
                        {
                            x.DataRecebimento = "-";
                        }

                    });

                    sends.RemoveAll(x => x.DataRecebimento.Equals("-"));
                    TempData["dataRecebimento"] = lvDate;
                }
            }
            if (!string.IsNullOrEmpty(matriculaDespachante))
            {
                sends.ForEach(x =>
                {
                    var sendTransaction = db.Transacoes.Where(y => y.EnvioId == x.Id && y.StatusId == 2).FirstOrDefault();

                    if (sendTransaction != null)
                    {
                        x.MatriculaDespachante = (!string.IsNullOrEmpty(sendTransaction.MatriculaModificador) &&
                                                 sendTransaction.MatriculaModificador.ToUpper().Contains(matriculaDespachante.ToUpper())) ?
                                                 sendTransaction.MatriculaModificador : "-";
                    }
                    else
                    {
                        x.MatriculaDespachante = "-";
                    }

                });

                sends.RemoveAll(x => x.MatriculaDespachante.Equals("-"));
                TempData["matriculaDespachante"] = matriculaDespachante;
            }
            if (!string.IsNullOrEmpty(matriculaRecebedor))
            {
                sends.ForEach(x =>
                {
                    var receivedTransaction = db.Transacoes.Where(y => y.EnvioId == x.Id && y.StatusId == 3).FirstOrDefault();

                    if (receivedTransaction != null)
                    {
                        x.MatriculaRecebedor = (!string.IsNullOrEmpty(receivedTransaction.MatriculaModificador) &&
                                                 receivedTransaction.MatriculaModificador.ToUpper().Contains(matriculaDespachante.ToUpper())) ?
                                                 receivedTransaction.MatriculaModificador : "-";
                    }
                    else
                    {
                        x.MatriculaRecebedor = "-";
                    }

                });

                sends.RemoveAll(x => x.MatriculaDespachante.Equals("-"));
                TempData["matriculaRecebedor"] = matriculaRecebedor;
            }
            if (!string.IsNullOrEmpty(status))
            {
                int lvIdStatus = 0;

                if (Int32.TryParse(status, out lvIdStatus) && lvIdStatus != 0)
                {
                    sends.ForEach(x =>
                    {
                        var lastTransaction = db.Transacoes.Where(y => y.EnvioId == x.Id).Select(y => y).
                                              OrderByDescending(y => y.Id).FirstOrDefault();

                        if (lastTransaction != null)
                        {
                            var lastStatus = db.Status.Where(z => z.Id == lastTransaction.StatusId).FirstOrDefault();
                            x.CurrentStatus = (lastStatus != null && lastStatus.Id == lvIdStatus) ? lastStatus.Nome : "-";
                        }
                        else
                        {
                            x.CurrentStatus = "-";
                        }

                    });

                    sends.RemoveAll(x => x.CurrentStatus.Equals("-"));
                    TempData["status"] = lvIdStatus;
                }
            }

            return sends;
        }

        private void TreatingSendsData(DataTable table)
        {
            try
            {
                int transportCell = 5;
                int reasonCell = 7;
                int destinySedeCell = 12;
                int originSedeCell = 11;

                foreach (DataRow line in table.Rows)
                {
                    var transportId = Int32.Parse(line[transportCell].ToString());
                    var reasonId = Int32.Parse(line[reasonCell].ToString());
                    var sedeDestinyId = Int32.Parse(line[destinySedeCell].ToString());
                    var sedeOriginId = Int32.Parse(line[originSedeCell].ToString());

                    line[transportCell] = db.TipoTransportes.Where(x => x.Id == transportId).FirstOrDefault().Nome;
                    line[reasonCell] = db.Motivos.Where(x => x.Id == reasonId).FirstOrDefault().Nome;
                    line[destinySedeCell] = db.Sedes.Where(x => x.Id == sedeDestinyId).FirstOrDefault().Nome;
                    line[originSedeCell] = db.Sedes.Where(x => x.Id == sedeOriginId).FirstOrDefault().Nome;
                }
            }
            catch (Exception e)
            {
                DebugLog.Logar(e.Message);
                DebugLog.Logar(e.StackTrace);
            }
        }

        /*Fill Dates and status fields*/
        private void GetCurrentStatus(List<Envio> sends)
        {
            sends.ForEach(x =>
            {
                var allMyTransactions = db.Transacoes.Where(y => y.EnvioId == x.Id);
                var createTransaction = allMyTransactions.Where(y => y.StatusId == 1).FirstOrDefault();
                var sendTransaction = allMyTransactions.Where(y => y.StatusId == 2).FirstOrDefault();
                var receivedTransaction = allMyTransactions.Where(y => y.StatusId == 3).FirstOrDefault();
                var lastTransaction = allMyTransactions.Select(y => y).OrderByDescending(y => y.Id).FirstOrDefault();

                if (createTransaction != null)
                {
                    x.DataCriacao = createTransaction.Data.ToString("dd/MM/yyyy");
                }
                else
                {
                    x.DataCriacao = "";
                }

                if (sendTransaction != null)
                {
                    x.DataEnvio = sendTransaction.Data.ToString("dd/MM/yyyy");
                    x.MatriculaDespachante = sendTransaction.MatriculaModificador;
                }
                else
                {
                    x.DataEnvio = "";
                    x.MatriculaDespachante = "";
                }

                if (receivedTransaction != null)
                {
                    x.DataRecebimento = receivedTransaction.Data.ToString("dd/MM/yyyy");
                    x.MatriculaRecebedor = receivedTransaction.MatriculaModificador;
                }
                else
                {
                    x.DataRecebimento = "";
                }

                if (lastTransaction != null)
                {
                    var lastStatus = db.Status.Where(z => z.Id == lastTransaction.StatusId).FirstOrDefault();
                    x.CurrentStatus = (lastStatus != null) ? lastStatus.Nome : "-";
                }
                else
                {
                    x.CurrentStatus = "-";
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
                dbUser.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
