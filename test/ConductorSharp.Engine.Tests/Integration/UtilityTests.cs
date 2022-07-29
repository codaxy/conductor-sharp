using ConductorSharp.Engine.Tests.Samples.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Integration
{
    public class UtilityTests
    {
        [Fact]
        public void NamingUtilShouldReturnCorrectNameableObjectName()
        {
            Assert.Equal("TEST_StringInterpolation", NamingUtil.NameOf<StringInterpolation>());
            Assert.Equal("CUSTOMER_get", NamingUtil.NameOf<CustomerGetV1>());
        }

        [Fact]
        public void NamingUtilShouldReturnCorrectTaskReferenceName() =>
            Assert.Equal("get_customer", NamingUtil.NameOf<SendCustomerNotification>(wf => wf.GetCustomer));
    }
}
