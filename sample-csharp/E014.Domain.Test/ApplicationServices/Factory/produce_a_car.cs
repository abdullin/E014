using E014.Contracts;
using NUnit.Framework;
// ReSharper disable InconsistentNaming
namespace E014.Domain.ApplicationServices.Factory
{
    public class produce_a_car : factory_application_service_spec
    {

        [Test]
        public void assigning_employee_not_in_factory_is_an_error()
        {
            Given(new FactoryOpened(FactoryId.ForTest));
            When(new ProduceACar(FactoryId.ForTest, "fry", "Ford"));

            Expect("unknown-employee");
        }

        [Test]
        public void missing_required_car_part_is_an_error()
        {
            Given(
                     new FactoryOpened(FactoryId.ForTest),
                     Library.RecordBlueprint("Ford", new CarPart("chassis", 1)),
                     new EmployeeAssignedToFactory(FactoryId.ForTest, "fry")
                );
            When(new ProduceACar(FactoryId.ForTest, "fry", "Ford"));
            Expect("required-part-not-found");
        }

        [Test]
        public void car_model_not_in_blueprint_library_is_an_error()
        {
            Given(
                     new FactoryOpened(FactoryId.ForTest),
                     new EmployeeAssignedToFactory(FactoryId.ForTest, "fry")
                );
            When(new ProduceACar(FactoryId.ForTest, "fry", "Volvo"));
            Expect("car-model-not-found");
        }

        [Test]
        public void car_produced_announcment_received()
        {
            Given(
                    Library.RecordBlueprint("death star", new CarPart("magic box", 10)),
                    Library.RecordBlueprint("Ford", new CarPart("chassis", 1), new CarPart("wheels", 4), new CarPart("engine", 1)),
                    new FactoryOpened(FactoryId.ForTest),
                    new EmployeeAssignedToFactory(FactoryId.ForTest, "fry"),
                    new ShipmentUnpackedInCargoBay(FactoryId.ForTest, "fry", new[] { new InventoryShipment("ship-1", new[] { new CarPart("chassis", 1), new CarPart("wheels", 4), new CarPart("engine", 1) }) })
                );

            When(new ProduceACar(FactoryId.ForTest, "fry", "Ford"));

            Expect(new CarProduced(FactoryId.ForTest,"fry", "Ford", new[] { new CarPart("chassis", 1), new CarPart("wheels", 4), new CarPart("engine", 1) }));
        }


        [Test]
        public void an_employee_who_has_already_produced_a_car_today_cant_be_assigned()
        {
            Given(
                    Library.RecordBlueprint("Ford", new CarPart("chassis", 1), new CarPart("wheels", 4), new CarPart("engine", 1)),
                    new FactoryOpened(FactoryId.ForTest),
                    new EmployeeAssignedToFactory(FactoryId.ForTest, "fry"),
                    new ShipmentUnpackedInCargoBay(FactoryId.ForTest, "fry", new[] { new InventoryShipment("ship-1", new[] { new CarPart("chassis", 1), new CarPart("wheels", 4), new CarPart("engine", 1) }) }),
                    new CarProduced(FactoryId.ForTest, "fry", "Ford", new[] { new CarPart("chassis", 1), new CarPart("wheels", 4), new CarPart("engine", 1) }),
                    new ShipmentUnpackedInCargoBay(FactoryId.ForTest, "fry", new[] { new InventoryShipment("ship-2", new[] { new CarPart("chassis", 1), new CarPart("wheels", 4), new CarPart("engine", 1) }) })
                );

            When(new ProduceACar(FactoryId.ForTest, "fry", "Ford"));

            Expect("employee-already-produced-car-today");
        }


        [Test]
        public void when_factory_not_open_is_an_error()
        {
            When(new ProduceACar(FactoryId.ForTest, "fry", "Ford"));
            Expect("factory-is-not-open");
        }
    }
}
