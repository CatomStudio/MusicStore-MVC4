using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Collections;
using System.Text;
using MusicStore.Domain.Utils;

namespace MusicStore.WebUI.Filters
{
    public class ExceptionFilter : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("|Url:");
                sb.Append(filterContext.HttpContext.Request.Url.AbsoluteUri);
                sb.Append("\r\n");
                sb.Append(filterContext.Exception.Message.Replace("<", string.Empty).Replace(">", string.Empty));
                sb.Append("\r\n");
                sb.Append(filterContext.Exception.Source);
                sb.Append("\r\n");
                sb.Append(filterContext.Exception.StackTrace);
                sb.Append("\r\n");
                Exception ex = filterContext.Exception.InnerException;
                while (ex != null)
                {
                    sb.Append("\r\n-----------------------");
                    sb.Append(filterContext.Exception.Message.Replace("<", string.Empty).Replace(">", string.Empty));
                    sb.Append("\r\n");
                    sb.Append(filterContext.Exception.Source);
                    sb.Append("\r\n");
                    sb.Append(filterContext.Exception.StackTrace);
                    ex = ex.InnerException;
                }

                StringBuilder sbHeaders = new StringBuilder();
                for (int i = 0; i < filterContext.HttpContext.Request.Headers.Keys.Count; i++)
                {
                    var keyName = filterContext.HttpContext.Request.Headers.AllKeys[i];
                    sbHeaders.Append(keyName + ":" + filterContext.HttpContext.Request.Headers.GetValues(keyName)[0] + " ");
                }

                LogHelper.WriteLog(sb.ToString());

                filterContext.ExceptionHandled = true;

                //普通请求，返回自定义错误页面
                var urlHelper = new UrlHelper(filterContext.RequestContext);
                var url = urlHelper.Action("ErrorRedirect", "SchoolPal");
                filterContext.Result = new RedirectResult(url);
            }
        }

    }
}