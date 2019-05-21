using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ModbusDirekt
{
    internal static class Logger
    {
        static Action<string> WriteInfo;
        static Action<string> WriteDebug;
        static Action<string> WriteError;

        public static void Info(string msg)
        {
            WriteInfo?.Invoke(msg);
        }

        internal static void Error(string msg)
        {
            WriteError?.Invoke(msg);
        }

        internal static void Debug(string msg)
        {
            WriteDebug?.Invoke(msg);
        }
    }
}