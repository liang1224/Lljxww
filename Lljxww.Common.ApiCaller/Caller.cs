﻿using System.Net;
using Lljxww.Common.ApiCaller;
using Lljxww.Common.ApiCaller.Models;
using Lljxww.Common.ApiCaller.Models.Config;
using Microsoft.Extensions.Options;

namespace Lljxww.Common.WebApiCaller
{
    public partial class Caller
    {
        public async Task<ApiResult?> InvokeAsync(string apiNameAndMethodName, object? requestParam = null, RequestOption? requestOption = null)
        {
            // 创建请求对象
            CallerContext context = CallerContext.Build(apiNameAndMethodName, _apiCallerConfig, requestParam, requestOption);

            requestOption ??= new RequestOption();

            // 尝试从缓存读取结果
            if (context.NeedCache && requestOption.IsFromCache)
            {
                context.ApiResult = GetCacheEvent?.Invoke(context);

                if (context.ApiResult != null)
                {
                    context.ResultFrom = "Cache";
                }
            }

            // 从新Http请求获取结果
            if (context.ApiResult == null)
            {
                try
                {
                    // 设置本次请求的超时时间（如果有）
                    if (requestOption?.Timeout != -1)
                    {
                        if (requestOption != null)
                        {
                            context.Timeout = requestOption.Timeout;
                        }
                    }

                    // 执行请求
                    context = await context.RequestAsync();
                }
                catch (Exception ex)
                {
                    Exception innerException = ex;

                    while (innerException.InnerException != null)
                    {
                        innerException = innerException.InnerException;
                    }

                    string? fullName = innerException.GetType().FullName;
                    if (fullName != null && fullName.Contains(nameof(TaskCanceledException)))
                    {
                        try
                        {
                            // 取消请求后的操作
                            OnRequestTimeout?.Invoke(context);
                        }
                        catch { }
                        return new ApiResult(false, "上游服务响应超时");
                    }
                    else
                    {
                        try
                        {
                            // 触发异常事件
                            OnException?.Invoke(context, ex);
                        }
                        catch { }
                        throw innerException;
                    }
                }

                // 处理缓存
                if (context.NeedCache)
                {
                    try
                    {
                        SetCacheEvent?.Invoke(context);
                    }
                    catch { }
                }
            }

            // 记录日志事件
            try
            {
                LogEvent?.Invoke(context);
            }
            catch { }

            // 执行后事件
            try
            {
                if (requestOption != null && requestOption.IsTriggerOnExecuted)
                {
                    OnExecuted?.Invoke(context);
                }
            }
            catch { }

            return context.ApiResult;
        }
    }

    public partial class Caller
    {
        private readonly ApiCallerConfig _apiCallerConfig;

        private readonly IHttpClientFactory _httpClientFactory;


        public Caller(IHttpClientFactory httpClientFactory, IOptions<ApiCallerConfig> apiCallerConfigOption)
        {
            _httpClientFactory = httpClientFactory;
            _apiCallerConfig = apiCallerConfigOption.Value;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        #region 事件

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public delegate void SetCacheHandler(CallerContext context);
        public static event SetCacheHandler SetCacheEvent;

        /// <summary>
        /// 读取缓存
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public delegate ApiResult? GetCacheHandler(CallerContext context);
        public static event GetCacheHandler GetCacheEvent;

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="context"></param>
        public delegate void LogHandler(CallerContext context);
        public static event LogHandler LogEvent;

        /// <summary>
        /// 请求方法执行结束后的操作
        /// </summary>
        /// <param name="context"></param>
        public delegate void OnExecutedHandler(CallerContext context);
        public static event OnExecutedHandler OnExecuted;

        /// <summary>
        /// 执行发生异常时触发
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ex"></param>
        public delegate void OnExceptionHandler(CallerContext context, Exception ex);
        public static event OnExceptionHandler OnException;

        /// <summary>
        /// 请求超时时触发
        /// </summary>
        /// <param name="context"></param>
        public delegate void OnRequestTimeoutHandler(CallerContext context);
        public static event OnRequestTimeoutHandler OnRequestTimeout;

        #endregion
    }
}
