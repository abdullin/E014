using E014.Contracts;
using NUnit.Framework;

namespace E014.Domain.ApplicationServices.Factory
{
    public class assign_employee_to_factory : factory_application_service_spec
    {
        [Test]
        public void correct_assign_employee()
        {
            Given(new FactoryOpened(FactoryId.ForTest));
            When(new AssignEmployeeToFactory(FactoryId.ForTest,"fry"));
            Expect(new EmployeeAssignedToFactory(FactoryId.ForTest, "fry"));
        }
        [Test]
        public void fry_is_assigned_to_factory()
        {
            Given(new FactoryOpened(FactoryId.ForTest),
                    new EmployeeAssignedToFactory(FactoryId.ForTest,"fry"));
            When(new AssignEmployeeToFactory(FactoryId.ForTest, "fry"));
            Expect("more than 1 person");
        }
        [Test]
        public void bender_comes_to_empty_factory()
        {
            Given(new FactoryOpened(FactoryId.ForTest));
            When(new AssignEmployeeToFactory(FactoryId.ForTest, "bender"));
            Expect("bender-employee");
        }

        [Test]
        public void factory_not_open()
        {
            When(new AssignEmployeeToFactory(FactoryId.ForTest, "fry"));
            Expect("factory-is-not-open");
        }
    }
}
