using ConductorSharp.KafkaCancellationNotifier;
using ConductorSharp.KafkaCancellationNotifier.Service;
using Confluent.Kafka;

namespace ConductorSharp.Engine.Tests.Unit
{
    public class KafkaOptionsValidatorTests : IDisposable
    {
        private readonly KafkaOptionsValidator _validator = new();
        private readonly string _existingFile = Path.GetTempFileName();
        private readonly string _missingFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pem");

        public void Dispose() => File.Delete(_existingFile);

        private static KafkaOptions CreateOptions() =>
            new()
            {
                BootstrapServers = "broker:9093",
                TopicName = "conductor.status.task",
                GroupId = "group"
            };

        private KafkaOptions CreateSaslOptions(SecurityProtocol protocol)
        {
            var options = CreateOptions();
            options.SecurityProtocol = protocol;
            options.SaslMechanism = SaslMechanism.ScramSha256;
            options.SaslUsername = "user";
            options.SaslPassword = "password";
            return options;
        }

        private IEnumerable<string> Validate(KafkaOptions options) => _validator.Validate(null, options).Failures ?? Enumerable.Empty<string>();

        [Fact]
        public void PlaintextWithRequiredSettingsIsValid()
        {
            Assert.True(_validator.Validate(null, CreateOptions()).Succeeded);
        }

        [Theory]
        [InlineData(nameof(KafkaOptions.BootstrapServers))]
        [InlineData(nameof(KafkaOptions.TopicName))]
        [InlineData(nameof(KafkaOptions.GroupId))]
        public void MissingRequiredSettingFails(string settingName)
        {
            var options = CreateOptions();
            typeof(KafkaOptions).GetProperty(settingName)!.SetValue(options, " ");

            Assert.Contains($"{settingName} is required", Validate(options));
        }

        [Theory]
        [InlineData(SecurityProtocol.SaslPlaintext)]
        [InlineData(SecurityProtocol.SaslSsl)]
        public void SaslWithAllSettingsIsValid(SecurityProtocol protocol)
        {
            Assert.Empty(Validate(CreateSaslOptions(protocol)));
        }

        [Theory]
        [InlineData(SecurityProtocol.SaslPlaintext)]
        [InlineData(SecurityProtocol.SaslSsl)]
        public void SaslRequiresMechanismAndCredentials(SecurityProtocol protocol)
        {
            var options = CreateOptions();
            options.SecurityProtocol = protocol;

            var failures = Validate(options).ToList();

            Assert.Contains(failures, f => f.StartsWith(nameof(KafkaOptions.SaslMechanism)));
            Assert.Contains($"{nameof(KafkaOptions.SaslUsername)} is required", failures);
            Assert.Contains($"{nameof(KafkaOptions.SaslPassword)} is required", failures);
        }

        [Theory]
        [InlineData(SaslMechanism.Gssapi)]
        [InlineData(SaslMechanism.OAuthBearer)]
        public void UnsupportedSaslMechanismFails(SaslMechanism mechanism)
        {
            var options = CreateSaslOptions(SecurityProtocol.SaslSsl);
            options.SaslMechanism = mechanism;

            Assert.Contains(Validate(options), f => f.Contains($"{mechanism} is not supported"));
        }

        [Fact]
        public void SaslSettingsAreNotRequiredForSsl()
        {
            var options = CreateOptions();
            options.SecurityProtocol = SecurityProtocol.Ssl;

            Assert.Empty(Validate(options));
        }

        [Fact]
        public void MutualTlsWithExistingPemFilesIsValid()
        {
            var options = CreateOptions();
            options.SecurityProtocol = SecurityProtocol.Ssl;
            options.SslCaLocation = _existingFile;
            options.SslCertificateLocation = _existingFile;
            options.SslKeyLocation = _existingFile;

            Assert.Empty(Validate(options));
        }

        [Fact]
        public void MutualTlsWithExistingKeystoreIsValid()
        {
            var options = CreateOptions();
            options.SecurityProtocol = SecurityProtocol.SaslSsl;
            options.SaslMechanism = SaslMechanism.Plain;
            options.SaslUsername = "user";
            options.SaslPassword = "password";
            options.SslKeystoreLocation = _existingFile;

            Assert.Empty(Validate(options));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CertificateAndKeyMustBeConfiguredTogether(bool configureCertificate)
        {
            var options = CreateOptions();
            options.SecurityProtocol = SecurityProtocol.Ssl;
            if (configureCertificate)
                options.SslCertificateLocation = _existingFile;
            else
                options.SslKeyLocation = _existingFile;

            Assert.Contains(Validate(options), f => f.Contains("must be configured together"));
        }

        [Fact]
        public void KeystoreCannotBeCombinedWithCertificateAndKey()
        {
            var options = CreateOptions();
            options.SecurityProtocol = SecurityProtocol.Ssl;
            options.SslCertificateLocation = _existingFile;
            options.SslKeyLocation = _existingFile;
            options.SslKeystoreLocation = _existingFile;

            Assert.Contains(Validate(options), f => f.StartsWith($"{nameof(KafkaOptions.SslKeystoreLocation)} cannot be combined"));
        }

        [Theory]
        [InlineData(nameof(KafkaOptions.SslCaLocation))]
        [InlineData(nameof(KafkaOptions.SslCertificateLocation))]
        [InlineData(nameof(KafkaOptions.SslKeyLocation))]
        [InlineData(nameof(KafkaOptions.SslKeystoreLocation))]
        public void MissingFileFails(string settingName)
        {
            var options = CreateOptions();
            options.SecurityProtocol = SecurityProtocol.Ssl;
            typeof(KafkaOptions).GetProperty(settingName)!.SetValue(options, _missingFile);

            Assert.Contains($"{settingName} file '{_missingFile}' does not exist", Validate(options));
        }

        [Fact]
        public void SslSettingsAreNotValidatedWhenProtocolIsNotSsl()
        {
            var options = CreateOptions();
            options.SslCaLocation = _missingFile;
            options.SslCertificateLocation = _missingFile;

            Assert.Empty(Validate(options));
        }
    }
}
