using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using E014;
using E014.ApplicationServices.Factory;
using E014.Contracts;
using Microsoft.CSharp.RuntimeBinder;
using Platform;

namespace Lokad.Btw.Worker
{
    public class ConsoleEnvironment
    {
        public IEventStore Events;
        public FactoryApplicationService FactoryAppService;
        public IDictionary<string, IShellAction> Handlers;
        public InMemoryBlueprintLibrary Blueprints;
        public ILogger Log = LogManager.GetLoggerFor<ConsoleEnvironment>();
        public ActiveFactoriesProjection ActiveFactories;
        public WorkerRegistryProjection WorkerRegistry;

        public static ConsoleEnvironment BuildEnvironment()
        {
            var handler = new SynchronousEventHandler();
            var store = new InMemoryStore(handler);

            var blueprints = new InMemoryBlueprintLibrary();
            blueprints.Register("model-t", new CarPart("wheel",4), new CarPart("engine",1), new CarPart("chassis",1));
            var fas = new FactoryApplicationService(store, blueprints);



            // wire projections
            var activeFactories = new ActiveFactoriesProjection();
            handler.RegisterHandler(activeFactories);

            var workerRegistry = new WorkerRegistryProjection();
            handler.RegisterHandler(workerRegistry);

            return new ConsoleEnvironment
                {
                    Events = store,
                    FactoryAppService = fas,
                    Handlers = ConsoleActions.Actions,
                    Blueprints = blueprints,
                    ActiveFactories = activeFactories,
                    WorkerRegistry = workerRegistry
                };
        }
    }

    public sealed class InMemoryStore : IEventStore
    {
        readonly ConcurrentDictionary<string, IList<IEvent>> _store = new ConcurrentDictionary<string, IList<IEvent>>();
        readonly SynchronousEventHandler _handler;

        static ILogger Log = LogManager.GetLoggerFor<InMemoryStore>();
        public InMemoryStore(SynchronousEventHandler handler)
        {
            _handler = handler;
        }

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
            _store.AddOrUpdate(id, events.ToList(), (s, list) => list.Concat(events).ToList());
            
            foreach (var @event in events)
            {
                Log.Info("{0}", @event);

                _handler.Handle(@event);
            }
        }
    }


    public sealed class SynchronousEventHandler
    {
        readonly IList<object> _handlers = new List<object>(); 
        public void Handle(IEvent @event)
        {
            foreach (var handler in _handlers)
            {
                try
                {
                    ((dynamic) handler).When((dynamic) @event);
                }
                catch (RuntimeBinderException e)
                {
                    // binding failure. Ignore
                }
            }
        }
        public void RegisterHandler(object projection)
        {
            _handlers.Add(projection);
        }
    }


    public sealed class ActiveFactoriesProjection
    {
        public IDictionary<FactoryId,FactoryInfo> Factories = new Dictionary<FactoryId, FactoryInfo>();

        public sealed class FactoryInfo
        {
            public int WorkerCount;
            public int PartsInCargoBay;
        }


        public void When(FactoryOpened e)
        {
            Factories[e.Id] = new FactoryInfo();
        }
        public void When(EmployeeAssignedToFactory e)
        {
            Factories[e.Id].WorkerCount += 1;
        }
        public void When(ShipmentReceivedInCargoBay e)
        {
            Factories[e.Id].PartsInCargoBay += e.Shipment.Cargo.Sum(p => p.Quantity);
        }
        public void When(ShipmentUnpackedInCargoBay e)
        {
            Factories[e.Id].PartsInCargoBay -= e.InventoryShipments.Sum(s => s.Cargo.Sum(p => p.Quantity));
        }
    }

    public sealed class WorkerRegistryProjection
    {
        public List<Tuple<string,FactoryId>> List = new List<Tuple<string, FactoryId>>();
  
        public void When(EmployeeAssignedToFactory e)
        {
            List.Add(Tuple.Create(e.EmployeeName, e.Id));
        }
        
    }

    public sealed class InMemoryBlueprintLibrary : ICarBlueprintLibrary
    {
        readonly IDictionary<string,CarBlueprint> _bluePrints = new Dictionary<string, CarBlueprint>(StringComparer.InvariantCultureIgnoreCase);

        static ILogger Log = LogManager.GetLoggerFor<InMemoryBlueprintLibrary>();
        public CarBlueprint TryToGetBlueprintForModelOrNull(string modelName)
        {
            CarBlueprint value;
            if (_bluePrints.TryGetValue(modelName,out value))
            {
                return value;
            }
            return null;
        }
        public void Register(string modelName, params CarPart[] parts)
        {
            Log.Debug("Adding new design {0}: {1}", modelName, string.Join(", ", parts.Select(p => string.Format("{0} x {1}", p.Quantity, p.Name))));
            _bluePrints[modelName] = new CarBlueprint(modelName, parts);
        }
    }
}