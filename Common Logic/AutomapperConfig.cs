﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using FYP_WebApp.DTO;
using FYP_WebApp.Models;

namespace FYP_WebApp.Common_Logic
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<StoredLocation, StoredLocationDto>();
            CreateMap<Message, MessageDto>();
            CreateMap<GpsReport, GpsReportDto>();

            //Reverse
            CreateMap<MessageDto, Message>();
            CreateMap<GpsReportDto, GpsReport>();
        }
    }

    public class AutomapperConfig
    {
        private static AutomapperConfig _instance;

        static AutomapperConfig()
        {
            _instance = new AutomapperConfig();
        }
        
        public static AutomapperConfig instance()
        {
            return _instance;
        }

        public MapperConfiguration Configure()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            return config;
        }
    }
}