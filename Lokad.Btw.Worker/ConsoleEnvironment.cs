using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using E014;
using E014.ApplicationServices.Factory;
using E014.Contracts;
using Platform;

namespace Lokad.Btw.Worker
{
    public class ConsoleEnvironment
    {
        public IEventStore Events;
        public FactoryApplicationService FactoryAppService;
        public IDictionary<string, ConsoleCommand> Handlers;
        public ILogger Log = LogManager.GetLoggerFor<ConsoleEnvironment>();
    }

    public sealed class InMemoryStore : IEventStore
    {
        readonly ConcurrentDictionary<string, IList<IEvent>> _store = new ConcurrentDictionary<string, IList<IEvent>>();

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

    public sealed class InMemoryBlueprintLibrary : ICarBlueprintLibrary
    {
        IDictionary<string,CarBlueprint> _bluePrints = new Dictionary<string, CarBlueprint>(StringComparer.InvariantCultureIgnoreCase);
        public CarBlueprint TryToGetBlueprintForModelOrNull(string modelName)
        {
            CarBlueprint value;
            if (_bluePrints.TryGetValue(modelName,out value))
            {
                return value;
            }
            return null;
        }
    }

}