using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform;

namespace Lokad.Btw.Worker
{
    class Program
    {

        static ILogger Log = LogManager.GetLoggerFor<Program>();
        static void Main(string[] args)
        {
            Log.Info("Starting :)");

            Console.ReadLine();
            

        }
    }
}
