﻿using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Routing;
using Moq;

internal static class ApiContextHelper
{
    public static HttpControllerContext CreateControllerContext(HttpConfiguration configuration = null, IHttpController instance = null, IHttpRouteData routeData = null, HttpRequestMessage request = null)
    {
        HttpConfiguration config = configuration ?? new HttpConfiguration();
        IHttpRouteData route = routeData ?? new HttpRouteData(new HttpRoute());
        HttpRequestMessage req = request ?? new HttpRequestMessage();
        req.SetConfiguration(config);
        req.SetRouteData(route);

        HttpControllerContext context = new HttpControllerContext(config, route, req);
        if (instance != null)
        {
            context.Controller = instance;
        }
        context.ControllerDescriptor = CreateControllerDescriptor(config);

        return context;
    }

    public static HttpActionContext CreateActionContext(HttpControllerContext controllerContext = null, HttpActionDescriptor actionDescriptor = null)
    {
        HttpControllerContext context = controllerContext ?? ApiContextHelper.CreateControllerContext();
        HttpActionDescriptor descriptor = actionDescriptor ?? CreateActionDescriptor();
        descriptor.ControllerDescriptor = context.ControllerDescriptor;
        return new HttpActionContext(context, descriptor);
    }

    public static HttpActionContext GetHttpActionContext(HttpRequestMessage request)
    {
        HttpActionContext actionContext = CreateActionContext();
        actionContext.ControllerContext.Request = request;
        return actionContext;
    }

    public static HttpActionExecutedContext GetActionExecutedContext(HttpRequestMessage request, HttpResponseMessage response)
    {
        HttpActionContext actionContext = CreateActionContext();
        actionContext.ControllerContext.Request = request;
        HttpActionExecutedContext actionExecutedContext = new HttpActionExecutedContext(actionContext, null) { Response = response };
        return actionExecutedContext;
    }

    public static HttpControllerDescriptor CreateControllerDescriptor(HttpConfiguration config = null)
    {
        if (config == null)
        {
            config = new HttpConfiguration();
        }
        return new HttpControllerDescriptor() { Configuration = config, ControllerName = "TestController"};
    }

    public static HttpActionDescriptor CreateActionDescriptor()
    {
        var mock = new Mock<HttpActionDescriptor>() { CallBase = true };
        mock.SetupGet(d => d.ActionName).Returns("TestAction");
        return mock.Object;
    }
}