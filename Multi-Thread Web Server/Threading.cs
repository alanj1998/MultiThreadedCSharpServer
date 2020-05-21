using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Multi_Thread_Web_Server
{
    /// <summary>
    /// Worker class which does all the heavy lifting of the server
    /// </summary>
    class Threading
    {
        public List<Thread> _workerThreads = new List<Thread>();
        private Action<int, IResponse> OnThreadFinish;
        private Queue<KeyValuePair<int, IRoute>> workQueue = new Queue<KeyValuePair<int, IRoute>>();
        private IConsole console;

        [ThreadStaticAttribute]
        static int id;

        /// <summary>
        /// Upon creation of a worker class, a number of threads is instantiatied and started.
        /// An action is passed in which acts as a callback when the thread is finished working.
        /// Also a console abstraction is passed to allow easy logging.
        /// </summary>
        /// <param name="numberOfThreads"></param>
        /// <param name="onThreadFinish"></param>
        /// <param name="console"></param>
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

        /// <summary>
        /// This is the thread work method. It tries to grab a hold of the work queue which stores
        /// all of the work that has to be done. Once it has the lock on it, it dequeues a single item
        /// and returns the lock for someone else.
        /// The dequeued item is ran on this thread until finish after which the thread returns to handling more work.
        /// </summary>
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

        /// <summary>
        /// Simple method used to add work to the queue.
        /// Made to run on the server thread.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="r"></param>
        public void AddWork(int id, IRoute r)
        {
            KeyValuePair<int, IRoute> kv = new KeyValuePair<int, IRoute>(id, r);
            workQueue.Enqueue(kv);
        }
    }
}
