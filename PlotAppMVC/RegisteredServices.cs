﻿using AutoMapper;
using DataAccess;
using Models.Entities;
using NLog.Web;
using Services;
using Services.ApiFiles;

namespace PlotAppMVC
{
    public static class RegisterServices
    {

        public static void ConfigureServices(this WebApplicationBuilder builder)
        {

            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });

            IMapper mapper = mapperConfig.CreateMapper();
            builder.Services.AddSingleton(mapper);

            builder.Services.AddControllersWithViews();

            builder.Logging.ClearProviders();
            builder.Host.UseNLog();

            builder.Services.AddMemoryCache();

            builder.Services.AddAuthenticationCore();

            builder.Services.AddSingleton<IDbConnection, DbConnection>();
            var connectionString = builder.Configuration.GetConnectionString("MongoDB");
            var databaseName = builder.Configuration.GetSection("myplotapp").Key;
            builder.Services.AddIdentity<UserModel, RoleModel>().AddMongoDbStores<UserModel, RoleModel, Guid>(connectionString, databaseName);

            builder.Services.AddTransient<IAccountService, AccountService>();
            builder.Services.AddTransient<IPlotService, PlotService>();
            builder.Services.AddTransient<IRoleService, RoleService>();
            builder.Services.AddTransient<IPlotProcessor, PlotProcessor>();
            builder.Services.AddTransient<IAuctionService, AuctionService>();

        }
    }
}
