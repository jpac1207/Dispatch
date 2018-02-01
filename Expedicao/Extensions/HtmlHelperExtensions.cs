using Newtonsoft.Json;
using Expedicao.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Expedicao.Extensions
{
    public static class HtmlHelperExtensions
    {

        public static HtmlString HtmlConvertToJson(this HtmlHelper htmlHelper, object model)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };
            return new HtmlString(JsonConvert.SerializeObject(model, settings));
        }

        public static MvcHtmlString BuildSortableLink(this HtmlHelper htmlHelper, string fieldName, string actionName,
                                                        string sortField, QueryOptions queryOptions)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var isCurrentSortField = queryOptions.SortField == sortField;

            return new MvcHtmlString(string.Format("<a href=\"{0}\">{1}</a>", urlHelper.Action(actionName, new
            {
                SortField = sortField,
                SortOrder = (isCurrentSortField && queryOptions.SortOrder == SortOrder.ASC) ? SortOrder.DESC : SortOrder.ASC
            }
            ), fieldName));
        }

        private static string BuildSortIcon(bool isCurrentSortField, QueryOptions queryOptions)
        {
            string sortIcon = "sort";

            if (isCurrentSortField)
            {
                sortIcon += "-by-alphabet";
                if (queryOptions.SortOrder == SortOrder.DESC)
                    sortIcon += "-alt";
            }
            return string.Format("<span class=\"{0} {1}{2}\"></span>", "glyphicon", "glyphicon-", sortIcon);
        }

        public static MvcHtmlString BuildNextPreviousLinks(this HtmlHelper htmlHelper, QueryOptions queryOptions, string actionName)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);

            return new MvcHtmlString(string.Format(
            "<nav>" +
            " <ul class=\"pager\">" +
                " <li class=\"previous {0}\" disabled=\"{0}\">{1}</li>" +
                " <li class=\"next {2}\" disabled=\"{0}\">{3}</li>" +
            " </ul>" +
            "</nav>",
            IsPreviousDisabled(queryOptions),
            BuildPreviousLink(urlHelper, queryOptions, actionName),
            IsNextDisabled(queryOptions),
            BuildNextLink(urlHelper, queryOptions, actionName)
            ));
        }

        private static string IsPreviousDisabled(QueryOptions queryOptions)
        {
            return (queryOptions.CurrentPage == 1)
            ? "disabled" : string.Empty;
        }

        private static string IsNextDisabled(QueryOptions queryOptions)
        {
            return (queryOptions.CurrentPage == queryOptions.TotalPages)
            ? "disabled" : string.Empty;
        }

        private static string BuildPreviousLink(UrlHelper urlHelper, QueryOptions queryOptions, string actionName)
        {
            return string.Format(
            "<a href=\"{0}\"><span aria-hidden=\"true\">&larr;</span> Anterior</a>",
            urlHelper.Action(actionName, new
            {
                SortOrder = queryOptions.SortOrder,
                SortField = queryOptions.SortField,
                CurrentPage = queryOptions.CurrentPage - 1,
                PageSize = queryOptions.PageSize
            }));
        }

        private static string BuildNextLink(UrlHelper urlHelper, QueryOptions queryOptions, string actionName)
        {
            return string.Format(
            "<a href=\"{0}\">Próxima <span aria-hidden=\"true\">&rarr;</span></a>",
            urlHelper.Action(actionName, new
            {
                SortOrder = queryOptions.SortOrder,
                SortField = queryOptions.SortField,
                CurrentPage = queryOptions.CurrentPage + 1,
                PageSize = queryOptions.PageSize
            }));
        }

        public static MvcHtmlString GetFormatedDate(this HtmlHelper htmlHelper, DateTime data)
        {
            return (data != DateTime.MinValue) ? new MvcHtmlString(data.ToString("dd/MM/yyyy")) : new MvcHtmlString("-");
        }

        public static MvcHtmlString GetFormatedDate(this HtmlHelper htmlHelper, DateTime? data)
        {
            return (data != null) ? new MvcHtmlString(data.Value.ToString("dd/MM/yyyy")) : new MvcHtmlString("-");
        }

        public static MvcHtmlString GetBackMessage(this HtmlHelper htmlHelper)
        {
            return new MvcHtmlString("Voltar Para a Lista");
        }

        public static MvcHtmlString GetCreateMessage(this HtmlHelper htmlHelper)
        {
            return new MvcHtmlString("Criar Novo");
        }

        public static MvcHtmlString GetDeleteMessage(this HtmlHelper htmlHelper)
        {
            return new MvcHtmlString("Deletar");
        }

        public static MvcHtmlString GetDeleteConfirmationMessage(this HtmlHelper htmlHelper)
        {
            return new MvcHtmlString("Você realmente deseja deletar isso?");
        }

        public static MvcHtmlString GetCurrentParameters(this HtmlHelper htmlHelper, HttpRequestBase Request)
        {
            string parameters = "";

            for (int i = 0; i < Request.QueryString.Keys.Count; i++)
            {

                parameters += Request.QueryString.Keys[i];
                parameters += "=" + Request.QueryString.GetValues(Request.QueryString.Keys[i])[0];

                if (i < Request.QueryString.Keys.Count - 1)
                {
                    parameters += "&";
                }
            }

            return new MvcHtmlString(parameters);
        }

    }
}