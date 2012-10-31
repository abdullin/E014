using E014.Contracts;
using NUnit.Framework;

namespace E014.Domain.ApplicationServices.Factory
{
// ReSharper disable InconsistentNaming
    public class unpack_shipment_in_cargo_bay : factory_application_service_spec
    {
        [Test]
        public void an_unpacked_announcement_is_made_with_correct_inventory_list()
        {
            Given(new FactoryOpened(Id),
                            new EmployeeAssignedToFactory(Id, "fry"),
                            new ShipmentReceivedInCargoBay(Id, new InventoryShipment("ship-1", new[] { new CarPart("chassis", 1), })));

            When(new UnpackAndInventoryShipmentInCargoBay(Id, "fry"));
            Expect(new ShipmentUnpackedInCargoBay(Id, "fry", new[] { new InventoryShipment("ship-1", new[] { new CarPart("chassis", 1), }) }));
        }

        [Test]
        public void assigning_employee_not_in_factory_is_an_error()
        {
            Given(new FactoryOpened(Id),
                  new EmployeeAssignedToFactory(Id, "ben"),
                  new ShipmentReceivedInCargoBay(Id, new InventoryShipment("ship-1", new[] { new CarPart("chassis", 1), })));
            When(new UnpackAndInventoryShipmentInCargoBay(Id, "fry"));
            Expect("unknown-employee");
        }

        static readonly FactoryId Id = FactoryId.ForTest;

     

        [Test]
        public void an_employee_asked_to_unpack_more_than_once_a_day_is_not_allowed()
        {
            var shipment = new InventoryShipment("ship-1", new[] {new CarPart("chassis", 1),});

            Given(new FactoryOpened(Id),
                  new EmployeeAssignedToFactory(Id, "fry"),
                  new ShipmentReceivedInCargoBay(Id, shipment),
                  new ShipmentUnpackedInCargoBay(Id, "fry", new[] { shipment, }));
            When(new UnpackAndInventoryShipmentInCargoBay(Id, "fry"));
            Expect("employee-already-unpacked-cargo");
        }

        [Test]
        public void it_is_an_error_if_there_are_no_shipments_to_unpack()
        {
            Given(new FactoryOpened(Id),
                  new EmployeeAssignedToFactory(Id, "fry"));
            When(new UnpackAndInventoryShipmentInCargoBay(Id, "fry"));
            Expect("empty-InventoryShipments");
        }

        [Test]
        public void when_factory_not_open_is_an_error()
        {
            When(new UnpackAndInventoryShipmentInCargoBay(Id, "fry"));
            Expect("factory-is-not-open");
        }
    }
}
