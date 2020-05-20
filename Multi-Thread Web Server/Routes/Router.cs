using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_Thread_Web_Server.Routes
{
    class Router
    {
        Dictionary<KeyValuePair<string, string>, Type> routes = new Dictionary<KeyValuePair<string, string>, Type>();

        public Router() 
        {
            RegisterRoute(new KeyValuePair<string, string>("/", "GET"), typeof(TestRoute));
            RegisterRoute(new KeyValuePair<string, string>("/fibonacci", "GET"), typeof(Fibonacci));
            RegisterRoute(new KeyValuePair<string, string>("/hello", "GET"), typeof(Hello));
        }

        public void RegisterRoute(KeyValuePair<string, string> path, Type route)
        {
            routes.Add(path, route);
        }

        public Type Navigate(string path, string method)
        {
            KeyValuePair<string, string> kv = new KeyValuePair<string, string>(path, method);
            
            try
            {
                return routes[kv];
            }
            catch(Exception)
            {
                return typeof(NotFound);
            }
        }
    }
}
