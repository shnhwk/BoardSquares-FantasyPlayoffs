using System.Security.Claims;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using SerilogWeb.Classic;

namespace BoardSquares
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
            //HttpContext.Current.RewritePath("/Account/Login");

            SerilogWebClassic.Configure(cfg => cfg
                .LogAtLevel(LogEventLevel.Debug)
            );
            var colOpts = new ColumnOptions();
            colOpts.Store.Add(StandardColumn.LogEvent);
            colOpts.Store.Remove(StandardColumn.Properties);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer(System.Configuration.ConfigurationManager.ConnectionStrings["BoardSquaresDB"].ConnectionString,
                    restrictedToMinimumLevel: LogEventLevel.Information, columnOptions: colOpts,
                    sinkOptions: new MSSqlServerSinkOptions { TableName = "FP_Logs", SchemaName = "dbo" })
                .Enrich.WithUserName()
                .CreateLogger();
        }

        protected void Application_End()
        {
            Log.CloseAndFlush();
        }

    }
}
