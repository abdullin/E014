using System;
using System.Linq;
using System.Threading;
using E014;
using E014.ApplicationServices.Factory;

namespace Lokad.Btw.Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            var env = ConsoleEnvironment.BuildEnvironment();
            env.Log.Info("Starting Being The Worst interactive shell :)");
            env.Log.Info("Type 'help' to get more info");
            

            // TODO: add distance-based suggestions
            while(true)
            {
                Thread.Sleep(300);
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                var split = line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                IShellCommand value;
                if (!env.Handlers.TryGetValue(split[0],out value))
                {
                    env.Log.Error("Unknown command '{0}'. Type 'help' for help", line);
                    continue;
                }
                try
                {
                    value.Execute(env, split.Skip(1).ToArray());
                }
                catch (DomainError ex)
                {
                    env.Log.Error("{0}: {1}", ex.Name, ex.Message);
                }
                catch(ArgumentException ex)
                {
                    env.Log.Error("Invalid usage of '{0}': {1}",split[0], ex.Message);
                    env.Log.Debug(value.Usage);
                }
                catch (Exception ex)
                {
                    env.Log.ErrorException(ex, "Failure while processing command '{0}'", split[0]);
                }
            }
        }
    }
}
