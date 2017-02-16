using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Dependencies;

namespace PayAtTable.API.Helpers
{
    public class TinyIocMvcDependencyResolver : IDependencyResolver
    {
        private TinyIoC.TinyIoCContainer _container;
        public TinyIocMvcDependencyResolver(TinyIoC.TinyIoCContainer container)
        {
            _container = container;
        }
        public object GetService(Type serviceType)
        {
            try
            {
                return _container.Resolve(serviceType);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return _container.ResolveAll(serviceType, true);
            }
            catch (Exception)
            {
                return Enumerable.Empty<object>();
            }
        }

        public IDependencyScope BeginScope()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

}