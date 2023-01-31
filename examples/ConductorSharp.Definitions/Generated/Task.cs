using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using MediatR;

namespace ConductorSharp.Definitions.Generated
{
    public partial class EmailPrepareV1Input : IRequest<EmailPrepareV1Output>
    {
        /// <originalName>
        /// address
        /// </originalName>
        public dynamic Address { get; set; }

        /// <originalName>
        /// name
        /// </originalName>
        public dynamic Name { get; set; }
    }

    public partial class EmailPrepareV1Output
    {
        /// <originalName>
        /// email_body
        /// </originalName>
        public dynamic EmailBody { get; set; }
    }

    /// <originalName>
    /// EMAIL_prepare
    /// </originalName>
    [OriginalName("EMAIL_prepare")]
    public partial class EmailPrepareV1 : SimpleTaskModel<EmailPrepareV1Input, EmailPrepareV1Output> { }

    public partial class CustomerGetV1Input : IRequest<CustomerGetV1Output>
    {
        /// <originalName>
        /// customer_id
        /// </originalName>
        public dynamic CustomerId { get; set; }
    }

    public partial class CustomerGetV1Output
    {
        /// <originalName>
        /// name
        /// </originalName>
        public dynamic Name { get; set; }

        /// <originalName>
        /// address
        /// </originalName>
        public dynamic Address { get; set; }
    }

    /// <originalName>
    /// CUSTOMER_get
    /// </originalName>
    ///
    [OriginalName("CUSTOMER_get")]
    public partial class CustomerGetV1 : SimpleTaskModel<CustomerGetV1Input, CustomerGetV1Output> { }
}
