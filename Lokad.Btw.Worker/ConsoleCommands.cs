using System;
using System.Collections.Generic;
using System.Linq;
using E014.Contracts;

namespace Lokad.Btw.Worker
{
    public static class ConsoleCommands
    {
        public static IDictionary<string,ConsoleCommand> Commands = new Dictionary<string,ConsoleCommand>(); 

        static ConsoleCommands()
        {
            Register("open", OpenFactory, "Opens a new factory: open <factoryId>");
            Register("help", Help, "Print Help: help [<command>]");
            Register("exit", Exit, "Exit the shell: exit");
            Register("assign", AssignEmployee, "Assign Employee: assign <factoryId> <employeeName>");
            Register("ship", RecieveShipment, "RecieveShipment: ship <factoryId> <shipment> [<part>,<part>...]");
            Register("unpack", UnpackAndInventoryShipmentInCargoBay, "Unpack shipment: unpack <factoryId> <shipment>");
        }
        static void Register(string keyword, Action<ConsoleEnvironment, string[]> processor, string description = null)
        {
            Commands.Add(keyword, new ConsoleCommand()
                {
                    Processor = processor,
                    Usage = description
                });
        }



        public static void OpenFactory(ConsoleEnvironment env, string[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("Expected at least 2 args");
            }
            var id = int.Parse(args[0]);

            env.FactoryAppService.When(new OpenFactory(new FactoryId(id)));
        }

        public static void AssignEmployee(ConsoleEnvironment env, string[] args)
        {
            if (args.Length < 2)
            {
                throw new ArgumentException("Expected at least 2 args");
            }
            var id = int.Parse(args[0]);
            var name = string.Join(" ", args.Skip(1));
            env.FactoryAppService.When(new AssignEmployeeToFactory(new FactoryId(id), name));
        }
        public static void RecieveShipment(ConsoleEnvironment env, string[] args)
        {
            if (args.Length < 2)
                throw new ArgumentException("Expected at least 2 args");
            
            var id = int.Parse(args[0]);
            var name = args[1];
            var parts = args.Skip(2).GroupBy(s => s).Select(g => new CarPart(g.Key, g.Count())).ToArray();
            env.FactoryAppService.When(new ReceiveShipmentInCargoBay(new FactoryId(id),name, parts));
        }

        public static void UnpackAndInventoryShipmentInCargoBay(ConsoleEnvironment env, string[] args)
        {
            if (args.Length < 2)
            {
                throw new ArgumentException("Expected at least 2 args");
            }
            var id = int.Parse(args[0]);
            var employee = string.Join(" ", args.Skip(1));
            env.FactoryAppService.When(new UnpackAndInventoryShipmentInCargoBay(new FactoryId(id),employee));
        }

        public static void Help(ConsoleEnvironment env, string[] args)
        {
            if (args.Length > 0)
            {
                ConsoleCommand value;
                if (!env.Handlers.TryGetValue(args[0],out value))
                {
                    env.Log.Error("Can't find help for '{0}'", args[0]);
                    return;
                }
                env.Log.Info(value.Usage ?? "No Help available");
                return;
            }
            env.Log.Info("Available commands");
            foreach (var handler in env.Handlers)
            {
                env.Log.Info("  {0}", handler.Key.ToUpperInvariant());
                if (!string.IsNullOrWhiteSpace(handler.Value.Usage))
                {
                    env.Log.Info("    {0}", handler.Value.Usage);
                }
            }
        }

        public static void Exit(ConsoleEnvironment env, string[] args)
        {
            System.Environment.Exit(0);
        }
    }

    public class ConsoleCommand
    {
        public string Usage;
        public Action<ConsoleEnvironment, string[]> Processor;
    }
}