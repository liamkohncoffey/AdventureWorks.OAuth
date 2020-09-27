// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Security.Claims;
using AdventureWorks.OAuth.Configuration;
using AdventureWorks.OAuth.Data;
using AdventureWorks.OAuth.Models;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
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
                     //options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                     //options.ClientId = "244245730187-2t04d5ifu1r0lede8vc7nop6coft6v39.apps.googleusercontent.com";
                     //options.ClientSecret = "5MQsMIbqM9CGAxDtkTdAs_rx";
                     options.ClientId = "244245730187-2t04d5ifu1r0lede8vc7nop6coft6v39.apps.googleusercontent.com";
                     options.ClientSecret = "5MQsMIbqM9CGAxDtkTdAs_rx";
                     options.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
                     options.ClaimActions.Clear();
                     options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                     options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                     options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
                     options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
                     options.ClaimActions.MapJsonKey("urn:google:profile", "link");
                     options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
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