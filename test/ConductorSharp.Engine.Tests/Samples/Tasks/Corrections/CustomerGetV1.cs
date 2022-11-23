namespace ConductorSharp.Engine.Tests.Samples.Tasks
{
    public partial class CustomerGetV1Output
    {
        [TypedProperty(nameof(Address))]
        public string AddressString { get; set; }

        [TypedProperty(nameof(Name))]
        public string NameString { get; set; }
    }
}
