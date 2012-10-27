using E014.Contracts;
using NUnit.Framework;

namespace E014.Domain.ApplicationServices.Factory
{
// ReSharper disable InconsistentNaming
    public class unload_shipment_from_cargo_bay : factory_application_service_spec
    {
        [Test]
        public void unloading_valid_shipment()
        {
            Given(new FactoryOpened(Id),
                            new EmployeeAssignedToFactory(Id, "fry"),
                            new ShipmentTransferredToCargoBay(Id, new InventoryShipment("ship-1", new[] { new CarPart("chassis", 1), })));

            When(new UnloadShipmentFromCargoBay(Id, "fry"));
            Expect(new UnloadedFromCargoBay(Id, "fry", new[] { new InventoryShipment("ship-1", new[] { new CarPart("chassis", 1), }) }));
        }

        [Test]
        public void fry_not_assigned_to_factory()
        {
            Given(new FactoryOpened(Id),
                  new EmployeeAssignedToFactory(Id, "ben"),
                  new ShipmentTransferredToCargoBay(Id, new InventoryShipment("ship-1", new[] { new CarPart("chassis", 1), })));
            When(new UnloadShipmentFromCargoBay(Id, "fry"));
            Expect("unknown-employee");
        }

        static readonly FactoryId Id = FactoryId.ForTest;

     

        [Test]
        public void shipment_already_unloaded()
        {
            var shipment = new InventoryShipment("ship-1", new[] {new CarPart("chassis", 1),});

            Given(new FactoryOpened(Id),
                  new EmployeeAssignedToFactory(Id, "fry"),
                  new ShipmentTransferredToCargoBay(Id, shipment),
                  new UnloadedFromCargoBay(Id, "fry", new[] { shipment, }));
            When(new UnloadShipmentFromCargoBay(Id, "fry"));
            Expect("empty-InventoryShipments");
        }

        [Test]
        public void no_shipment()
        {
            Given(new FactoryOpened(Id),
                  new EmployeeAssignedToFactory(Id, "fry"));
            When(new UnloadShipmentFromCargoBay(Id, "fry"));
            Expect("empty-InventoryShipments");
        }

        [Test]
        public void factory_not_open()
        {
            When(new UnloadShipmentFromCargoBay(Id, "fry"));
            Expect("factory-is-not-open");
        }
    }
}
