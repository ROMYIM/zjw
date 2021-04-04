using System;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Yiqi.Infrastructure.RemoteServices.Options;
using Yiqi.Models;

namespace Yiqi.Infrastructure.RemoteServices
{
    public class GitjtService
    {
        private readonly long _requestTime;

        private IOptionsMonitor<GitjtOptions> _optionsMonitor;

        public HttpClient Client { get; }

        private GitjtOptions Options => _optionsMonitor.CurrentValue;

        public GitjtService(HttpClient client, IOptionsMonitor<GitjtOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;

            _requestTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            client.BaseAddress = new System.Uri(Options.Host);
            Client = client;
        }

        protected virtual async ValueTask<IEnumerable<KeyValuePair<string, object>>> BuildRequestParametersAsync(PageParameters pageParameters)
        {
            var requestParameters = new SortedDictionary<string, object>();
            requestParameters.Add("reqtime", _requestTime);
            requestParameters.Add("appid", Options.AppId);
            requestParameters.Add("data", JsonSerializer.Serialize(pageParameters));

            var sign = await GenerateSignAsync(requestParameters);
            requestParameters.Add("sign", sign);
            return requestParameters;
        }

        protected virtual async ValueTask<string> GenerateSignAsync(IEnumerable<KeyValuePair<string, object>> sortedParameters)
        {
            var signBuilder = new StringBuilder();
            signBuilder.Append(Options.Password);
            foreach (var parameter in sortedParameters)
            {
                signBuilder.Append(parameter.Key).Append(parameter.Value);
            }
            signBuilder.Append(Options.Password);

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(signBuilder.ToString()));
            var sha1 = SHA1.Create();
            var signBytes = await sha1.ComputeHashAsync(stream).ConfigureAwait(false);
            return Encoding.UTF8.GetString(signBytes);
        }
    }
}