using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using E014.Domain.ApplicationServices;
using E014.Domain.ApplicationServices.Factory;
using NUnit.Framework;
// ReSharper disable InconsistentNaming
namespace E014.Domain
{
    /// <summary>
    /// This class scans all available specifications for messages used
    /// then performs round-trip via specified serializer,
    /// and then does the structural comparison of resulting values
    /// </summary>
    [TestFixture]
    public sealed class test_message_serialization
    {

        static IEnumerable<Assembly> AssembliesWithSpecs()
        {
            yield return typeof(assign_employee_to_factory).Assembly;
        }

        static IEnumerable<SerializerInfo> RegisterSerializers()
        {
            
            yield return new SerializerInfo("Binary", a =>
                {
                    var binary = new DataContractSerializer(a.GetType());
                    using (var mem = new MemoryStream())
                    {
                        binary.WriteObject(mem, a);
                        mem.Seek(0, SeekOrigin.Begin);
                        return binary.ReadObject(mem);
                    }
                });
        }



        Group[] _messages;
        SerializerInfo[] _serializers;


        [TestFixtureSetUp]
        public void Setup()
        {
            _messages = ListMessages();
            _serializers = RegisterSerializers().ToArray();

        }

        sealed class SerializerInfo
        {
            public readonly string Name;
            public readonly Func<object, object> DeepClone;

            public SerializerInfo(string name, Func<object, object> deepClone)
            {
                Name = name;
                DeepClone = deepClone;
            }
        }

        static Group[] ListMessages()
        {
            var types = AssembliesWithSpecs().SelectMany(t => t.GetExportedTypes());

            return types
                .Where(t => typeof(IListSpecifications).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract)
                .SelectMany(t => ((IListSpecifications)Activator.CreateInstance(t)).ListSpecifications())
                .SelectMany(GetGroups)
                .Where(g => !(g.Message is IAmFakeEventForTesting))
                .GroupBy(g => g.Message.GetType())
                .Select(g => new Group(g.ToArray(), g.Key))
                .ToArray();
        }

        [Test]
        public void messages_should_be_present()
        {
            CollectionAssert.IsNotEmpty(_messages, "This test should pick up messages");
        }

        [Test]
        public void serializers_should_be_present()
        {
            CollectionAssert.IsNotEmpty(_serializers, "This test should have serializers");
        }

        public sealed class Group
        {
            public readonly IEnumerable<Source> Messages;
            public readonly Type Type;

            public Group(IEnumerable<Source> messages, Type type)
            {
                Messages = messages;
                Type = type;
            }

            public override string ToString()
            {
                return Type.Name + " x" + Messages.Count();
            }
        }

        public sealed class Source
        {
            public readonly IMessage Message;
            public readonly string Origin;

            public Source(IMessage message, string origin)
            {
                Message = message;
                Origin = origin;
            }
        }

        static IEnumerable<Source> GetGroups(SpecificationInfo run)
        {
            var name = string.Format("{0} {1}", run.GroupName, run.CaseName);

            foreach (var @event in run.Given)
            {
                yield return new Source(@event, name + " Given");
            }
            yield return new Source(run.When, name + " When");
            foreach (var w in run.Then)
            {
                yield return new Source(w, name + " Expect");
            }
        }

        [TestCaseSource("ListMessages")]
        public void serialization_should_work_for(Group msgs)
        {
            var list = new List<string>();
            int count = 0;
            
            foreach (var exp in msgs.Messages)
            {
                count++;
                var expected = exp.Message;

                foreach (var serializer in _serializers)
                {
                    var actual = serializer.DeepClone(expected);
                    var compare = CompareObjects.FindDifferences(expected, actual);
                    if (!string.IsNullOrEmpty(compare))
                    {
                        list.Add(serializer.Name + ": " + exp.Origin + Environment.NewLine + compare);
                    }
                }
               
            }
            if (list.Count > 0)
            {
                Assert.Fail("{0} out of {1}\r\n{2}", list.Count, count, string.Join(Environment.NewLine, list));
            }
        }
    }
}