using System;
using System.Collections.Generic;
using System.Linq;
using E014.Contracts;

namespace Lokad.Btw.Worker
{
    public static class ConsoleCommands
    {
        public static IDictionary<string,IShellCommand> Commands = new Dictionary<string,IShellCommand>(); 

        static ConsoleCommands()
        {
            Register(new OpenCommand());
            Register(new RegisterBlueprintCommand());
            Register(new HireEmployeeCommand());
            Register(new RecieveShipment());
            Register(new UnpackShipments());
            Register(new HelpCommand());
            Register(new ExitCommand());
            
        }
        static void Register(IShellCommand cmd)
        {
            Commands.Add(cmd.Keyword, cmd);
        }
    }

    public interface IShellCommand
    {
        string Keyword { get; }
        string Usage { get; }
        void Execute(ConsoleEnvironment env, string[] args);
    }

    public class OpenCommand : IShellCommand
    {
        public string Keyword { get { return "open"; } }
        public string Usage { get { return "open <factoryId> - opens new factory"; } }

        public void Execute(ConsoleEnvironment env, string[] args)
        {
            if (args.Length != 1)
                throw new ArgumentException("Expected at least 2 args");
            var id = int.Parse(args[0]);
            env.FactoryAppService.When(new OpenFactory(new FactoryId(id)));
        }
    }
    public class RegisterBlueprintCommand : IShellCommand
    {
        public string Keyword { get { return "reg"; } }
        public string Usage { get { return Keyword + " <design> [<part>, <part>...]"; } }
        public void Execute(ConsoleEnvironment env, string[] args)
        {
            if (args.Length < 2)
            {
                throw new ArgumentException("Expected at least 2 args");
            }
            
            var design = args[0];
            var parts = args.Skip(1).GroupBy(s => s).Select(g => new CarPart(g.Key, g.Count())).ToArray();
            env.Blueprints.Register(design, parts);
        }
    }

    public class HireEmployeeCommand : IShellCommand
    {
        public string Keyword { get { return "hire"; } }
        public string Usage { get { return "hire <employeeName>"; } }
        public void Execute(ConsoleEnvironment env, string[] args)
        {
            if (args.Length < 2)
            {
                throw new ArgumentException("Expected at least 2 args");
            }
            var id = int.Parse(args[0]);
            var name = string.Join(" ", args.Skip(1));
            env.FactoryAppService.When(new AssignEmployeeToFactory(new FactoryId(id), name));
        }
    }

    public class RecieveShipment : IShellCommand
    {
        public string Keyword { get { return "ship"; } }
        public string Usage { get { return "ship <factoryId> <shipment> [<part>,<part>...]"; } }
        public void Execute(ConsoleEnvironment env, string[] args)
        {
            if (args.Length < 2)
                throw new ArgumentException("Expected at least 2 args");

            var id = int.Parse(args[0]);
            var name = args[1];
            var parts = args.Skip(2).GroupBy(s => s).Select(g => new CarPart(g.Key, g.Count())).ToArray();
            env.FactoryAppService.When(new ReceiveShipmentInCargoBay(new FactoryId(id), name, parts));
        }
    }

    public class UnpackShipments : IShellCommand
    {
        public string Keyword { get { return "unpack"; } }
        public string Usage { get { return "unpack <factoryId> <shipment>"; } }
        public void Execute(ConsoleEnvironment env, string[] args)
        {
            if (args.Length < 2)
            {
                throw new ArgumentException("Expected at least 2 args");
            }
            var id = int.Parse(args[0]);
            var employee = string.Join(" ", args.Skip(1));
            env.FactoryAppService.When(new UnpackAndInventoryShipmentInCargoBay(new FactoryId(id), employee));

        }
    }

    public class HelpCommand : IShellCommand
    {
        public string Keyword { get { return "help"; } }
        public string Usage { get { return "help [<command>]"; } }
        public void Execute(ConsoleEnvironment env, string[] args)
        {
            if (args.Length > 0)
            {
                IShellCommand value;
                if (!env.Handlers.TryGetValue(args[0], out value))
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
    }

    public class ExitCommand : IShellCommand
    {
        public string Keyword { get { return "exit"; } }
        public string Usage { get { return "exit"; } }
        public void Execute(ConsoleEnvironment env, string[] args)
        {
            Environment.Exit(0);
        }
    }
}