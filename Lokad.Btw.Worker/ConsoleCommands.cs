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
            Register("open", OpenFactory, "open <factoryId> - Opens a new factory");
            Register("usage", Usage, "usage - prints usage");
            Register("exit", Exit, "exit - exit the shell");
            Register("assign", AssignEmployee, "assign <factoryId> <employeeName>");
        }
        static void Register(string keyword, Action<Environment, string[]> processor, string description = null)
        {
            Commands.Add(keyword, new ConsoleCommand()
                {
                    Processor = processor,
                    Usage = description
                });
        }



        public static void OpenFactory(Environment env, string[] args)
        {
            if (args.Length != 1)
            {
                env.Log.Error("FactoryId expected");
                return;
            }
            var id = int.Parse(args[0]);

            env.FactoryAppService.When(new OpenFactory(new FactoryId(id)));
        }

        public static void AssignEmployee(Environment env, string[] args)
        {
            if (args.Length < 2)
            {
                env.Log.Error("Expected 2 args");
                return;
            }
            var id = int.Parse(args[0]);
            var name = string.Join(" ", args.Skip(1));
            env.FactoryAppService.When(new AssignEmployeeToFactory(new FactoryId(id), name));
        }

        public static void Usage(Environment env, string[] args)
        {
            foreach (var handler in env.Handlers)
            {
                env.Log.Info("  {0}", handler.Key.ToUpperInvariant());
            }
        }

        public static void Exit(Environment env, string[] args)
        {
            System.Environment.Exit(0);
        }
    }

    public class ConsoleCommand
    {
        public string Usage;
        public Action<Environment, string[]> Processor;
    }
}