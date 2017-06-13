﻿using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using StudioX.AspNetCore.App.Models;
using StudioX.AspNetCore.Mvc.Controllers;
using StudioX.UI;
using StudioX.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace StudioX.AspNetCore.App.Controllers
{
    public class SimpleTestController : StudioXController
    {
        public ActionResult SimpleContent()
        {
            return Content("Hello world...");
        }

        public JsonResult SimpleJson()
        {
            return Json(new SimpleViewModel("Forty Two", 42));
        }

        public JsonResult SimpleJsonException(string message, bool userFriendly)
        {
            if (userFriendly)
            {
                throw new UserFriendlyException(message);
            }

            throw new Exception(message);
        }

        [DontWrapResult]
        public JsonResult SimpleJsonExceptionDownWrap()
        {
            throw new UserFriendlyException("an exception message");
        }

        [DontWrapResult]
        public JsonResult SimpleJsonDontWrap()
        {
            return Json(new SimpleViewModel("Forty Two", 42));
        }

        [HttpGet]
        [WrapResult]
        public void GetVoidTest()
        {
            
        }

        [DontWrapResult]
        public void GetVoidTestDontWrap()
        {

        }

        [HttpGet]
        public ActionResult GetActionResultTest()
        {
            return Content("GetActionResultTest-Result");
        }

        [HttpGet]
        public async Task<ActionResult> GetActionResultTestAsync()
        {
            await Task.Delay(0);
            return Content("GetActionResultTestAsync-Result");
        }

        [HttpGet]
        public async Task GetVoidExceptionTestAsync()
        {
            await Task.Delay(0);
            throw new UserFriendlyException("GetVoidExceptionTestAsync-Exception");
        }

        [HttpGet]
        public async Task<ActionResult> GetActionResultExceptionTestAsync()
        {
            await Task.Delay(0);
            throw new UserFriendlyException("GetActionResultExceptionTestAsync-Exception");
        }

        [HttpGet]
        public ActionResult GetCurrentCultureNameTest()
        {
            return Content(CultureInfo.CurrentCulture.Name);
        }
    }
}
