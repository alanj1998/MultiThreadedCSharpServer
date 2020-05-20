using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
using System.IO;

namespace ServerViewApp
{
    class AppIsolatedStorage
    {
        public void SavePortToUser(string port)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                try
                {
                    store.CreateDirectory("ServerStorage");
                    IsolatedStorageFileStream stream = store.CreateFile($"ServerStorage/UserSettings");

                    using (StreamWriter str = new StreamWriter(stream))
                    {
                        str.WriteLine(port);
                    }
                }
                catch (Exception err)
                {
                    throw err;
                }
            }
        }

        public int GetPortForUser()
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                try
                {
                    IsolatedStorageFileStream f = store.OpenFile("ServerStorage/UserSettings", FileMode.Open);
                    byte[] byteArr = new byte[f.Length];
                    int port = -1;

                    f.Read(byteArr, 0, byteArr.Length);
                    string s = Encoding.UTF8.GetString(byteArr);

                    int.TryParse(s, out port);
                    f.Close();
                    return port;
                }
                catch (Exception err)
                {
                    throw err;
                }
            }
        }

        public void SaveLogs(List<string> s)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                try
                {
                    store.CreateDirectory("ServerStorage");
                    IsolatedStorageFileStream stream = store.CreateFile($"ServerStorage/{DateTime.Now.ToString("yyyyMMddHHmmssffff")}Log");

                    using (StreamWriter str = new StreamWriter(stream))
                    {
                        for (int i = 0; i < s.Count(); i++)
                        {
                            str.WriteLine(s[i]);
                        }
                    }
                }
                catch (Exception err)
                {
                    throw err;
                }
            }
        }
    }
}
