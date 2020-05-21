using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_Thread_Web_Server
{
    class Program
    {
        /// <summary>
        /// Incase the server is to be ran using a console, this interface is given.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            bool isCorrectAnswer = false;
            int threadCount;
            Server s;

            Console.WriteLine("Multi-Threaded Server - S00168420");

            do
            {
                Console.Write("Select how many threads you want to have available for the server: ");
                string res = Console.ReadLine();

                isCorrectAnswer = int.TryParse(res, out threadCount);

                ServerConsole sc = new ServerConsole();
                if(threadCount > -1)
                {
                    s = new Server(threadCount, 8080, sc);
                    s.Listen();
                } else
                {
                    Console.WriteLine("Wrong input detected...\n");
                    continue;
                }
            } while (!isCorrectAnswer);
        }
    }
}
