using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_Thread_Web_Server.Routes
{
    class Hello: IRoute
    {
        public Hello(string method, string path, Dictionary<string, string> parameters, string body) : base(method, path, parameters, body) { }

        public override IResponse DoWork()
        {
            int statusCode = 200;
            string body;
            string contentType;

            if(!this.Parameters.ContainsKey("name"))
            {
                statusCode = 400;
                body = $"<h4>{Method} {Path} Bad Request. Missing name in query string</h4>";
                contentType = "text/html";

            } 
            else
            {
                body = $"{{\"message\": \"Hello {this.Parameters["name"]}\"}}";
                contentType = "application/json";
            }

            return new IResponse(
                statusCode,
                body,
                contentType
            );
        }
    }
}
