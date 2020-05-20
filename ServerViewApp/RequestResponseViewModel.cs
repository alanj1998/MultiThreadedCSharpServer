using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerViewApp
{
    public class RequestResponseViewModel
    {
        private int _statusCode;

        public string Method { get; set; }
        public string Path { get; set; }
        public int StatusCode {
            get { return this._statusCode; }
            set
            {
                this._statusCode = value;
                this.StatusText = this.GetStatusTextFromCode(value);
            }
        }

        public string Color
        {
            get
            {
                string color;
                switch(this._statusCode)
                {
                    case 200:
                        color = "green";
                        break;
                    default:
                        color = "red";
                        break;
                }

                return color;
            }
        }
        public string StatusText { get; private set; }

        public string Time { get; set; }

        private string GetStatusTextFromCode(int statusCode)
        {
            switch(statusCode)
            {
                case 200:
                    return "Ok";
                case 400:
                    return "Bad Request";
                case 404:
                default:
                    return "Not Found";
            }
        }
    }
}
