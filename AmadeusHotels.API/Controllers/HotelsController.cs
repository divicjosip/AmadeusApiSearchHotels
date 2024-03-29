﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AmadeusHotels.API.Helpers;
using AmadeusHotels.Bll.Models;
using AmadeusHotels.Bll.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AmadeusHotels.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private readonly ILogger<HotelsController> _logger;
        private readonly IHotelsSearchService _hotelsSearchService;
        private MemoryCache _cache;

        public HotelsController(ILogger<HotelsController> logger, IHotelsSearchService hotelsSearchService, MyMemoryCache memoryCache)
        {
            _logger = logger;
            _hotelsSearchService = hotelsSearchService;
            _cache = memoryCache.Cache;
        }

        /// <summary>
        ///  Returns data for given search request. Each request provides pagination parameters with limitation of 100 items per page.
        ///  This endpoint fetches requested data from Amadeus Api service and stores them in the database with certain expiration time.
        ///  For given combination of parameters for subsequent search requests with same combination of parameters, first chekcs cache memory,
        ///  after that checks if there are enough items in database and then returns them from database.
        ///  If there are some items in database, but not enough for given request, the rest is fetched recursively from stored NextItemsLink.
        ///  After stored data is expired (parameter in appsetings), new search request is stored in database and fetches fresh set of data from Amadeus API
        /// </summary>
        /// <param name="cityCode"><para>Destination City Code (or Airport Code). In case of city code, the search will be done around the city center.<br /> 
        /// Available codes can be found in <see href="https://www.iata.org/en/publications/directories/code-search/">IATA table codes</see> (3 chars IATA Code) <br /> 
        /// Example: PAR</para></param>
        /// <param name="checkInDate">check-in date of the stay (hotel local date). Format YYYY-MM-DD <br /> 
        /// The lowest accepted value is the present date (no dates in the past)</param>
        /// <param name="checkOutDate">check-out date of the stay (hotel local date). Format YYYY-MM-DD<br /> 
        /// The lowest accepted value is checkInDate+1</param>
        /// <param name="pageSize">Defines the number of items returned in response. Maximum value is 100</param>
        /// <param name="pageOffset">Defines the page offset</param>
        /// <param name="cancellationToken"></param>
        /// <returns>HotelsSearchResponse</returns>
        /// <response code="200">Model HotelsSearchResponse for requested page, with additional information about current page size and offset, and information if there is another page</response>
        /// <response code="400">Bad request with invalid parameters</response>
        /// <response code="500">Unexpected internal error</response>
        /// <response code="502">Problem retrieving Amadeus Hotels information from Amadeus API</response>
        [HttpGet]
        [Route("search")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(HotelsSearchResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(void))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<HotelsSearchResponse>> Search(string cityCode, DateTime checkInDate, DateTime checkOutDate, int pageSize, int pageOffset, CancellationToken cancellationToken)
        {
            try
            {
                HotelsSearchUserRequest hotelsSearchRequest = new HotelsSearchUserRequest(cityCode, checkInDate, checkOutDate, pageSize, pageOffset);
                ValidateAndSanitazeHotelsSearchRequest(hotelsSearchRequest);

                HotelsSearchResponse response;

                bool isCacheHit = _cache.TryGetValue(hotelsSearchRequest.ToCacheKey(), out response);

                if (!isCacheHit)
                {
                    _logger.LogInformation($"No cache hit. CityCode: {hotelsSearchRequest.CityCode}, " +
                          $"CheckIn: {hotelsSearchRequest.CheckInDate}, CheckOut: { hotelsSearchRequest.CheckOutDate}, pageSize: {hotelsSearchRequest.PageSize}, pageOffset: {hotelsSearchRequest.PageOffset}");

                    response = await _hotelsSearchService.SearchHotels(hotelsSearchRequest, cancellationToken);

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                     .SetSize(1)
                        .SetSlidingExpiration(TimeSpan.FromMinutes(2))
                        // Remove from cache after this time, regardless of sliding expiration
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                    _cache.Set(hotelsSearchRequest.ToCacheKey(), response, cacheEntryOptions); 
                }
                else
                {
                    _logger.LogInformation($"Cache hit for request. CityCode: {hotelsSearchRequest.CityCode}, " +
                           $"CheckIn: {hotelsSearchRequest.CheckInDate}, CheckOut: { hotelsSearchRequest.CheckOutDate}, pageSize: {hotelsSearchRequest.PageSize}, pageOffset: {hotelsSearchRequest.PageOffset}");
                }

                
                return Ok(response);
            }
            catch(ArgumentException argEx)
            {
                _logger.LogWarning(argEx, argEx.Message);
                return StatusCode((int)HttpStatusCode.BadRequest, new { message =  argEx.Message });
            }
            catch(HttpRequestException reqEx)
            {
                _logger.LogError(reqEx, "Cannot retrieve Amadeus Hotels information from Amadeus API.");
                return StatusCode((int)HttpStatusCode.BadGateway, new { message = "Cannot retrieve Amadeus Hotels information from Amadeus API. Reason: " + reqEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error.");
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "Internal error." });
            }

        }

        private void ValidateAndSanitazeHotelsSearchRequest(HotelsSearchUserRequest hotelsSearchRequest)
        {
            if (String.IsNullOrEmpty(hotelsSearchRequest.CityCode))
            {
                throw new ArgumentException("City code must be provided.");
            }
            if (hotelsSearchRequest.CityCode.Length != 3)
            {
                throw new ArgumentException("City code must have three letters.");
            }
            if (hotelsSearchRequest.CheckInDate.Date < DateTime.Now.Date)
            {
                throw new ArgumentException("Check-in date can not be in past.");
            }
            if (hotelsSearchRequest.CheckOutDate <= hotelsSearchRequest.CheckInDate.AddDays(1))
            {
                throw new ArgumentException("Check-out date must be at least one day after check-in date.");
            }
            if(hotelsSearchRequest.PageSize < 1 || hotelsSearchRequest.PageOffset < 0)
            {
                throw new ArgumentException("Invalid page size or page offset values.");
            }
            if(hotelsSearchRequest.PageSize > 100)
            {
                throw new ArgumentException("Maximum page size is 100.");
            }

            hotelsSearchRequest.CheckInDate = hotelsSearchRequest.CheckInDate.Date;
            hotelsSearchRequest.CheckOutDate = hotelsSearchRequest.CheckOutDate.Date;
        } 
    }
}