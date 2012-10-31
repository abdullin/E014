using E014.Contracts;
using NUnit.Framework;
// ReSharper disable InconsistentNaming
namespace E014.Domain.ApplicationServices.Factory
{
    public class receive_shipment_in_cargo_bay : factory_application_service_spec
    {
        [Test]
        public void a_shipment_received_announcement_is_made_with_correct_car_parts_list()
        {
            Given(new FactoryOpened(FactoryId.ForTest), 
                    new EmployeeAssignedToFactory(FactoryId.ForTest, "yoda"));
            When(new ReceiveShipmentInCargoBay(FactoryId.ForTest, "shipment-777", new[] { new CarPart("engine", 1) }));
            Expect(new ShipmentReceivedInCargoBay(FactoryId.ForTest, new InventoryShipment("shipment-777", new[] { new CarPart("engine", 1) })));
        }


        [Test]
        public void empty_shipment_is_not_allowed()
        {
            Given(new FactoryOpened(FactoryId.ForTest),
                            new EmployeeAssignedToFactory(FactoryId.ForTest, "yoda"));
            When(new ReceiveShipmentInCargoBay(FactoryId.ForTest, "some shipment", new CarPart[0]));
            Expect("empty-InventoryShipments");
        }

        [Test]
        public void an_empty_shipment_that_comes_to_factory_with_no_employees_is_not_received()
        {
            Given(new FactoryOpened(FactoryId.ForTest));
            When(new ReceiveShipmentInCargoBay(FactoryId.ForTest, "some shipment", new[] { new CarPart("chassis", 1) }));
            Expect("unknown-employee");
        }
        [Test]
        public void there_are_already_two_shipments_in_cargo_bay_so_no_new_shipments_allowed()
        {
            Given(
                new FactoryOpened(FactoryId.ForTest),
                    new EmployeeAssignedToFactory(FactoryId.ForTest, "chubakka"),
                    new ShipmentReceivedInCargoBay(FactoryId.ForTest, new InventoryShipment("shipmt-11", new[] { new CarPart("engine", 3) })),
                    new ShipmentReceivedInCargoBay(FactoryId.ForTest, new InventoryShipment("shipmt-12", new[] { new CarPart("wheels", 40) }))
                );

            When(new ReceiveShipmentInCargoBay(FactoryId.ForTest, "shipmt-13", new[] { new CarPart("bmw6", 20) }));
            Expect("more-than-two-InventoryShipments");
        }

        [Test]
        public void when_factory_not_open_is_an_error()
        {
            When(new ReceiveShipmentInCargoBay(FactoryId.ForTest, "some shipment", new CarPart[0]));
            Expect("factory-is-not-open");
        }
    }
}
