using InfluxDB.Client;
using Microsoft.Extensions.Options;
using Poen.Config;
using System.Runtime.CompilerServices;



namespace Poen.Services
{
    public class InfluxDBService
    {
        private readonly InfluxDbConfig _settings;

        public InfluxDBService(IOptions<InfluxDbConfig> settings)
        {
            _settings = settings.Value;
        }

        public void Write(Action<InfluxWriteDetails> action)
        {
            if (string.IsNullOrEmpty(_settings.ApiKey))
                throw new Exception("InfluxDb ApiKey not set");

            if (string.IsNullOrEmpty(_settings.Bucket))
                throw new Exception("InfluxDb Bucket not set");

            if (string.IsNullOrEmpty(_settings.Endpoint))
                throw new Exception("InfluxDb Endpoint not set");

            if (string.IsNullOrEmpty(_settings.Organisation))
                throw new Exception("InfluxDb Organisation not set");

            using var client = InfluxDBClientFactory.Create(_settings.Endpoint, _settings.ApiKey);

            InfluxWriteDetails details = new InfluxWriteDetails
            {
                Api = client.GetWriteApi(),
                Bucket = _settings.Bucket,
                Organisation = _settings.Organisation,
            };
            action(details);
        }

        //public async Task<T> QueryAsync<T>(Func<QueryApi, Task<T>> action)
        //{
        //    using var client = InfluxDBClientFactory.Create("http://localhost:8086", _token);
        //    var query = client.GetQueryApi();
        //    return await action(query);
        //}
    }

    public class InfluxWriteDetails
    {
        public required WriteApi Api;
        public required string Bucket { get; set; }
        public required string Organisation { get; set; }
    }
}





