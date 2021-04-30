using AmadeusHotels.Bll.Models;
using AmadeusHotels.Bll.Models.AmadeusApiCustomModels;
using AmadeusHotels.Bll.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AmadeusHotels.Bll.Services
{
    public class AmadeusApiServiceProvider : IAmadeusApiServiceProvider
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<AmadeusApiServiceProvider> _logger;
        private readonly IAmadeusTokenService _amadeusTokenService;

        public AmadeusApiServiceProvider(HttpClient httpClient, ILogger<AmadeusApiServiceProvider> logger, IAmadeusTokenService amadeusTokenService)
        {
            this.httpClient = httpClient;
            this._amadeusTokenService = amadeusTokenService;
            this._logger = logger;
        }

        /// <summary>
        /// Fetch from Amadeus API, maximum nubmer of items Amadeus API returns in one request is 100 (we always query for this maximum number), so if more items are needed it fetches recursively.
        /// Method returns data from Amadues Search Hotels Api including all preceding data and data for requested page (+ surplus up to 100 from current request).
        /// </summary>
        /// <param name="hotelsSearchRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<HotelsSearchAmaduesFetchModel> FetchAmadeusHotels(HotelsSearchUserRequest hotelsSearchRequest, CancellationToken cancellationToken)
        {
            HotelsSearchAmaduesFetchModel amadeusFetchModel = new HotelsSearchAmaduesFetchModel();
            amadeusFetchModel.Items = new List<AmadeusApiHotelsSearchResponseItem>();

            string tokenString = await _amadeusTokenService.getAmadeusToken(cancellationToken);

            httpClient.DefaultRequestHeaders.Add("Accept", MediaTypeNames.Application.Json);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);

            // request to Amadeus Api is made in a way that Amadeus Api returns maximum possible number of items it can (it seems that limit is 100 items per request)
            var requestHotelsModel = new AmadeusApiHotelsSearchRequest(hotelsSearchRequest.CityCode, hotelsSearchRequest.CheckInDate, hotelsSearchRequest.CheckOutDate);

            // flag - if our user requsts certain page, all preceeding data should be fetched so they can be stored in db
            int minimumItemsToReturn = hotelsSearchRequest.PageSize * (hotelsSearchRequest.PageOffset + 1);
            int currentItemsReturnedCount;

            var urlParams = await requestHotelsModel.ToUrlParamsString();

            HttpResponseMessage response = await httpClient.GetAsync("/v2/shopping/hotel-offers" + "?" + urlParams, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errors = await ProccessError(response);
                var firstError = errors.Errors[0];
                throw new HttpRequestException(firstError.Code + " - " + firstError.Title);
            }

            response.EnsureSuccessStatusCode();

            var currentHotelsResponse = await ProccessResponse(response);
            _logger.LogInformation("Successful in first request from Amadeus API");

            amadeusFetchModel.Items.AddRange(currentHotelsResponse.Data);

            currentItemsReturnedCount = amadeusFetchModel.Items.Count;

            int iterationCount = 1;
            bool hasMoreItems = currentHotelsResponse.Meta != null && currentHotelsResponse.Meta.Links != null && !String.IsNullOrEmpty(currentHotelsResponse.Meta.Links.Next);
            string nextItemsLink = hasMoreItems ? currentHotelsResponse.Meta.Links.Next : null;

            while(currentItemsReturnedCount < minimumItemsToReturn && hasMoreItems)
            {
                nextItemsLink = null;
                hasMoreItems = currentHotelsResponse.Meta != null && currentHotelsResponse.Meta.Links != null && !String.IsNullOrEmpty(currentHotelsResponse.Meta.Links.Next);
                if (hasMoreItems)
                {
                    //nextAmadeusHotelsResponse = await FetchNextAmadeusHotels(currentHotelsResponse.Meta.Links.Next, cancellationToken);
                    nextItemsLink = currentHotelsResponse.Meta.Links.Next;

                    currentHotelsResponse = await FetchNextAmadeusHotels(currentHotelsResponse.Meta.Links.Next, cancellationToken);

                    _logger.LogInformation("Iteration count for getting next items: " + iterationCount);
                    iterationCount++;

                    amadeusFetchModel.Items.AddRange(currentHotelsResponse.Data);
                    
                }

                currentItemsReturnedCount = amadeusFetchModel.Items.Count;

            }

            _logger.LogInformation("Successful in getting data from Amadeus API. Returned Search Hotels items: " + currentItemsReturnedCount);
            amadeusFetchModel.nextItemsUrl = nextItemsLink;
            return amadeusFetchModel;
        }

        /// <summary>
        /// Returns next items from link that was stored in db for search request. Keeps getting recursively until at least "itemsToFetch" are fetched 
        /// </summary>
        /// <param name="uri">NextItemsLink that is stored in database for certain SearchRequest</param>
        /// <param name="itemsToFetch">keeps fetching from api, until at least this number of items is fetched</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<HotelsSearchAmaduesFetchModel> FetchNextAmadeusHotelsRecursively(string uri, int itemsToFetch, CancellationToken cancellationToken)
        {
            HotelsSearchAmaduesFetchModel amadeusFetchModel = new HotelsSearchAmaduesFetchModel();
            amadeusFetchModel.Items = new List<AmadeusApiHotelsSearchResponseItem>();

            var currentHotelsResponse = await FetchNextAmadeusHotels(uri, cancellationToken);
            _logger.LogInformation("Successful in first request from Amadeus API - (method FetchNextAmadeusHotelsRecursively)");

            amadeusFetchModel.Items.AddRange(currentHotelsResponse.Data);

            int currentItemsReturnedCount = amadeusFetchModel.Items.Count;

            int iterationCount = 1;
            bool hasMoreItems = currentHotelsResponse.Meta != null && currentHotelsResponse.Meta.Links != null && !String.IsNullOrEmpty(currentHotelsResponse.Meta.Links.Next);
            string nextItemsLink = hasMoreItems ? currentHotelsResponse.Meta.Links.Next : null;

            while (currentItemsReturnedCount < itemsToFetch && hasMoreItems)
            {
                nextItemsLink = null;
                hasMoreItems = currentHotelsResponse.Meta != null && currentHotelsResponse.Meta.Links != null && !String.IsNullOrEmpty(currentHotelsResponse.Meta.Links.Next);
                if (hasMoreItems)
                {
                    nextItemsLink = currentHotelsResponse.Meta.Links.Next;

                    currentHotelsResponse = await FetchNextAmadeusHotels(currentHotelsResponse.Meta.Links.Next, cancellationToken);

                    _logger.LogInformation("Iteration (method FetchNextAmadeusHotelsRecursively) count for getting next items: " + iterationCount);
                    iterationCount++;

                    amadeusFetchModel.Items.AddRange(currentHotelsResponse.Data);

                }

                currentItemsReturnedCount = amadeusFetchModel.Items.Count;

            }

            _logger.LogInformation("Successful in getting data from Amadeus API (method FetchNextAmadeusHotelsRecursively). Returned Search Hotels items: " + currentItemsReturnedCount);
            amadeusFetchModel.nextItemsUrl = nextItemsLink;
            return amadeusFetchModel;
        }

        private async Task<AmadeusApiHotelsSearchResponse> FetchNextAmadeusHotels(string uri, CancellationToken cancellationToken)
        {
            string tokenString = await _amadeusTokenService.getAmadeusToken(cancellationToken);

            httpClient.DefaultRequestHeaders.Add("Accept", MediaTypeNames.Application.Json);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);

            HttpResponseMessage response = await httpClient.GetAsync(uri, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errors = await ProccessError(response);
                var firstError = errors.Errors[0];
                throw new HttpRequestException(firstError.Code + " - " + firstError.Title);
            }

            response.EnsureSuccessStatusCode();

            var amadeusHotelsResponse = await ProccessResponse(response);

            return amadeusHotelsResponse;
        }

        public async Task<AmadeusApiHotelsSearchResponse> ProccessResponse(HttpResponseMessage response)
        {
            try
            {
                var contentStream = await response.Content.ReadAsStreamAsync();

                using var streamReader = new StreamReader(contentStream);
                using var jsonReader = new JsonTextReader(streamReader);

                JsonSerializer serializer = new JsonSerializer();

                var searchResponse = serializer.Deserialize<AmadeusApiHotelsSearchResponse>(jsonReader);

                _logger.LogInformation("Response from Amadeus api succesfull. Model fetchef: " + searchResponse.ToString());

                return searchResponse;
            }
            catch (Exception e)
            {
                throw new Exception("Could not parse JSON response for Amadeus hotels search.", e);
            }
        }

        private async Task<AmadeusApiErrorResponse> ProccessError(HttpResponseMessage response)
        {
            try
            {
                var contentStream = await response.Content.ReadAsStreamAsync();

                using var streamReader = new StreamReader(contentStream);
                using var jsonReader = new JsonTextReader(streamReader);

                JsonSerializer serializer = new JsonSerializer();

                var searchResponse = serializer.Deserialize<AmadeusApiErrorResponse>(jsonReader);

                _logger.LogInformation("Response from Amadeus api succesfull. Model fetchef: " + searchResponse.ToString());

                return searchResponse;
            }
            catch (Exception e)
            {
                throw new Exception("Could not parse JSON Error for Amadeus hotels search.", e);
            }
        }

        
    }
}
