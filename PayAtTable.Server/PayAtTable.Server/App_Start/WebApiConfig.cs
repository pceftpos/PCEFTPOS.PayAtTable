using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Net.Http.Formatting;
using TinyIoC;
using PayAtTable.Server.Data;
using PayAtTable.Server.Models;
using PayAtTable.API.Helpers;
using System.Reflection;
using PayAtTable.Server.Helpers;

namespace PayAtTable.Server
{
    public static class WebApiConfig
    {
        static readonly Common.Logging.ILog log = Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Type LoadInstanceFromAssembly(Type interfaceType, Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsInterface == false && type.IsAbstract == false)
                {
                    if (type.GetInterface(interfaceType.FullName) != null)
                    {
                        return type;
                    }
                }
            }
            return null;
        }


        public static void Register(HttpConfiguration config)
        {
            try
            {
                // Find assembly from path defined in web.config
                var path = PayAtTable.Server.Properties.Settings.Default.DataRepositoryAssembly;
                var mappedPath = System.Web.Hosting.HostingEnvironment.MapPath(path);
                Assembly assembly = Assembly.LoadFrom(mappedPath);

                // Register data repository implementations with the IoC container
                var container = TinyIoCContainer.Current;
                container.Register(typeof(IOrdersRepository), LoadInstanceFromAssembly(typeof(IOrdersRepository), assembly)).AsPerRequestSingleton();
                container.Register(typeof(ITablesRepository), LoadInstanceFromAssembly(typeof(ITablesRepository), assembly)).AsPerRequestSingleton();
                container.Register(typeof(IEFTPOSRepository), LoadInstanceFromAssembly(typeof(IEFTPOSRepository), assembly)).AsPerRequestSingleton();
                container.Register(typeof(ITendersRepository), LoadInstanceFromAssembly(typeof(ITendersRepository), assembly)).AsPerRequestSingleton();
                container.Register(typeof(ISettingsRepository), LoadInstanceFromAssembly(typeof(ISettingsRepository), assembly)).AsPerRequestSingleton();

                // Uncomment the following code to load a local data repository in this project rather than an external DLL
                //container.Register<IOrdersRepository, DemoRepository>().AsPerRequestSingleton();
                //container.Register<ITablesRepository, DemoRepository>().AsPerRequestSingleton();
                //container.Register<IEFTPOSRepository, DemoRepository>().AsPerRequestSingleton();
                //container.Register<ITendersRepository, DemoRepository>().AsPerRequestSingleton();
                //container.Register<ISettingsRepository, DemoRepository>().AsPerRequestSingleton();            
                container.Register<IClientValidator, ClientValidator>().AsPerRequestSingleton();            

                // Set Web API dependancy resolver
                System.Web.Http.GlobalConfiguration.Configuration.DependencyResolver = new TinyIocWebApiDependencyResolver(container);

                // Comment out the following line to support XML
                // config.Formatters.Remove(config.Formatters.XmlFormatter);

                // Configure JSON formatter
                GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore; 

                // Web API2 routes
                config.MapHttpAttributeRoutes();
#if SSL
                config.Filters.Add(new RequireHttpsAttribute());
#endif

                // Uncomment the following code to support WEB API v1 routing
                //config.Routes.MapHttpRoute(
                //    name: "DefaultApi",
                //    routeTemplate: "api/{controller}/{id}",
                //    defaults: new { id = RouteParameter.Optional }
                //);

                // Uncomment the following to add a key validator to the message pipeline. 
                // This will check that the "apikey" parameter in each request matches an api key in our list. 
                //config.MessageHandlers.Add(new ApiKeyHandler("key"));
            }
            catch (Exception ex)
            {
                log.ErrorEx((tr) => { tr.Message = "Exception encounted during configuration"; tr.Exception = ex; });
                throw ex;
            }
        }
    }
}
