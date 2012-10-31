using E014.Contracts;
using NUnit.Framework;
// ReSharper disable InconsistentNaming
namespace E014.Domain.ApplicationServices.Factory
{
    public class open_factory : factory_application_service_spec
    {
        [Test]
        public void open_factory_correclty_with_factory_id()
        {
            When(new OpenFactory(FactoryId.ForTest));
            Expect(new FactoryOpened(FactoryId.ForTest));
        }

        [Test]
        public void attempt_to_open_more_than_once_is_not_allowed()
        {
            Given(new FactoryOpened(FactoryId.ForTest));
            When(new OpenFactory(FactoryId.ForTest));
            Expect("factory-already-created");
        }
    }
}
