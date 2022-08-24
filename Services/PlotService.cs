﻿

using Microsoft.AspNetCore.Identity;

namespace Services
{
    public class PlotService : IPlotService
    {
        private readonly IDbConnection _db;
        private readonly IMemoryCache _cache;
        private readonly UserManager<UserModel> _user;
        private readonly IMongoCollection<PlotModel> _plots;
        private const string CacheName = "PlotData";

        public PlotService(IDbConnection db, IMemoryCache cache, UserManager<UserModel> user)
        {
            _db = db;
            _cache = cache;
            _user = user;
            _plots = db.PlotCollection;
        }

        public async Task<List<PlotModel>> GetAllPlots()
        {
            var output = _cache.Get<List<PlotModel>>(CacheName);
            if (output is null)
            {
                var result = await _plots.FindAsync(_ => true);
                output = result.ToList();
                _cache.Set(CacheName, output, TimeSpan.FromMinutes(1));
            }
            return output;
        }

        public async Task<List<PlotModel>> GetUsersPlots(string userId)
        {
            //var output = _cache.Get<List<PlotModel>>(userId);
            //if (output is null)
            //{
            //    //var userPlotsIds = _user.Users.FirstOrDefault(u => userId)?.PlotsIds;
            //    if (userPlotsIds is null) return null;
            //    var plots = await GetAllPlots();
            //    var userPlots = plots.ToList().Where(p => userPlotsIds.Contains(p.Id.ToString()));
            //    output = plots.ToList();

            //    _cache.Set(userId, output, TimeSpan.FromMinutes(1));
            //}

            //return output;
            throw new NotImplementedException();
        }

        public async Task<PlotModel> GetPlot(string id)
        {
            //var results = await _plots.FindAsync(s => s.Id == id);
            //return results.FirstOrDefault();
            throw new NotImplementedException();
        }

        public async Task UpdateSuggestion(PlotModel plot)
        {
            await _plots.ReplaceOneAsync(s => s.Id == plot.Id, plot);
            _cache.Remove(CacheName);
        }

        public async Task CreatePlot(PlotModel plot, ObjectId userId)
        {
            var client = _db.Client;

            using var session = await client.StartSessionAsync();

            session.StartTransaction();

            try
            {
                var db = client.GetDatabase(_db.DbName);
                var plotInTransaction = db.GetCollection<PlotModel>(_db.PlotCollectionName);
                await plotInTransaction.InsertOneAsync(plot);

                //var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
                //var user = await _userData.GetUser(userId);
                //user.PlotsIds.Add(plot.Id);
                //await usersInTransaction.ReplaceOneAsync(u => u.Id == user.Id, user);

                await session.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }

        public async Task DeletePlot(string plotId, string userId)
        {
            //var client = _db.Client;

            //using var session = await client.StartSessionAsync();

            //session.StartTransaction();

            //try
            //{
            //    var db = client.GetDatabase(_db.DbName);
            //    var plotInTransaction = db.GetCollection<PlotModel>(_db.PlotCollectionName);
            //    await plotInTransaction.DeleteOneAsync(p => p.Id == plotId);

            //    var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
            //    var user = _user.Users.FirstOrDefault(u => u.UserId == userId);
            //    if (user == null) return;
            //    await usersInTransaction.ReplaceOneAsync(u => u.Id == user.Id, user);

            //    await session.CommitTransactionAsync();
            //}
            //catch (Exception ex)
            //{
            //    await session.AbortTransactionAsync();
            //    throw;
            //}
            throw new NotImplementedException();
        }
    }
}
