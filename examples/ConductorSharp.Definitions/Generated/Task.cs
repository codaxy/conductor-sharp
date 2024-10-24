using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using MediatR;
using Newtonsoft.Json;

namespace ConductorSharp.Definitions.Generated
{
    public partial class EmailPrepareV1Input : IRequest<EmailPrepareV1Output>
    {
        /// <originalName>
        /// address
        /// </originalName>
        public object Address { get; set; }

        /// <originalName>
        /// name
        /// </originalName>
        public object Name { get; set; }
    }

    public partial class EmailPrepareV1Output
    {
        /// <originalName>
        /// email_body
        /// </originalName>
        public object EmailBody { get; set; }
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
        public object CustomerId { get; set; }
    }

    public partial class CustomerGetV1Output
    {
        /// <originalName>
        /// name
        /// </originalName>
        public object Name { get; set; }

        /// <originalName>
        /// address
        /// </originalName>
        public object Address { get; set; }
    }

    /// <originalName>
    /// CUSTOMER_get
    /// </originalName>
    ///
    [OriginalName("CUSTOMER_get")]
    public partial class CustomerGetV1 : SimpleTaskModel<CustomerGetV1Input, CustomerGetV1Output> { }

    public partial class EnumTaskInput : IRequest<EnumTaskOutput>
    {
        /// <originalName>
        /// status
        /// </originalName>
        [JsonProperty("status")]
        public object Status { get; set; }
    }

    public partial class EnumTaskOutput
    {
        /// <originalName>
        /// status
        /// </originalName>
        [JsonProperty("status")]
        public object Status { get; set; }
    }

    /// <originalName>
    /// ENUM_task
    /// </originalName>
    /// <ownerEmail>
    /// undefined@undefined.local
    /// </ownerEmail>
    /// <node>
    ///
    /// </node>
    /// <summary>
    ///
    /// </summary>
    [OriginalName("ENUM_task")]
    public partial class EnumTask : SimpleTaskModel<EnumTaskInput, EnumTaskOutput> { }
}
