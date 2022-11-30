namespace ConductorSharp.Engine.Tests.Samples.Tasks
{
    public class FullName
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public partial class CustomerGetV1Output
    {
        [TypedProperty(nameof(Address))]
        public string AddressString { get; set; }

        [TypedProperty(nameof(Name))]
        public FullName FullName { get; set; }
    }
}
