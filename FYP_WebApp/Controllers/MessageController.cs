﻿using FYP_WebApp.Common_Logic;
using FYP_WebApp.ServiceLayer;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Web.Mvc;
using FYP_WebApp.DataAccessLayer;
using FYP_WebApp.Models;

namespace FYP_WebApp.Controllers
{
    [CustomAuth(Roles = "Admin, Manager, Member")]
    public class MessageController : Controller
    {
        private readonly MessageService _messageService;
        private readonly AccountService _accountService;

        public MessageController()
        {
            _messageService = new MessageService();
            _accountService = new AccountService();
        }

        public MessageController(IMessageRepository messageRepository, IPairingRepository pairingRepository, ITeamRepository teamRepository, ApplicationDbContext context, IConfigurationRecordRepository configurationRecordRepository)
        {
            _messageService = new MessageService(messageRepository, pairingRepository, teamRepository, context,
                configurationRecordRepository);
            _accountService = new AccountService(context);
        }

        public ActionResult Index(int? messageSent)
        {
            if (messageSent == 1)
            {
                ViewBag.MessageSent = true;
            }

            var userList = new SelectList(_accountService.GetAll().Where(x => x.IsInactive != true).ToList(), "Id", "DisplayString");
            ViewBag.UserList = userList;
            var userId = User.Identity.GetUserId();
            return View(_messageService.GetAll().Where(x=>x.RecipientId == userId));
        }

        public ActionResult Create(string recipient)
        {
            var userList = new SelectList(_accountService.GetAll().Where(x => x.IsInactive != true).ToList(), "Id", "DisplayString");
            ViewBag.UserList = userList;

            if (!string.IsNullOrEmpty(recipient))
            {
                return View(new Message {RecipientId = recipient});
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string recipient, string content)
        {

            if (string.IsNullOrEmpty(recipient))
            {
                return RedirectToAction("Error", "Error", new { @Error = Errors.InvalidParameter, @Message = "A recipient is required."});
            }

            if (string.IsNullOrEmpty(content))
            {
                return RedirectToAction("Error", "Error", new { @Error = Errors.InvalidParameter, @Message = "A message cannot be empty." });
            }

            var userList = new SelectList(_accountService.GetAll().Where(x => x.IsInactive != true).ToList(), "Id", "DisplayString");
            ViewBag.UserList = userList;

            var message = new Message {RecipientId = recipient, Content = content};

            message.SenderId = User.Identity.GetUserId();
            message.MessageReceived = DateTime.Now;
            message.MessageType = MessageType.Routine;
            
            var result = _messageService.Create(message);

            if (result.Success)
            {
                return RedirectToAction("Index", "Message", new{messageSent = 1});
            }
            else
            {
                return View(message);
            }
        }

        public ActionResult Details(int id)
        {
            try
            {
                _messageService.MarkMessageAsRead(id);
                return View(_messageService.GetDetails(id));
            }
            catch (ArgumentException ex)
            {
                return RedirectToAction("Error", "Error", new { @Error = Errors.InvalidParameter, @Message = ex.Message });
            }
            catch (MessageNotFoundException ex)
            {
                return RedirectToAction("Error", "Error", new { @Error = Errors.EntityNotFound, @Message = ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _messageService.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}