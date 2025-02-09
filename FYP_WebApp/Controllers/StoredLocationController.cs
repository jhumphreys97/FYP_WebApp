﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FYP_WebApp.Common_Logic;
using FYP_WebApp.DataAccessLayer;
using FYP_WebApp.Models;
using FYP_WebApp.ServiceLayer;
using Microsoft.AspNet.Identity;
using PagedList;

namespace FYP_WebApp.Controllers
{
    public class StoredLocationController : Controller
    {
        private readonly StoredLocationService _storedLocationService;
        private readonly TeamService _teamService;
        private readonly GpsReportService _gpsReportService;
        private readonly ConfigurationRecordService _configurationRecordService;

        public StoredLocationController()
        {
            _storedLocationService = new StoredLocationService();
            _teamService = new TeamService();
            _gpsReportService = new GpsReportService();
            _configurationRecordService = new ConfigurationRecordService();
        }

        public StoredLocationController(IStoredLocationRepository locationRepository, ITeamRepository teamRepository, IGpsReportRepository gpsRepository, IConfigurationRecordRepository configRepository)
        {
            _storedLocationService = new StoredLocationService(locationRepository);
            _teamService = new TeamService(teamRepository);
            _gpsReportService = new GpsReportService(gpsRepository);
            _configurationRecordService = new ConfigurationRecordService(configRepository);
        }

        [CustomAuth(Roles = "Admin, Manager, Member")]
        public ActionResult Index(string currentFilter, string searchStringName, int? page)
        {
            var locations = _storedLocationService.Index();

            if (!User.IsInRole("Admin"))
            {
                locations = locations.Where(l => l.IsInactive == false).ToList();
            }

            if (searchStringName != null)
            {
                page = 1;
            }
            else
            {
                searchStringName = currentFilter;
            }

            ViewBag.CurrentFilter = searchStringName;

            if (!string.IsNullOrEmpty(searchStringName))
            {
                locations = locations.Where(s =>
                    s.Label.IndexOf(searchStringName, StringComparison.CurrentCultureIgnoreCase) >= 0).ToList();
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            return View(locations.ToPagedList(pageNumber, pageSize));
        }

        [CustomAuth(Roles = "Admin, Manager, Member")]
        public ActionResult Details(int id)
        {
            try
            {
                var storedLocation = _storedLocationService.GetDetails(id);
                var mapsApiKey = _configurationRecordService.GetLatestConfigurationRecord().MapsApiKey;
                ViewBag.mapUrl =
                    $"https://www.google.com/maps/embed/v1/place?key={mapsApiKey}&q={storedLocation.Latitude},{storedLocation.Longitude}";

                if (!User.IsInRole("Admin"))
                {
                    var teamIds = new List<int>();

                    foreach (var team in _teamService.GetAll())
                    {
                        if (team.ManagerId == User.Identity.GetUserId())
                        {
                            teamIds.Add(team.Id);
                        }
                    }

                    if (teamIds.Count > 0)
                    {
                        var visibleCheckins = new List<GpsReport>();
                        foreach (var checkin in storedLocation.CheckIns)
                        {
                            if (checkin.User.TeamId != null && teamIds.Contains((int)checkin.User.TeamId))
                            {
                                visibleCheckins.Add(checkin);
                            }
                        }

                        storedLocation.CheckIns = visibleCheckins;
                    }
                }

                if (storedLocation.CheckIns != null)
                {
                    storedLocation.CheckIns = storedLocation.CheckIns.OrderByDescending(x => x.Time).ToList();
                }

                return View(storedLocation);
            }
            catch (ArgumentException ex)
            {
                return RedirectToAction("Error", "Error", new { @Error = Errors.InvalidParameter, @Message = ex.Message });
            }
            catch (StoredLocationNotFoundException ex)
            {
                return RedirectToAction("Error", "Error", new { @Error = Errors.EntityNotFound, @Message = ex.Message });
            }
        }

        [CustomAuth(Roles = "Admin, Manager")]
        public ActionResult Create()
        {
            return View();
        }

        [CustomAuth(Roles = "Admin, Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Label,Latitude,Longitude")] StoredLocation storedLocation)
        {
            if (ModelState.IsValid)
            {
                ServiceResponse response = _storedLocationService.CreateAction(storedLocation);

                if (response.Success == true)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(storedLocation);
                }
            }

            return View(storedLocation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuth(Roles = "Admin, Manager")]
        public ActionResult CreateWithContent(string label, string latitude, string longitude)
        {
            var decimalLatitude = Convert.ToDecimal(latitude);
            var decimalLongitude = Convert.ToDecimal(longitude);

            if (string.IsNullOrEmpty(label))
            {
                return RedirectToAction("Error", "Error", new { @Error = Errors.EntityNotFound, @Message = "Unable to create location. Label not provided" });
            }

            ServiceResponse response = _storedLocationService.CreateAction(new StoredLocation { Label = label, Longitude = decimalLongitude, Latitude = decimalLatitude });

            if (response.Success == true)
            {
                var storedLocation = _storedLocationService.Index().FirstOrDefault(x => x.Label == label && x.Latitude == decimalLatitude && x.Longitude == decimalLongitude && x.IsInactive == false);

                _gpsReportService.LinkExistingLocations(storedLocation);

                if (storedLocation != null)
                {
                    return RedirectToAction("Details", "StoredLocation", new { @id = storedLocation.Id });
                }
                else
                {
                    return RedirectToAction("Error", "Error", new { @Error = Errors.EntityNotFound, @Message = "Unable to create location." });
                }
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [CustomAuth(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            try
            {
                return View(_storedLocationService.EditView(id));
            }
            catch (ArgumentException ex)
            {
                return RedirectToAction("Error", "Error", new { @Error = Errors.InvalidParameter, @Message = ex.Message });
            }
            catch (StoredLocationNotFoundException ex)
            {
                return RedirectToAction("Error", "Error", new { @Error = Errors.EntityNotFound, @Message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuth(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "Id,Label,Latitude,Longitude,IsInactive")] StoredLocation storedLocation)
        {

            if (ModelState.IsValid)
            {
                ServiceResponse response = _storedLocationService.EditAction(storedLocation);

                if (response.Success == true)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(storedLocation);
                }
            }

            return View(storedLocation);
        }

        [CustomAuth(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            try
            {
                return View(_storedLocationService.DeleteView(id));
            }
            catch (ArgumentException ex)
            {
                return RedirectToAction("Error", "Error", new { @Error = Errors.InvalidParameter, @Message = ex.Message });
            }
            catch (StoredLocationNotFoundException ex)
            {
                return RedirectToAction("Error", "Error", new { @Error = Errors.EntityNotFound, @Message = ex.Message });
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CustomAuth(Roles = "Admin")]
        public ActionResult DeleteAction(int id)
        {
            ServiceResponse response = _storedLocationService.DeleteAction(id);

            if (response.Success == true)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Error", "Error", new {@Error = Errors.SystemError, @Message="Deletion unsuccessful"});
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _storedLocationService.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
