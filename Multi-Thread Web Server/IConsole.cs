using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace Multi_Thread_Web_Server
{
    /// <summary>
    /// Console abstraction to allow different logging techinques.
    /// </summary>
    public interface IConsole
    {
        void WriteLine(string text);
    }

    public class ServerConsole: IConsole
    {
        public void WriteLine(string text)
        {
            Console.WriteLine(text);
        }
    }

    public class UIConsole: IConsole
    {
        public ObservableCollection<string> Logs { get; set; }

        public UIConsole()
        {
            Logs = new ObservableCollection<string>();
        }
        public void WriteLine(string text)
        {
            Logs.Insert(0, text);
        }
    }
}
