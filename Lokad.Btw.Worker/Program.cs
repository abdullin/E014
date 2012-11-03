using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using E014;
using E014.ApplicationServices.Factory;
using Platform;

namespace Lokad.Btw.Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            var env = BuildEnvironment();
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
                ConsoleCommand value;
                if (!env.Handlers.TryGetValue(split[0],out value))
                {
                    env.Log.Error("Unknown command '{0}'. Type 'help' for help", line);
                    continue;
                }
                try
                {
                    value.Processor(env, split.Skip(1).ToArray());
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

        static ConsoleEnvironment BuildEnvironment()
        {
            var store = new InMemoryStore();
            var fas = new FactoryApplicationService(store, null);
            

            return new ConsoleEnvironment()
                {
                    Events = store,
                    FactoryAppService = fas,
                    Handlers = ConsoleCommands.Commands,

                };
        }
    }

}
