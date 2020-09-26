﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using AdventureWorks.OAuth.Configuration;
using AdventureWorks.OAuth.Data;
using AdventureWorks.OAuth.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdventureWorks.OAuth
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString.UserIdentityConnectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);
            
           
          //  services.Configure<IISOptions>(iis =>
          //  {
          //      iis.AuthenticationDisplayName = "Windows";
          //      iis.AutomaticAuthentication = false;
          //  });

          var builder = services.AddIdentityServer()
              .AddConfigurationStore(options =>
              {
                  options.ConfigureDbContext = b => b.UseSqlServer(connectionString.IdentityConnectionString);
              })
              .AddOperationalStore(options =>
              {
                  options.ConfigureDbContext = b => b.UseSqlServer(connectionString.IdentityConnectionString);
              }).AddAspNetIdentity<ApplicationUser>();

            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }
            
            
            services.AddAuthentication()
                 .AddGoogle(options =>
                 {
                     // register your IdentityServer with Google at https://console.developers.google.com
                     // enable the Google+ API
                     // set the redirect URI to http://localhost:5000/signin-google
                     options.ClientId = "copy client ID from Google here";
                     options.ClientSecret = "copy client secret from Google here";
                 });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }
    }
}