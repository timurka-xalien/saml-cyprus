using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Cameyo.SamlPoc.WebApp.Services
{
    public class SamlPocTraceListener : TraceListener
    {
        private const string ComponentSpaceMessagePrefix = "ComponentSpace.SAML2 Verbose: 0 : ";

        public override void Write(string message)
        {
            if (message == ComponentSpaceMessagePrefix)
            {
                return;
            }

            Log("SAML", "ComponentSpace: " + StripProcessThreadDate(message), false);
        }

        public override void WriteLine(string message)
        {
            Log("SAML", "ComponentSpace: " + StripProcessThreadDate(message), true);
        }

        private string StripProcessThreadDate(string message)
        {
            if (!message.StartsWith(Process.GetCurrentProcess().Id.ToString()))
            {
                return message;
            }

            var messageParts = message.Split(':');

            return string.Join(":", messageParts.Skip(4)).TrimStart();
        }

        // Use it in the form: Log("SAML", "SomeFunction: something");
        public static void Log(string component, string msg, bool addNewLine = true)
        {
            var logFile = Path.Combine(Path.GetTempPath(), component + ".log");

            try
            {
                using (StreamWriter sw = File.AppendText(logFile))
                {
                    var now = DateTime.UtcNow;

                    var lineEnding = addNewLine ? "\r\n" : "";

                    var line = 
                        now.ToString() + 
                        " [PID=" + Process.GetCurrentProcess().Id + " #" + Process.GetCurrentProcess().SessionId +
                        " TID=" + Thread.CurrentThread.ManagedThreadId + "] " + 
                        component + ": " + msg + 
                        lineEnding;

                    sw.Write(line);
                }
            }
            catch
            {
                Debug.WriteLine("** Couldn't write to log file: " + logFile);
            }
        }
    }
}