using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Web.Http;
using VspWS.Data;
using VspWS.DataAccess;
using VspWS.MessageProcessorService.App_Start;

namespace VspWS.MessageProcessorService
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configure(WebApiConfig.Configure);
            Database.SetInitializer(new DropCreateDatabaseAlways<AlSys>());
            Database.SetInitializer(new DropCreateDatabaseAlways<Falcon>());
            using(var dal = new AlSysDAL())
            {
                dal.Ping();
            }
            using (var dal = new FalconDAL())
            {
                dal.Ping();
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            if (ex != null)
            {
                Trace.TraceError(ex.ToString());
            }
        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}