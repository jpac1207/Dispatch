using Expedicao.DAL;
using Expedicao.Extensions;
using Expedicao.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Expedicao.Filters
{
    public class AdminFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var myController = actionContext.Controller;
            UserContext dbUser = null;

            try
            {
                //Faço a verificação apenas em produção
                if (ConfigurationManager.AppSettings["SECURITY_USER"].ToString().Equals("true"))
                {
                    var userId = actionContext.HttpContext.Session["userId"];
                    var appId = ConfigurationManager.AppSettings["APP_ID"].ToString();

                    //Se o usuário não tiver logado ou a sessão tiver expirado                
                    if (userId == null)
                    {
                        myController.ControllerContext.HttpContext.Response.Headers.Clear();
                        myController.ControllerContext.HttpContext.Response.Redirect("http://172.20.15.19/gasag", true);
                    }
                    else
                    {
                        int lvUserId = Int32.Parse(userId.ToString());
                        int lvAppId = Int32.Parse(appId);
                        dbUser = new UserContext();

                        GrantedUser grantedUser = dbUser.GrantedUsers.Where(x => x.UserId == lvUserId &&
                                                                            x.AppId == lvAppId && x.GrantId == 2).FirstOrDefault();

                        dbUser.Dispose();

                        if (grantedUser == null)
                        {
                            myController.ControllerContext.HttpContext.Response.Redirect("172.20.15.22/despacho/Envios/Received", true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DebugLog.Logar(e.Message);
                DebugLog.Logar(e.StackTrace);
                DebugLog.Logar(Utility.Details(e));

                if (dbUser != null)
                    dbUser.Dispose();

                myController.ControllerContext.HttpContext.Response.Redirect("http://172.20.15.19/gasag", true);
            }
            base.OnActionExecuting(actionContext);
        }
    }
}