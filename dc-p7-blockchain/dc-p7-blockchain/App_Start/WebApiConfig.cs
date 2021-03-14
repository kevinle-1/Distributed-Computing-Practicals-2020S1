// Filename: WebApiConfig.cs
// Project:  DC Assignment (COMP3008)
// Purpose:  Configuration for Web API 
// Author:   Kevin Le (19472960)
//
// Date:     24/05/2020

using System.Net.Http.Headers;
using System.Web.Http;

namespace dc_p7_blockchain.App_Start
{
    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}