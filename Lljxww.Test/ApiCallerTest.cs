﻿using Lljxww.ApiCaller;
using Lljxww.ApiCaller.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lljxww.Test;

[TestClass]
public class ApiCallerTest
{
    private readonly IServiceCollection services = new ServiceCollection();

    public ApiCallerTest()
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("apicaller.json")
            .Build();

        services.ConfigureCaller(config);

        CallerEvents.OnExecuted += context =>
        {
            string apiName = context.ApiName;
        };
    }

    [TestMethod]
    public void InvokeTest()
    {
        Caller? caller = services.BuildServiceProvider().GetRequiredService<Caller>();

        string? username = "liang1224";

        ApiResult? result = caller.InvokeAsync("gh.GetUserInfo", new
        {
            username
        }, new RequestOption
        {
            CustomAuthorizeInfo = "123"
        }).Result;

        Assert.AreEqual(username, result!["login"]);
        Assert.AreEqual(username, result!["Login"]);
        Assert.AreEqual(username, result!["lOgin"]);
        Assert.AreEqual(username, result!["loGin"]);
        Assert.AreEqual(username, result!["logIn"]);
        Assert.AreEqual(username, result!["logiN"]);
    }
}