using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace DebugHelper
{

    public static class DbgConsole
    {
        [Conditional("DEBUG")]
        public static void Message(String message)
        {
            StackTrace stackTrace = new StackTrace();
            Console.WriteLine("<i> " + stackTrace.GetFrame(1).GetMethod().ReflectedType.FullName + "." + stackTrace.GetFrame(1).GetMethod().Name + " : " + message);
        }
        [Conditional("DEBUG")]
        public static void Error(String message)
        {
            StackTrace stackTrace = new StackTrace();
            Console.WriteLine("<e> " + stackTrace.GetFrame(1).GetMethod().ReflectedType.FullName + "." + stackTrace.GetFrame(1).GetMethod().Name + " : " + message);
        }
    }

}