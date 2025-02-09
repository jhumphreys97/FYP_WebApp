﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using FYP_WebApp.Common_Logic;
using FYP_WebApp.DataAccessLayer;
using FYP_WebApp.DTO;
using FYP_WebApp.Models;
using FYP_WebApp.ServiceLayer;
using Newtonsoft.Json;

namespace FYP_WebApp.API
{
    public class StoredLocationController : ApiController
    {

        private readonly StoredLocationService _storedLocationService;
        private readonly LogService _logService;
        private readonly Mapper _mapper;

        public StoredLocationController()
        {
            _storedLocationService = new StoredLocationService();
            _logService = new LogService();
            var config = AutomapperConfig.instance().Configure();
            _mapper = new Mapper(config);
        }

        public StoredLocationController(IStoredLocationRepository locationRepository, IApiLogRepository apiLogRepository, Mapper mapper)
        {
            _storedLocationService = new StoredLocationService(locationRepository);
            _logService = new LogService(apiLogRepository);
            _mapper = mapper;
        }

        [HttpGet]
        public IEnumerable<StoredLocationDto> Get()
        {
            var response =  _mapper.Map<IList<StoredLocation>, List<StoredLocationDto>>(_storedLocationService.Index().Where(l => l.IsInactive == false).ToList());

            _logService.CreateApiLog(new ApiLog
            {
                LogLevel = LogLevel.Info,
                Controller = this.ControllerContext.ControllerDescriptor.ControllerName,
                Action = this.ActionContext.ActionDescriptor.ActionName,
                TimeStamp = DateTime.Now,
                RequestString = JsonConvert.SerializeObject("None", Formatting.Indented),
                ResponseString = JsonConvert.SerializeObject(response, Formatting.Indented),
                StatusCode = HttpStatusCode.OK.ToString()
            });

            return response;
        }

        [HttpGet]
        public IHttpActionResult Get(int id)
        {
            if (id == 0)
            {
                var response = Content(HttpStatusCode.BadRequest, "No ID was provided.");

                _logService.CreateApiLog(new ApiLog
                {
                    LogLevel = LogLevel.Error,
                    Controller = this.ControllerContext.ControllerDescriptor.ControllerName,
                    Action = this.ActionContext.ActionDescriptor.ActionName,
                    TimeStamp = DateTime.Now,
                    RequestString = JsonConvert.SerializeObject("None", Formatting.Indented),
                    ResponseString = JsonConvert.SerializeObject(response.Content, Formatting.Indented),
                    StatusCode = HttpStatusCode.NotFound.ToString()
                });

                return response;
            }

            var storedLocation = _storedLocationService.GetDetails(id);

            if (storedLocation != null)
            {
                var response = Json(_mapper.Map<StoredLocation, StoredLocationDto>(storedLocation));

                _logService.CreateApiLog(new ApiLog
                {
                    LogLevel = LogLevel.Info,
                    Controller = this.ControllerContext.ControllerDescriptor.ControllerName,
                    Action = this.ActionContext.ActionDescriptor.ActionName,
                    TimeStamp = DateTime.Now,
                    RequestString = JsonConvert.SerializeObject("None", Formatting.Indented),
                    ResponseString = JsonConvert.SerializeObject(response.Content, Formatting.Indented),
                    StatusCode = HttpStatusCode.OK.ToString()
                });

                return response;
            }
            else
            {
                var response = Content(HttpStatusCode.NotFound, "A location with the specified ID was not found.");

                _logService.CreateApiLog(new ApiLog
                {
                    LogLevel = LogLevel.Error,
                    Controller = this.ControllerContext.ControllerDescriptor.ControllerName,
                    Action = this.ActionContext.ActionDescriptor.ActionName,
                    TimeStamp = DateTime.Now,
                    RequestString = JsonConvert.SerializeObject("None", Formatting.Indented),
                    ResponseString = JsonConvert.SerializeObject(response.Content, Formatting.Indented),
                    StatusCode = HttpStatusCode.NotFound.ToString()
                });

                return response;
            }
        }
    }
}
