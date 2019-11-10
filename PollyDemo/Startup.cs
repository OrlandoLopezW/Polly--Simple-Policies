using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;
using Polly.Timeout;
using Polly.Wrap;
using PollyDemo.Utility.Constants;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace PollyDemo
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            IPolicyRegistry<string> registry = services.AddPolicyRegistry();

            IAsyncPolicy<HttpResponseMessage> _httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .RetryAsync(3);
            registry.Add("SimpleHttpRetryPolicy", _httpRetryPolicy);

            IAsyncPolicy<HttpResponseMessage> httpWaitAndRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt));
            registry.Add("SimpleWaitAndRetryPolicy", httpWaitAndRetryPolicy);

            IAsyncPolicy<HttpResponseMessage> noOpPolicy = Policy.NoOpAsync().AsAsyncPolicy<HttpResponseMessage>();
            registry.Add("NoOpPolicy", noOpPolicy);

            services.AddHttpClient(PollyConstants.RemoteServer, client =>
            {
                client.BaseAddress = new Uri("http://localhost:58042/api/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }).AddPolicyHandlerFromRegistry(PolicySelector);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
            app.UseMvc();
        }


        #region Private Methods

        private IAsyncPolicy<HttpResponseMessage> PolicySelector(
            IReadOnlyPolicyRegistry<string> policyRegistry,
            HttpRequestMessage httpRequestMessage)
        {
            if (httpRequestMessage.Method == HttpMethod.Get)
            {
                return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("SimpleHttpRetryPolicy");
            }
            else if (httpRequestMessage.Method == HttpMethod.Post)
            {
                return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("NoOpPolicy");
            }
            else
            {
                return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("SimpleWaitAndRetryPolicy");
            }
        }

        #endregion
    }
}
