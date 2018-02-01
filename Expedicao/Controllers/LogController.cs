using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Expedicao.Extensions;
using System.Configuration;
using Expedicao.DAL;
using Expedicao.Models;

namespace Expedicao.Controllers
{
    public class LogController : Controller
    {
        // GET: Log
        public ActionResult Index()
        {
            String referrer = "";
            String user = "";
            String Ip = Request.UserHostAddress.ToString();

            try
            {
                referrer = "192.168.13.125 172.20.15.22";
                //referrer = "localhost";
                user = Request.QueryString["userId"];
            }
            catch (Exception ex)
            {
                referrer = null;
                user = null;
                DebugLog.Logar(ex.Message);
            }

            if (referrer != null && referrer.Contains(Request.ServerVariables["SERVER_NAME"].ToString()))
            {
                UserContext dbUser = new UserContext();
                var appId = ConfigurationManager.AppSettings["APP_ID"].ToString();
                int lvUserId = Int32.Parse(user.ToString());
                int lvAppId = Int32.Parse(appId);
                var userObj = dbUser.Users.Where(x => x.Id == lvUserId).FirstOrDefault();

                if (user != null)
                {
                    GrantedUser g = dbUser.GrantedUsers.Where(x => x.UserId == (userObj.Id) &&
                                                              x.AppId == (lvAppId)).FirstOrDefault();

                    if (g != null)
                    {
                        Session["userId"] = user.ToString();

                        if (g.GrantId == 1)
                        {
                            dbUser.Dispose();
                            return RedirectToAction("Received", "Envios");
                        }
                        else
                        {
                            dbUser.Dispose();
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        this.HttpContext.Response.Redirect("http://" + Request.ServerVariables["SERVER_NAME"] + "/gasag/");
                    }
                }
                else
                {
                    this.HttpContext.Response.Redirect("http://" + Request.ServerVariables["SERVER_NAME"] + "/gasag/");
                }

                dbUser.Dispose();

                return RedirectToAction("Index", "Home");
            }
            else
            {
                DebugLog.Logar(referrer);
                DebugLog.Logar(Request.ServerVariables["SERVER_NAME"].ToString());
                this.HttpContext.Response.Redirect("http://" + Request.ServerVariables["SERVER_NAME"] + "/gasag/");
            }

            Response.End();
            return View();
        }
    }
}