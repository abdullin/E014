using E014.Contracts;
using NUnit.Framework;

namespace E014.Domain.ApplicationServices.Factory
{
    public class assign_employee_to_factory : factory_application_service_spec
    {
        [Test]
        public void empty_factory_allows_any_employee_not_bender_to_be_assigned()
        {
            Given(new FactoryOpened(FactoryId.ForTest));
            When(new AssignEmployeeToFactory(FactoryId.ForTest, "fry"));
            Expect(new EmployeeAssignedToFactory(FactoryId.ForTest, "fry"));
        }

        [Test]
        public void duplicate_employee_name_is_assigned_but_not_allowed()
        {
            Given(new FactoryOpened(FactoryId.ForTest),
                    new EmployeeAssignedToFactory(FactoryId.ForTest,"fry"));
            When(new AssignEmployeeToFactory(FactoryId.ForTest, "fry"));
            Expect("employee-name-already-taken");
        }
        [Test]
        public void no_employee_named_bender_is_allowed_to_be_assigned()
        {
            Given(new FactoryOpened(FactoryId.ForTest));
            When(new AssignEmployeeToFactory(FactoryId.ForTest, "bender"));
            Expect("bender-employee");
        }

        [Test]
        public void cant_assign_employee_to_unopened_factory()
        {
            When(new AssignEmployeeToFactory(FactoryId.ForTest, "fry"));
            Expect("factory-is-not-open");
        }
    }
}
