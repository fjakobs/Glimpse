﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Web;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using Microsoft.Web.Infrastructure.DynamicValidationHelper;
using Newtonsoft.Json;

namespace Glimpse.Core.Plugin
{
    [GlimpsePlugin]
    internal class Request : IGlimpsePlugin, IProvideGlimpseHelp
    {
        public string Name
        {
            get { return "Request"; }
        }

        public object GetData(HttpContextBase context)
        {
            var request = context.Request;

            var cookies = new List<object[]>
                              {
                                  new[]{"Name", "Path", "Secure", "Value"}
                              };

            var requestCookies = request.Cookies;

            foreach (var key in request.Cookies.AllKeys)
            {
                var cookie = requestCookies[key];

                if (cookie != null)
                    cookies.Add(new[] { cookie.Name, cookie.Path, cookie.Secure.ToString(), context.Server.UrlDecode(cookie.Value) });
            }

            //To avoid validation exceptions, input values are read like this
            Func<NameValueCollection> formData;
            Func<NameValueCollection> queryStringData;
            ValidationUtility.GetUnvalidatedCollections(HttpContext.Current, out formData, out queryStringData);//HACK: ugly hack to use HttpContext.Current because GetUnvalidatedCollections does not take in an HttpContextBase

            var form = formData().Flatten();
            var querystring = queryStringData().Flatten();

            //make sure there is request data
            if (form == null && querystring == null && cookies.Count <= 1) return null;

            //read input stream
            var inputStream = context.Request.InputStream;
            inputStream.Position = 0;
            var inputString = new StreamReader(inputStream).ReadToEnd();
            object inputStreamResult = string.IsNullOrEmpty(inputString) ? null : inputString;

            //If input was json, deserialize to object so it can be rendered later.
            if (context.Request.ContentType.StartsWith(@"application/json", StringComparison.OrdinalIgnoreCase))
                inputStreamResult = JsonConvert.DeserializeObject(inputString);

            return new
                       {
                           Cookies = cookies,
                           Form = form,
                           QueryString = querystring,
                           InputStream = inputStreamResult,
                           Thread.CurrentThread.CurrentUICulture,
                           request.ApplicationPath,
                           request.AppRelativeCurrentExecutionFilePath,
                           request.CurrentExecutionFilePath,
                           request.FilePath,
                           request.Path,
                           request.PathInfo,
                           request.PhysicalApplicationPath,
                           request.PhysicalPath,
                           request.RawUrl,
                           Url = request.Url.ToString(),
                           UrlReferrer = request.UrlReferrer == null ? null : request.UrlReferrer.ToString(),
                           request.UserAgent,
                           request.UserHostAddress,
                           request.UserHostName
                       };
        }

        public void SetupInit()
        {
        }

        public string HelpUrl
        {
            get { return "http://getGlimpse.com/Help/Plugin/Request"; }
        }
    }
}