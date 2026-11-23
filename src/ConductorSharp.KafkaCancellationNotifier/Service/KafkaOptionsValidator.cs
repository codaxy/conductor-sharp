using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace ConductorSharp.KafkaCancellationNotifier.Service
{
    internal class KafkaOptionsValidator : IValidateOptions<KafkaOptions>
    {
        private static readonly SaslMechanism[] SupportedSaslMechanisms =
        {
            SaslMechanism.Plain,
            SaslMechanism.ScramSha256,
            SaslMechanism.ScramSha512
        };

        public ValidateOptionsResult Validate(string? name, KafkaOptions options)
        {
            var errors = new List<string>();

            RequireValue(errors, options.BootstrapServers, nameof(KafkaOptions.BootstrapServers));
            RequireValue(errors, options.TopicName, nameof(KafkaOptions.TopicName));
            RequireValue(errors, options.GroupId, nameof(KafkaOptions.GroupId));

            if (options.IsSaslProtocol)
                ValidateSasl(errors, options);

            if (options.IsSslProtocol)
                ValidateSsl(errors, options);

            return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
        }

        private static void ValidateSasl(List<string> errors, KafkaOptions options)
        {
            if (options.SaslMechanism is null)
                errors.Add(
                    $"{nameof(KafkaOptions.SaslMechanism)} is required when {nameof(KafkaOptions.SecurityProtocol)} is {options.SecurityProtocol}"
                );
            else if (!SupportedSaslMechanisms.Contains(options.SaslMechanism.Value))
                errors.Add(
                    $"{nameof(KafkaOptions.SaslMechanism)} {options.SaslMechanism} is not supported, use one of: {string.Join(", ", SupportedSaslMechanisms)}"
                );

            RequireValue(errors, options.SaslUsername, nameof(KafkaOptions.SaslUsername));
            RequireValue(errors, options.SaslPassword, nameof(KafkaOptions.SaslPassword));
        }

        private static void ValidateSsl(List<string> errors, KafkaOptions options)
        {
            if ((options.SslCertificateLocation is null) != (options.SslKeyLocation is null))
                errors.Add($"{nameof(KafkaOptions.SslCertificateLocation)} and {nameof(KafkaOptions.SslKeyLocation)} must be configured together");

            if (options.SslKeystoreLocation is not null && (options.SslCertificateLocation is not null || options.SslKeyLocation is not null))
                errors.Add(
                    $"{nameof(KafkaOptions.SslKeystoreLocation)} cannot be combined with {nameof(KafkaOptions.SslCertificateLocation)} or {nameof(KafkaOptions.SslKeyLocation)}"
                );

            RequireExistingFile(errors, options.SslCaLocation, nameof(KafkaOptions.SslCaLocation));
            RequireExistingFile(errors, options.SslCertificateLocation, nameof(KafkaOptions.SslCertificateLocation));
            RequireExistingFile(errors, options.SslKeyLocation, nameof(KafkaOptions.SslKeyLocation));
            RequireExistingFile(errors, options.SslKeystoreLocation, nameof(KafkaOptions.SslKeystoreLocation));
        }

        private static void RequireValue(List<string> errors, string? value, string settingName)
        {
            if (string.IsNullOrWhiteSpace(value))
                errors.Add($"{settingName} is required");
        }

        private static void RequireExistingFile(List<string> errors, string? path, string settingName)
        {
            if (path is not null && !File.Exists(path))
                errors.Add($"{settingName} file '{path}' does not exist");
        }
    }
}
