using E014.Contracts;
using NUnit.Framework;
// ReSharper disable InconsistentNaming
namespace E014.Domain.ApplicationServices.Factory
{
    public class produce_a_car : factory_application_service_spec
    {
        [Test]
        public void fry_not_assigned_to_factory()
        {
            Given(new FactoryOpened(FactoryId.ForTest));
            When(new ProduceACar(FactoryId.ForTest, "fry", "Ford", Library));

            Expect("unknown-employee");
        }

        [Test]
        public void part_not_found()
        {
            Given(
                     new FactoryOpened(FactoryId.ForTest),
                     Library.RecordBlueprint("Ford", new CarPart("chassis", 1)),
                     new EmployeeAssignedToFactory(FactoryId.ForTest, "fry")
                );
            When(new ProduceACar(FactoryId.ForTest, "fry", "Ford", Library));
            Expect("part-not-found");
        }

        [Test]
        public void cart_model_not_found()
        {
            Given(
                     new FactoryOpened(FactoryId.ForTest),
                     new EmployeeAssignedToFactory(FactoryId.ForTest, "fry")
                );
            When(new ProduceACar(FactoryId.ForTest, "fry", "Volvo", Library));
            Expect("car-model-not-found");
        }

        [Test]
        public void produced_car()
        {
            Given(
                    Library.RecordBlueprint("death star", new CarPart("magic box", 10)),
                    Library.RecordBlueprint("Ford", new CarPart("chassis", 1)),
                    new FactoryOpened(FactoryId.ForTest),
                    new EmployeeAssignedToFactory(FactoryId.ForTest, "fry"),
                    new ShipmentUnpackedInCargoBay(FactoryId.ForTest, "fry", new[] { new InventoryShipment("ship-1", new[] { new CarPart("chassis", 1), new CarPart("wheels", 4), new CarPart("engine", 1) }) })
                );

            When(new ProduceACar(FactoryId.ForTest, "fry", "Ford", Library));

            Expect(new CarProduced(FactoryId.ForTest,"fry", "Ford", new[] { new CarPart("chassis", 1), new CarPart("wheels", 4), new CarPart("engine", 1) }));
        }

        [Test]
        public void factory_not_open()
        {
            When(new ProduceACar(FactoryId.ForTest, "fry", "Ford", Library));
            Expect("factory-is-not-open");
        }
    }
}
