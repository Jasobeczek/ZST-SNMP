using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using RestWebService.SNMPAccessLayer;

namespace RestWebService
{
    public class WebService : IHttpHandler
    {
        private AccessLayer AccessLayer;

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
        /// </summary>
        public bool IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                String url = Convert.ToString(context.Request.Url);
                this.AccessLayer = new AccessLayer();

                switch (context.Request.HttpMethod)
                {
                    case "GET":
                        Read(context);
                        break;
                    case "POST":
                        Create(context);
                        break;
                    case "PUT":
                        Update(context);
                        break;
                    case "DELETE":
                        Delete(context);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                context.Response.Write(e.Message);
            }
        }

        private static void WriteResponse(string strMessage)
        {
            HttpContext.Current.Response.Write(strMessage);
        }

        /// <summary>
        /// Reads the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void Read(HttpContext context)
        {
            if (context.Request.QueryString["element"] != null)
            {
                String response = this.AccessLayer.Get(context.Request["element"].ToString());
                context.Response.ContentType = "text/json";
                WriteResponse(response);
            }
            else if (context.Request.QueryString["table"] != null)
            {
                String response = this.AccessLayer.GetTable(context.Request["table"].ToString());
                context.Response.ContentType = "text/json";
                WriteResponse(response);
            }
            else
            {
                WriteResponse("error");
            }
        }

        private void Delete(HttpContext context)
        {
            WriteResponse("error");
        }

        /// <summary>
        /// Updates the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        private void Update(HttpContext context)
        {
            String response = this.AccessLayer.Set(context.Request.BinaryRead(context.Request.ContentLength));
            context.Response.ContentType = "text";
            WriteResponse(response);
        }

        private void Create(HttpContext context)
        {
            WriteResponse("error");
        }
    }
}
