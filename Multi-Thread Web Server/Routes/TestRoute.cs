using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_Thread_Web_Server.Routes
{
    class TestRoute : IRoute
    {
        public TestRoute(string method, string path, Dictionary<string, string> parameters, string body) : base(method, path, parameters, body) {}

        public override IResponse DoWork()
        {
            return new IResponse(200, "{\"ok\": true}", "application/json");
        }
    }
}
