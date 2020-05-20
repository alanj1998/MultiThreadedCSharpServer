using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Multi_Thread_Web_Server
{
    class Threading
    {
        public List<Thread> _workerThreads = new List<Thread>();
        private Action<int, IResponse> OnThreadFinish;
        private Queue<KeyValuePair<int, IRoute>> workQueue = new Queue<KeyValuePair<int, IRoute>>();
        private IConsole console;

        [ThreadStaticAttribute]
        static int id;

        public Threading(int numberOfThreads, Action<int, IResponse> onThreadFinish, IConsole console)
        {
            this.OnThreadFinish = onThreadFinish;
            this.console = console;

            for(int i = 0; i < numberOfThreads; i++)
            {
                Thread t = new Thread(new ThreadStart(DoThreadWork));
                t.IsBackground = true;
                t.Name = "Thread " + (i + 1).ToString();

                _workerThreads.Add(t);
                _workerThreads[i].Start();
            }
        }

        public void DoThreadWork()
        {
            while (true)
            {
                if (Monitor.TryEnter(workQueue, 5000))
                {
                    if (workQueue.Count < 1)
                    {
                        Monitor.Pulse(workQueue);
                        Monitor.Exit(workQueue);
                        Thread.Sleep(100);
                    }
                    else
                    {
                        KeyValuePair<int, IRoute> kv = workQueue.Dequeue();
                        IRoute req = kv.Value;
                        id = kv.Key;

                        this.console.WriteLine($"[{Thread.CurrentThread.Name}: {DateTime.Now.ToString()}] Handling request with id {id}");

                        Monitor.Pulse(workQueue);
                        Monitor.Exit(workQueue);
                      
                        IResponse res = req.DoWork();
                        this.console.WriteLine($"[{Thread.CurrentThread.Name}: {DateTime.Now.ToString()}] Finished request with id {id}");

                        Monitor.Enter(OnThreadFinish);
                        OnThreadFinish(id, res);

                        Monitor.Pulse(OnThreadFinish);
                        Monitor.Exit(OnThreadFinish);
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        public void AddWork(int id, IRoute r)
        {
            KeyValuePair<int, IRoute> kv = new KeyValuePair<int, IRoute>(id, r);
            workQueue.Enqueue(kv);
        }
    }
}
