﻿using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ABSBuybackMVCWebAPI.Startup))]

namespace ABSBuybackMVCWebAPI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
