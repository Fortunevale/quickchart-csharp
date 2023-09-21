using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.IO;

namespace QuickChart
{
    internal class QuickChartShortUrlResponse
    {
#pragma warning disable IDE1006 // Naming Styles
        public bool status { get; set; }
        public string url { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }

    public class Chart
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public double DevicePixelRatio { get; set; }
        public string Format { get; set; }
        public string BackgroundColor { get; set; }
        public string Key { get; set; }
        public string Version { get; set; }
        public string Config { get; set; }

        public string Scheme { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }

        private static readonly HttpClient Client = new HttpClient();

        public Chart(string scheme = null, string host = null, int? port = null)
        {
            this.Width = 500;
            this.Height = 300;
            this.DevicePixelRatio = 1.0;
            this.Format = "png";
            this.BackgroundColor = "transparent";

            if (host != null)
            {
                this.Host = host;
                if (scheme != null)
                {
                    this.Scheme = scheme;
                    this.Port = port ?? (scheme == "http" ? 80 : 443);
                }
                else
                {
                    this.Scheme = "https";
                    this.Port = 443;
                }
            }
            else
            {
                this.Scheme = "https";
                this.Host = "quickchart.io";
                this.Port = 443;
            }
        }

        public string GetUrl()
        {
            if (this.Config == null)
            {
                throw new NullReferenceException("You must set Config on the QuickChart object before generating a URL");
            }

            var builder = new StringBuilder();
            _ = builder.Append("w=").Append(this.Width.ToString());
            _ = builder.Append("&h=").Append(this.Height.ToString());

            _ = builder.Append("&devicePixelRatio=").Append(this.DevicePixelRatio.ToString());
            _ = builder.Append("&f=").Append(this.Format);
            _ = builder.Append("&bkg=").Append(Uri.EscapeDataString(this.BackgroundColor));
            _ = builder.Append("&c=").Append(Uri.EscapeDataString(this.Config));
            if (!string.IsNullOrEmpty(this.Key))
            {
                _ = builder.Append("&key=").Append(Uri.EscapeDataString(this.Key));
            }
            if (!string.IsNullOrEmpty(this.Version))
            {
                _ = builder.Append("&v=").Append(Uri.EscapeDataString(this.Version));
            }

            return $"{this.Scheme}://{this.Host}:{this.Port}/chart?{builder}";
        }

        public string GetShortUrl()
        {
            if (this.Config == null)
            {
                throw new NullReferenceException("You must set Config on the QuickChart object before generating a URL");
            }

            var options = new JsonSerializerOptions
            {
                IgnoreNullValues = true
            };

            var json = JsonSerializer.Serialize(new
            {
                width = this.Width,
                height = this.Height,
                backgroundColor = this.BackgroundColor,
                devicePixelRatio = this.DevicePixelRatio,
                format = this.Format,
                chart = this.Config,
                key = this.Key,
                version = this.Version,
            }, options);

            var url = $"{this.Scheme}://{this.Host}:{this.Port}/chart/create";

            var response = Client.PostAsync(
                url,
                new StringContent(json, Encoding.UTF8, "application/json")
            ).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException("Unsuccessful response from API", response);
            }

            var responseText = response.Content.ReadAsStringAsync().Result;
            var result = JsonSerializer.Deserialize<QuickChartShortUrlResponse>(responseText);
            return result.url;
        }

        public byte[] ToByteArray()
        {
            if (this.Config == null)
            {
                throw new NullReferenceException("You must set Config on the QuickChart object before generating a URL");
            }

            var options = new JsonSerializerOptions
            {
                IgnoreNullValues = true
            };

            var json = JsonSerializer.Serialize(new
            {
                width = this.Width,
                height = this.Height,
                backgroundColor = this.BackgroundColor,
                devicePixelRatio = this.DevicePixelRatio,
                format = this.Format,
                chart = this.Config,
                key = this.Key,
                version = this.Version,
            }, options);

            var url = $"{this.Scheme}://{this.Host}:{this.Port}/chart";

            var response = Client.PostAsync(
                url,
                new StringContent(json, Encoding.UTF8, "application/json")
            ).Result;

            return !response.IsSuccessStatusCode
                ? throw new ApiException("Unsuccessful response from API", response)
                : response.Content.ReadAsByteArrayAsync().Result;
        }

        public void ToFile(string filePath) 
            => File.WriteAllBytes(filePath, this.ToByteArray());
    }

    public class ApiException : Exception
    {
        public ApiException(string message, HttpResponseMessage response) : base(message) 
            => this.Response = response;

        public HttpResponseMessage Response { get; private set; }
    }
}
