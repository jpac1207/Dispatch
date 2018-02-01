using Expedicao.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Expedicao.DAL;
using Expedicao.Models;

namespace Expedicao.Filters
{
    public class AutenticationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var myController = actionContext.Controller;

            try
            {
                //Faço a verificação apenas em produção
                if (ConfigurationManager.AppSettings["SECURITY_USER"].ToString().Equals("true"))
                {
                    var userId = actionContext.HttpContext.Session["userId"];

                    //Se o usuário não tiver logado ou a sessão tiver expirado                
                    if (userId == null)
                    {
                        myController.ControllerContext.HttpContext.Response.Redirect("http://172.20.15.19/gasag", true);
                    }
                }
            }
            catch (Exception e)
            {
                DebugLog.Logar(e.Message);
                DebugLog.Logar(e.StackTrace);
                DebugLog.Logar(Utility.Details(e));
                myController.ControllerContext.HttpContext.Response.Redirect("http://172.20.15.19/gasag", true);
            }
            base.OnActionExecuting(actionContext);
        }
    }
}
