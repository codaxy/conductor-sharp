using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Model
{
    public class ErrorOutput
    {
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Optional structured error classification. Null for plain (unclassified) failures, in which case it is
        /// omitted from serialized output (NullValueHandling.Ignore), preserving backward compatibility with
        /// consumers that only read <see cref="ErrorMessage"/>.
        /// </summary>
        public StructuredError StructuredError { get; set; }
    }
}
