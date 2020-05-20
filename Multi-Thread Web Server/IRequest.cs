﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_Thread_Web_Server
{
    abstract public class IRoute
    {
        public Dictionary<string, string> Parameters { get; set; }
        public string Body { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public DateTime Time { get; set; }

        public IRoute(string method, string path, Dictionary<string, string> parameters, string body) 
        {
            this.Method = method;
            this.Path = path;
            this.Parameters = parameters;
            this.Body = body;
            this.Time = DateTime.Now;
        }
        public abstract IResponse DoWork();
    }

    public class IResponse
    {
        public int StatusCode { get; set; }
        public string Body { get; set; }
        public string ContentType { get; set; }

        public IResponse(int statusCode, string body, string contentType)
        {
            this.StatusCode = statusCode;
            this.Body = body;
            this.ContentType = contentType;
        }
    }
}
