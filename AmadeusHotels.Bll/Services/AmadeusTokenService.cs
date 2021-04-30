using AmadeusHotels.Bll.Config;
using AmadeusHotels.Bll.Models.AmadeusApiCustomModels;
using AmadeusHotels.Bll.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AmadeusHotels.Bll.Services
{
    public class AmadeusTokenService : IAmadeusTokenService
    {
        private readonly IHttpClientFactory _clientFactory;
		private readonly IOptionsMonitor<AmadeusClientOptions> _amadeusClientOptions;
		private readonly ILogger<AmadeusTokenService> _logger;
		private static string _tokenString { get; set; } = "";
		private static DateTime _tokenExpiration { get; set; } = DateTime.MinValue;


		public AmadeusTokenService(IHttpClientFactory clientFactory, IOptionsMonitor<AmadeusClientOptions> amadeusClientOptions, ILogger<AmadeusTokenService> logger)
        {
            this._clientFactory = clientFactory;
			this._amadeusClientOptions = amadeusClientOptions;
			this._logger = logger;
        }


		public async Task<string> getAmadeusToken(CancellationToken cancellationToken)
		{
			if (String.IsNullOrEmpty(_tokenString) || _tokenExpiration < DateTime.Now)
			{
				var tokenGenerated = await this.generateToken(cancellationToken);

				if (!tokenGenerated)
				{
					return null;
				}
			}

			return _tokenString;
		}

		private async Task<bool> generateToken(CancellationToken cancellationToken)
		{
			var client = _clientFactory.CreateClient();

			string apiKey = _amadeusClientOptions.CurrentValue.ApiKey;
			string apiSecret = _amadeusClientOptions.CurrentValue.ApiSecret;

			if(String.IsNullOrEmpty(apiKey) || String.IsNullOrEmpty(apiSecret))
			{
				_logger.LogError("ApiKey i ApiSecret parameters for Amadesu service are not set");
				throw new ArgumentNullException("ApiKey i ApiSecret parameters for Amadesu service are not set");
			}

			client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");

			var paramsDict = new Dictionary<string, string>();
			paramsDict.Add("grant_type", "client_credentials");
			paramsDict.Add("client_id", apiKey);
			paramsDict.Add("client_secret", apiSecret);

			HttpResponseMessage response = await client.PostAsync(_amadeusClientOptions.CurrentValue.AuthTokenUrl, new FormUrlEncodedContent(paramsDict), cancellationToken);

			response.EnsureSuccessStatusCode();

			try
			{
				var contentStream = await response.Content.ReadAsStreamAsync();

				using var streamReader = new StreamReader(contentStream);
				using var jsonReader = new JsonTextReader(streamReader);

				JsonSerializer serializer = new JsonSerializer();

				var amadeusTokenResponse = serializer.Deserialize<AmadeusTokenResponse>(jsonReader);

				_tokenString = amadeusTokenResponse.Access_token;
				double secondsToExpire = amadeusTokenResponse.Expires_in;
				_tokenExpiration = DateTime.Now.AddSeconds(secondsToExpire);

				return true;
			}
			catch (Exception e)
			{
				throw new Exception("Could not parse JSON response for Amadeus token.", e);
			}

		}

	}
}
