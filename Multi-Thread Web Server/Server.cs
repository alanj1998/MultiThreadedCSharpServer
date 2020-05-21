using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Multi_Thread_Web_Server.Routes;
using System.Threading;
using System.Net;
using System.IO;
using System.Collections.ObjectModel;

namespace Multi_Thread_Web_Server
{
    /// <summary>
    /// Server class which exposes a port interface to accept HTTP requests.
    /// </summary>
    public class Server
    {
        private Threading _workers;
        private HttpListener _tcp;

        public ObservableCollection<KeyValuePair<IRoute, IResponse>> servedRequests = new ObservableCollection<KeyValuePair<IRoute, IResponse>>();

        private Dictionary<int, KeyValuePair<IRoute, HttpListenerContext>> sockets;
        int id = 0;
        private Thread requestHandlingThread;
        private Thread serverThread;
        private Router r = new Router();

        /// <summary>
        /// Upon creation of the server, it initialises the threading worker class,
        /// and prepares two threads.
        /// serverThread is used to take in requests.
        /// requestHandlingThread is used for handling all the requests.
        /// Currently it is set to allow the requestHandlingThread to finish.
        /// </summary>
        /// <param name="threads"></param>
        /// <param name="PORT"></param>
        /// <param name="console"></param>
        public Server(int threads, int PORT, IConsole console)
        {
            this._workers = new Threading(threads, SendBackResponse, console);

            this._tcp = new HttpListener();
            this._tcp.Prefixes.Add($"http://localhost:{PORT}/");
            this.sockets = new Dictionary<int, KeyValuePair<IRoute, HttpListenerContext>>();
            this.requestHandlingThread = new Thread(HandleRequest);
            this.serverThread = new Thread(ServerThread);
            this.serverThread.Priority = ThreadPriority.AboveNormal;
        }

        /// <summary>
        /// Listen method starts the TCP listener and also starts the server thread which was created in the constructor.
        /// </summary>
        public void Listen()
        {
            this._tcp.Start();
            this.serverThread.Start();
        }

        /// <summary>
        /// The Server Thread takes in the HTTP request and then passes it down to the RequestHandling Thread.
        /// </summary>
        private void ServerThread()
        {
            while (this._tcp.IsListening)
            {
                HttpListenerContext ctx = this._tcp.GetContext();

                this.requestHandlingThread = new Thread(HandleRequest);
                this.requestHandlingThread.Start(ctx);
                this.requestHandlingThread.Join();
            }
        }

        /// <summary>
        /// The request handler thread, takes in the request context, applies it through the routing algorithm,
        /// uses reflection to create the handler class and passes it down to the worker threads.
        /// </summary>
        /// <param name="context"></param>
        private void HandleRequest(object context)
        {
            HttpListenerContext ctx = context as HttpListenerContext;
            HttpListenerRequest req = ctx.Request;

            string text = "";
            using (var reader = new StreamReader(req.InputStream,
                                 req.ContentEncoding))
            {
                text = reader.ReadToEnd();
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            
            foreach(string key in req.QueryString.AllKeys)
            {
                parameters.Add(key.ToLower(), req.QueryString[key]);
            }
            Type t = this.r.Navigate(req.Url.AbsolutePath, req.HttpMethod);
            IRoute r = (IRoute)Activator.CreateInstance(t, req.HttpMethod, req.Url.AbsolutePath, parameters, text);

            id++;

            Monitor.Enter(this.sockets);
            this.sockets.Add(id, new KeyValuePair<IRoute, HttpListenerContext>(r, ctx));
            Monitor.Pulse(this.sockets);
            Monitor.Exit(this.sockets);

            this._workers.AddWork(id, r);
        }

        /// <summary>
        /// Callback method which returns the response to the user once the worker threads are finished with it.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="res"></param>
        private void SendBackResponse(int id, IResponse res)
        {
            Monitor.Enter(this.sockets);
            KeyValuePair<IRoute, HttpListenerContext> kv = this.sockets[id];
            this.sockets.Remove(id);

            Monitor.Pulse(this.sockets);
            Monitor.Exit(this.sockets);


            HttpListenerResponse conn = kv.Value.Response;
            try
            {
                byte[] bodyBytes = Encoding.UTF8.GetBytes(res.Body);
                conn.StatusCode = res.StatusCode;
                conn.ContentLength64 = bodyBytes.Length;
                conn.ContentType = res.ContentType;
                conn.OutputStream.Write(bodyBytes, 0, bodyBytes.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                conn.Close();
                this.servedRequests.Insert(0, new KeyValuePair<IRoute, IResponse>(kv.Key, res));
            }
        }

        /// <summary>
        /// Stop All Threads stops the server from working by aborting all threads.
        /// </summary>
        public void StopAllThreads()
        {
            this._tcp.Stop();
            this.serverThread.Abort();
            this.requestHandlingThread.Abort();
            this._workers._workerThreads.ForEach(thread =>
            {
                thread.Abort();
            });
        }
    }
}
