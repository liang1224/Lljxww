﻿using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace Lljxww.Common.WebApiCaller.Models.Config
{
    /// <summary>
    /// 接口配置文件
    /// </summary>
    public class ApiCallerConfig : IOptions<ApiCallerConfig>
    {
        /// <summary>
        /// 授权方式
        /// </summary>
        public IList<Authorization> Authorizations { get; set; }

        /// <summary>
        /// 接口配置节
        /// </summary>
        public IList<ServiceItem> ServiceItems { get; set; }

        public ApiCallerConfig Value => this;
    }
}
