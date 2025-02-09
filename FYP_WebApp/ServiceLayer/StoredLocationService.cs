﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FYP_WebApp.Common_Logic;
using FYP_WebApp.DataAccessLayer;
using FYP_WebApp.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.Facebook;

namespace FYP_WebApp.ServiceLayer
{
    public class StoredLocationService
    {
        private readonly IStoredLocationRepository _storedLocationRepository;

        public StoredLocationService()
        {
            _storedLocationRepository = new StoredLocationRepository(new ApplicationDbContext());
        }

        public StoredLocationService(IStoredLocationRepository storedLocationRepository)
        {
            _storedLocationRepository = storedLocationRepository;
        }

        public List<StoredLocation> Index()
        {
            return _storedLocationRepository.GetAll();
        }

        public StoredLocation GetDetails(int id)
        {
            if (id == 0)
            {
                throw new ArgumentException("No ID specified");
            }

            var storedLocation = _storedLocationRepository.GetById(id);

            if (storedLocation == null)
            {
                throw new StoredLocationNotFoundException("Location not found.");
            }

            return storedLocation;
        }

        public ServiceResponse CreateAction(StoredLocation storedLocation)
        {
            if (storedLocation != null)
            {
                _storedLocationRepository.Insert(storedLocation);
                _storedLocationRepository.Save();

                return new ServiceResponse {Success = true};
            }
            else
            {
                return new ServiceResponse {Success = false};
            }
        }

        public StoredLocation EditView(int id)
        {
            if (id == 0)
            {
                throw new ArgumentException("No ID specified");
            }

            var storedLocation = _storedLocationRepository.GetById(id);

            if (storedLocation == null)
            {
                throw new StoredLocationNotFoundException("Location not found.");
            }

            return storedLocation;
        }

        public ServiceResponse EditAction(StoredLocation storedLocation)
        {
            if (storedLocation != null)
            {
                _storedLocationRepository.Update(storedLocation);
                _storedLocationRepository.Save();

                return new ServiceResponse {Success = true};
            }
            else
            {
                return new ServiceResponse {Success = false};
            }
        }

        public StoredLocation DeleteView(int id)
        {
            if (id == 0)
            {
                throw new ArgumentException("No ID specified");
            }

            var storedLocation = _storedLocationRepository.GetById(id);

            if (storedLocation == null)
            {
                throw new StoredLocationNotFoundException("Location not found.");
            }

            return storedLocation;
        }

        public ServiceResponse DeleteAction(int id)
        {
            if (id != 0)
            {
                var storedLocation = DeleteView(id);

                _storedLocationRepository.Delete(storedLocation);
                _storedLocationRepository.Save();
                return new ServiceResponse {Success = true};
            }
            else
            {
                return new ServiceResponse {Success = false};
            }
        }

        public void Dispose()
        {
            _storedLocationRepository.Dispose();
        }
    }
}