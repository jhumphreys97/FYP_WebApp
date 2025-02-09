﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using AutoMapper;
using FYP_WebApp.Common_Logic;
using FYP_WebApp.DataAccessLayer;
using FYP_WebApp.DTO;
using FYP_WebApp.Models;
using FYP_WebApp.ServiceLayer;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Security.Facebook;
using Newtonsoft.Json;

namespace FYP_WebApp.API
{


    public class AccountController : ApiController
    {

        private readonly ApplicationDbContext _applicationDbContext;
        private  readonly Mapper _mapper;
        private readonly LogService _logService;

        public AccountController()
        {
            _applicationDbContext = new ApplicationDbContext();
            _logService = new LogService();
            var config = AutomapperConfig.instance().Configure();
            _mapper = new Mapper(config);
        }

        public AccountController(ApplicationDbContext context, IApiLogRepository apiLogRepository, Mapper mapper)
        {
            _applicationDbContext = context;
            _logService = new LogService(apiLogRepository);
            _mapper = mapper;
        }

        [HttpGet]
        public IEnumerable<MobileUserDto> Get()
        {
            var response =
                _mapper.Map<IList<ApplicationUser>, List<MobileUserDto>>(_applicationDbContext.Users.ToList());
            _logService.CreateApiLog(new ApiLog
            {
                LogLevel = LogLevel.Info,
                Controller = this.ControllerContext.ControllerDescriptor.ControllerName,
                Action = this.ActionContext.ActionDescriptor.ActionName,
                TimeStamp = DateTime.Now,
                RequestString = "",
                ResponseString = JsonConvert.SerializeObject(response, Formatting.Indented),
                StatusCode = HttpStatusCode.OK.ToString()
            });
            return response;
        }

        [HttpPost]
        [Route("api/Account/Login")]
        public IHttpActionResult Login(string username, string loginKey)
        {
            var user = _applicationDbContext.Users
                .FirstOrDefault(x => x.UserName == username && x.MobileLoginKey == loginKey);

            if (user == null)
            {
                var response = Content(HttpStatusCode.NotFound, "No user matched the specified parameters.");

                _logService.CreateApiLog(new ApiLog
                {
                    LogLevel = LogLevel.Error,
                    Controller = this.ControllerContext.ControllerDescriptor.ControllerName,
                    Action = this.ActionContext.ActionDescriptor.ActionName,
                    TimeStamp = DateTime.Now,
                    RequestString = JsonConvert.SerializeObject(new { username = username, loginKey = loginKey}),
                    ResponseString = JsonConvert.SerializeObject(response.Content, Formatting.Indented),
                    StatusCode = HttpStatusCode.NotFound.ToString()
                });

                return response;
            }
            else
            {
                if (user.IsInactive)
                {
                    var inactiveResponse = Content(HttpStatusCode.Unauthorized, "User is no longer active on this system.");

                    _logService.CreateApiLog(new ApiLog
                    {
                        LogLevel = LogLevel.Warn,
                        Controller = this.ControllerContext.ControllerDescriptor.ControllerName,
                        Action = this.ActionContext.ActionDescriptor.ActionName,
                        TimeStamp = DateTime.Now,
                        RequestString = JsonConvert.SerializeObject(new { username = username, loginKey = loginKey }),
                        ResponseString = JsonConvert.SerializeObject(inactiveResponse.Content, Formatting.Indented),
                        StatusCode = HttpStatusCode.Unauthorized.ToString()
                    });

                    return inactiveResponse;
                }

                if (user.LockoutEndDateUtc != null)
                {
                    var lockedOutResponse = Content(HttpStatusCode.Forbidden, "This account is currently locked.");

                    _logService.CreateApiLog(new ApiLog
                    {
                        LogLevel = LogLevel.Warn,
                        Controller = this.ControllerContext.ControllerDescriptor.ControllerName,
                        Action = this.ActionContext.ActionDescriptor.ActionName,
                        TimeStamp = DateTime.Now,
                        RequestString = JsonConvert.SerializeObject(new { username = username, loginKey = loginKey }),
                        ResponseString = JsonConvert.SerializeObject(lockedOutResponse.Content, Formatting.Indented),
                        StatusCode = HttpStatusCode.Forbidden.ToString()
                    });

                    return lockedOutResponse;
                }

                var response = Json(_mapper.Map<ApplicationUser, MobileUserDto>(user));


                _logService.CreateApiLog(new ApiLog
                {
                    LogLevel = LogLevel.Info,
                    Controller = this.ControllerContext.ControllerDescriptor.ControllerName,
                    Action = this.ActionContext.ActionDescriptor.ActionName,
                    TimeStamp = DateTime.Now,
                    RequestString = JsonConvert.SerializeObject(new { username = username, loginKey = loginKey }),
                    ResponseString = JsonConvert.SerializeObject(response.Content, Formatting.Indented),
                    StatusCode = HttpStatusCode.OK.ToString()
                });

                return response;
            }
        }
    }
}
