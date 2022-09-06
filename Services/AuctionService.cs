﻿using Microsoft.AspNetCore.Identity;

namespace Services
{
    public class AuctionService : IAuctionService
    {
        private readonly IDbConnection _db;
        private IMongoCollection<CategoryModel> _types;
        private IMongoCollection<ItemModel> _auctions;
        private readonly UserManager<UserModel> _user;


        public AuctionService(IDbConnection db, UserManager<UserModel> user)
        {
            _db = db;
            _user = user;
            _auctions = db.AuctionCollection;
            _types = db.TypeCollection;

        }
        public PagedResult<ItemModel> GetAuctions(Query query, string userId = "")
        {
            var results = _auctions.Find(_ => true).ToList();

            if (!string.IsNullOrWhiteSpace(userId))
            {
                results = results.Where(a => a.AuthorId == userId).ToList();
            }   

            if (!string.IsNullOrWhiteSpace(query.SearchPhrase))
            {
                results = results.Where(r => r.Description.Contains(query.SearchPhrase, StringComparison.OrdinalIgnoreCase)
                || r.Title.Contains(query.SearchPhrase, StringComparison.OrdinalIgnoreCase)
                || r.Type.Name.Contains(query.SearchPhrase, StringComparison.OrdinalIgnoreCase))
               .ToList();
            }

            if (query.PageNumber < 1)
            {
                query.PageNumber = (int)Math.Ceiling(results.Count / (double)query.PageSize);
            }

            if (query.PageNumber > (int)Math.Ceiling(results.Count / (double)query.PageSize))
            {
                query.PageNumber = 1;
            }


            var pagedResult = results.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToList();

            var totalItems = results.Count;

            var result = new PagedResult<ItemModel>(pagedResult, totalItems, query.PageSize, query.PageNumber, query.SearchPhrase);

            return result;
        }

        public ItemModel GetAuction(string auctionId)
        {
            var result = _auctions.Find(a => a.Id == auctionId).FirstOrDefault();
            return result;
        }

        public async Task<bool> CreateAuction(ItemDto dto, string userId)
        {
            var client = _db.Client;

            var auction = new ItemModel()
            {
                AuthorId = userId,
                Description = dto.Description,
                IsActive = dto.IsActive,
                Price = dto.Price,
                Title = dto.Title,
                TypeId = dto.Type,
            };

            auction.Type = (await _types.FindAsync(t => t.Id == auction.TypeId)).FirstOrDefault();

            //using var session = await client.StartSessionAsync();

            //session.StartTransaction();

            try
            {
                var db = client.GetDatabase(_db.DbName);
                var auctionInTransaction = db.GetCollection<ItemModel>(_db.AuctionCollectionName);
                await auctionInTransaction.InsertOneAsync(auction);

                var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
                var user = await _user.FindByIdAsync(userId);

                user.Auctions.Add(auction.Id);

                await usersInTransaction.ReplaceOneAsync(u => u.Id == user.Id, user);

                //await session.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                //await session.AbortTransactionAsync();
                return false;
            }
        }

        public async Task<bool> DeleteAuction(string auctionId, string userId)
        {
            var client = _db.Client;

            //using var session = await client.StartSessionAsync();

            //session.StartTransaction();

            try
            {
                var db = client.GetDatabase(_db.DbName);
                var auctionInTransaction = db.GetCollection<ItemModel>(_db.AuctionCollectionName);
                await auctionInTransaction.DeleteOneAsync(a => a.Id == auctionId);

                var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
                var user = await _user.FindByIdAsync(userId);
                user.Auctions.Remove(auctionId);
                await usersInTransaction.ReplaceOneAsync(u => u.Id == user.Id, user);

                //await session.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                //await session.AbortTransactionAsync();
                return false;
            }
        }

        public async Task<bool> UpdateAuction(string auctionId, ItemDto dto, string userId)
        {
            var auctionEntity = GetAuction(auctionId);

            auctionEntity.Title = dto.Title;
            auctionEntity.Price = dto.Price;
            auctionEntity.Description = dto.Description;
            auctionEntity.IsActive = dto.IsActive;
            auctionEntity.TypeId = dto.Type;
            auctionEntity.Type = (await _types.FindAsync(t => t.Id == dto.Type)).FirstOrDefault();

            var result = await _auctions.ReplaceOneAsync(a => a.Id == auctionId, auctionEntity);

            if (result.ModifiedCount > 0)
            {
                return true;
            }
            return false;
        }

        public List<CategoryModel> GetAllTypes()
        {
            var results = _types.Find(_ => true);
            return results.ToList();
        }

        public async Task<bool> CreateType(CategoryModel type)
        {
            var client = _db.Client;

            //using var session = await client.StartSessionAsync();

            //session.StartTransaction();

            try
            {
                var db = client.GetDatabase(_db.DbName);
                var typeInTransaction = db.GetCollection<CategoryModel>(_db.TypeCollectionName);
                await typeInTransaction.InsertOneAsync(type);

                //await session.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                //await session.AbortTransactionAsync();
                return false;
            }
        }

        public async Task<bool> DeleteType(string typeId)
        {
            var client = _db.Client;

            //using var session = await client.StartSessionAsync();

            //session.StartTransaction();

            try
            {
                var db = client.GetDatabase(_db.DbName);
                var typeInTransaction = db.GetCollection<CategoryModel>(_db.TypeCollectionName);
                await typeInTransaction.DeleteOneAsync(t => t.Id == typeId);

                //await session.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                //await session.AbortTransactionAsync();
                return false;
            }
        }
    }
}
