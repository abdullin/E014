using System;
using System.Collections.Generic;
using E014.Contracts;

namespace Lokad.Btw.Worker
{
    public static class ConsoleCommands
    {

        public static IDictionary<string,Action<Environment,string[]>> RegisterCommands()
        {
            return new Dictionary<string, Action<Environment, string[]>>(StringComparer.InvariantCultureIgnoreCase)
                {
                    {"open", OpenFactory},
                    {"usage", Usage},
                    {"exit", Exit}
                };
        } 

        public static void OpenFactory(Environment env, string[] args)
        {
            if (args.Length != 1)
            {
                env.Log.Error("FactoryId expected");
                return;
            }
            int id = int.Parse(args[0]);

            env.FactoryAppService.When(new OpenFactory(new FactoryId(id)));
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
}