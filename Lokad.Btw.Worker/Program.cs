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
            env.Log.Info("Type 'usage' to get more info");
            

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
                    env.Log.Error("Unknown command '{0}'. Type 'usage' for help", line);
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
                catch (Exception ex)
                {
                    env.Log.ErrorException(ex, "Failure while processing command '{0}'", split[0]);
                }
                
            }
        }

        static Environment BuildEnvironment()
        {
            var store = new InMemoryStore();
            var fas = new FactoryApplicationService(store, null);
            

            return new Environment()
                {
                    Events = store,
                    FactoryAppService = fas,
                    Handlers = ConsoleCommands.Commands,

                };
        }
    }

    public class Environment
    {
        public IEventStore Events;
        public FactoryApplicationService FactoryAppService;
        public IDictionary<string, ConsoleCommand> Handlers;
        public ILogger Log = LogManager.GetLoggerFor<Environment>();
    }

    public sealed class InMemoryStore : IEventStore
    {
        ConcurrentDictionary<string, IList<IEvent>> _store = new ConcurrentDictionary<string, IList<IEvent>>();

        static ILogger Log = LogManager.GetLoggerFor<InMemoryStore>();
        public EventStream LoadEventStream(string id)
        {
            var stream = _store.GetOrAdd(id, new IEvent[0]).ToList();

            return new EventStream()
                {
                    Events = stream,
                    StreamVersion = stream.Count
                };
        }

        public void AppendEventsToStream(string id, long expectedVersion, ICollection<IEvent> events)
        {
            foreach (var @event in events)
            {
                Log.Info("{0}", @event);
            }
            _store.AddOrUpdate(id, events.ToList(), (s, list) => list.Concat(events).ToList());
        }
    }
}
