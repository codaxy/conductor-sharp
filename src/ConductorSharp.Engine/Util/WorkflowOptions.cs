using System;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace ConductorSharp.Engine.Util
{

    public class WorkflowOptions
    {
        private readonly static string _labelRegexString = "[A-Z0-9]{1,25}";

        private readonly static Regex _labelRegex = new(_labelRegexString);

        private int _version;
        private string _description;
        private string[] _labels;
        private string _ownerApp;
        private string _ownerEmail;
        private Type _failureWorkflow;

        public int Version
        {
            get => _version;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Value must be greater than zero", nameof(Version));
                _version = value;
            }
        }
        public string Description
        {
            get => _description;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("Value must not be empty", nameof(Description));

                _description = value;
            }
        }
        public string[] Labels
        {
            get => _labels;
            set
            {
                if (value == null || value.Length == 0)
                    throw new ArgumentException("Value cannot be empty", nameof(Labels));

                if (value.Length > 10)
                    throw new ArgumentException("Cannot have more than 10 labels", nameof(Labels));

                if (value.Any(a => !_labelRegex.IsMatch(a)))
                    throw new ArgumentException($"Labels must match regex {_labelRegexString}");

                _labels = value;
            }
        }
        public string OwnerApp
        {
            get => _ownerApp;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("Value cannot be empty", nameof(OwnerApp));

                if (value.Length > 255)
                    throw new ArgumentException(
                        "Value cannot be longer than 255 characters",
                        nameof(OwnerApp)
                    );

                _ownerApp = value;
            }
        }
        public string OwnerEmail
        {
            get => _ownerEmail;
            set
            {
                try
                {
                    var parsedEmail = new MailAddress(value);
                    _ownerEmail = parsedEmail.Address;
                }
                catch (Exception)
                {
                    throw new ArgumentException(
                        "Value must be a valid email address",
                        nameof(OwnerEmail)
                    );
                }
            }
        }

        public Type FailureWorkflow
        {
            get => _failureWorkflow;
            set
            {
                // TODO: Add a check to make sure the type is actually a workflow model
                _failureWorkflow = value;
            }
        }
    }
}