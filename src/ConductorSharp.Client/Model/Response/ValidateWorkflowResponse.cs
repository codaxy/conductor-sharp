using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Client.Model.Response
{
    public class ValidateWorkflowResponse
    {
        public class ValidationErrors
        {
            public string Path { get; set; }
            public string Message { get; set; }
        }

        public int Status { get; set; }
        public string Message { get; set; }
        public bool Retryable { get; set; }

        public ValidationErrors[] ValidationErrorsResponse { get; set; }
    }
}
