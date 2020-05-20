using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_Thread_Web_Server.Routes
{
    public class Fibonacci : IRoute
    {
        public Fibonacci(string method, string path, Dictionary<string, string> parameters, string body) : base(method, path, parameters, body) { }
        public override IResponse DoWork()
        {
            string s = "";

            int a = 0, b = 1, c = 0;
            for (int i = 2; i < 10; i++)
            {
                c = a + b;
                s += " " + c;
                a = b;
                b = c;
            }

            return new IResponse(200, $"{{\"result\": \"{s}\"}}", "application/json");
        }
    }
}
