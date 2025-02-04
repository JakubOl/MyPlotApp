﻿using Microsoft.Extensions.Configuration;
using Models.Entities;
using MongoDB.Driver;

namespace DataAccess;
public class DbConnection : IDbConnection
{
    private readonly IConfiguration _config;
    private readonly IMongoDatabase _db;
    private string _connectionId = "MongoDB";
    public string DbName { get; private set; }

    public string PlotCollectionName { get; private set; } = "plots";
    public string RoleCollectionName { get; private set; } = "roleModels";
    public string UserCollectionName { get; private set; } = "userModels";
    public string AuctionCollectionName { get; private set; } = "auctions";
    public string CommentCollectionName { get; private set; } = "comments";
    public string CategoryCollectionName { get; private set; } = "categories";


    public MongoClient Client { get; private set; }
    public IMongoCollection<PlotModel> PlotCollection { get; private set; }
    public IMongoCollection<RoleModel> RoleCollection { get; private set; }
    public IMongoCollection<UserModel> UserCollection { get; private set; }
    public IMongoCollection<ItemModel> AuctionCollection { get; private set; }
    public IMongoCollection<CommentModel> CommentCollection { get; private set; }
    public IMongoCollection<CategoryModel> CategoryCollection { get; private set; }



    public DbConnection(IConfiguration config)
    {
        _config = config;
        Client = new MongoClient(_config.GetConnectionString(_connectionId));
        DbName = _config["DatabaseName"];
        _db = Client.GetDatabase(DbName);


        PlotCollection = _db.GetCollection<PlotModel>(PlotCollectionName);
        RoleCollection = _db.GetCollection<RoleModel>(RoleCollectionName);
        UserCollection = _db.GetCollection<UserModel>(UserCollectionName);
        AuctionCollection = _db.GetCollection<ItemModel>(AuctionCollectionName);
        CommentCollection = _db.GetCollection<CommentModel>(CommentCollectionName);
        CategoryCollection = _db.GetCollection<CategoryModel>(CategoryCollectionName);

    }
}