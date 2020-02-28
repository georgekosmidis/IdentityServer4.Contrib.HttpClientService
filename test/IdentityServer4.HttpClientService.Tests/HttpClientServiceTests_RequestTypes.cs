using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityServer4.HttpClientService.Models;
using Microsoft.Extensions.Options;
using IdentityServer4.HttpClientService;
using IdentityServer4.HttpClientService.Tests.Helpers;
using IdentityServer4.HttpClientService.Infrastructure;
using System.Net;
using IdentityModel.Client;
using System;
using System.IO;
using System.Text;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace IdentityServer4.HttpClientService.Test
{
    [TestClass]
    public class HttpClientServiceTests_RequestTypes
    {
        private class ComplexRequestTypeTest
        {
            public int TestInt { get; set; } = 1;
            public bool TestBool { get; set; } = true;
        }

        [TestMethod]
        public async Task HttpClientServiceTests_ComplexRequestType()
        {

            var httpClientService = new HttpClientServiceFactory(
                IConfigurationMocks.Get("section_data"),
                IHttpClientFactoryMocks.Get(HttpStatusCode.Created, "body_of_response"),
                new HttpRequestMessageFactory(
                    IHttpContextAccessorMocks.Get()
                ),
                new TokenResponseService(
                    IHttpClientFactoryMocks.Get(HttpStatusCode.OK),
                    IAccessTokenCacheManagerMocks.Get(
                        await TokenResponseMock.GetValidResponseAsync("access_token", 3600)
                    )
                )
            ).CreateHttpClientService();

            var result = await httpClientService.SendAsync<string, ComplexRequestTypeTest>(
                    new Uri("http://localhost"),
                    HttpMethod.Post,
                    new ComplexRequestTypeTest()
                    {
                        TestInt = 1,
                        TestBool = true
                    }
                );

            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);

            var requestBody = await result.HttpRequestMessge.Content.ReadAsStringAsync();
            Assert.AreEqual("application/json", result.HttpRequestMessge.Content.Headers.ContentType.MediaType);
            Assert.AreEqual("utf-8", result.HttpRequestMessge.Content.Headers.ContentType.CharSet);
            Assert.AreEqual("{\"TestInt\":1,\"TestBool\":true}", requestBody);
        }

        [TestMethod]
        public async Task HttpClientServiceTests_PrimitiveRequestType()
        {

            var httpClientService = new HttpClientServiceFactory(
                IConfigurationMocks.Get("section_data"),
                IHttpClientFactoryMocks.Get(HttpStatusCode.Created, "body_of_response"),
                new HttpRequestMessageFactory(
                    IHttpContextAccessorMocks.Get()
                ),
                new TokenResponseService(
                    IHttpClientFactoryMocks.Get(HttpStatusCode.OK),
                    IAccessTokenCacheManagerMocks.Get(
                        await TokenResponseMock.GetValidResponseAsync("access_token", 3600)
                    )
                )
            ).CreateHttpClientService();

            var result = await httpClientService.SendAsync<string, int>(
                    new Uri("http://localhost"),
                    HttpMethod.Post,
                    123
                );

            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);

            var requestBody = await result.HttpRequestMessge.Content.ReadAsStringAsync();
            Assert.AreEqual("text/plain", result.HttpRequestMessge.Content.Headers.ContentType.MediaType);
            Assert.AreEqual("utf-8", result.HttpRequestMessge.Content.Headers.ContentType.CharSet);
            Assert.AreEqual("123", requestBody);
        }

        [TestMethod]
        public async Task HttpClientServiceTests_StringContentRequestType()
        {

            var httpClientService = new HttpClientServiceFactory(
                IConfigurationMocks.Get("section_data"),
                IHttpClientFactoryMocks.Get(HttpStatusCode.Created, "body_of_response"),
                new HttpRequestMessageFactory(
                    IHttpContextAccessorMocks.Get()
                ),
                new TokenResponseService(
                    IHttpClientFactoryMocks.Get(HttpStatusCode.OK),
                    IAccessTokenCacheManagerMocks.Get(
                        await TokenResponseMock.GetValidResponseAsync("access_token", 3600)
                    )
                )
            ).CreateHttpClientService();

            var result = await httpClientService.SendAsync<string, StringContent>(
                    new Uri("http://localhost"),
                    HttpMethod.Post,
                    new StringContent("request_body", Encoding.UTF32, "some/type")
                );            

            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);

            var requestBody = await result.HttpRequestMessge.Content.ReadAsStringAsync();
            Assert.AreEqual("some/type", result.HttpRequestMessge.Content.Headers.ContentType.MediaType);
            Assert.AreEqual("utf-32", result.HttpRequestMessge.Content.Headers.ContentType.CharSet);
            Assert.AreEqual("request_body", requestBody);

            httpClientService.Dispose();
        }

        [TestMethod]
        public async Task HttpClientServiceTests_StreamContentRequestType()
        {

            var httpClientService = new HttpClientServiceFactory(
                IConfigurationMocks.Get("section_data"),
                IHttpClientFactoryMocks.Get(HttpStatusCode.Created, "body_of_response"),
                new HttpRequestMessageFactory(
                    IHttpContextAccessorMocks.Get()
                ),
                new TokenResponseService(
                    IHttpClientFactoryMocks.Get(HttpStatusCode.OK),
                    IAccessTokenCacheManagerMocks.Get(
                        await TokenResponseMock.GetValidResponseAsync("access_token", 3600)
                    )
                )
            ).CreateHttpClientService();

            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream))
            {
                writer.Write("test_body_as_stream");
                writer.Flush();
                memoryStream.Position = 0;

                var result = await httpClientService.SendAsync<string, StreamContent>(
                    new Uri("http://localhost"),
                    HttpMethod.Post,
                    new StreamContent(memoryStream)
                );

                Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);

                var requestBody = await result.HttpRequestMessge.Content.ReadAsStringAsync();
                Assert.AreEqual("test_body_as_stream", requestBody);

                httpClientService.Dispose();
            }
        }
    }
}
