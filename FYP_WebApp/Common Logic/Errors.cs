﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_WebApp.Common_Logic
{
    public enum Errors
    {
        AccountInactive = 0,
        AccountLocked = 1,
        AccountUnauthorized = 2,
        SystemError = 3,
        HttpError = 4,
        InvalidParameter = 5,
        EntityNotFound = 6,
        Unauthorized = 7
    }
}