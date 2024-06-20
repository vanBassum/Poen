using InfluxDB.Client;
using InfluxDB.Client.Api.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;



namespace Tracker.Services
{
    public class InfluxDBService
    {
        private readonly InfluxDb _settings;

        public InfluxDBService(IOptions<InfluxDb> settings)
        {
            _settings = settings.Value;
        }

        public void Write(Action<InfluxWriteDetails> action)
        {
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





