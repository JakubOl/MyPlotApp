﻿using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Dtos;
using Models.Entities;
using Services;

namespace PlotAppMVC.Controllers
{
    [Authorize]
    public class AuctionController : Controller
    {
        private readonly IAuctionService _auctionService;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AuctionController(IAuctionService auctionService
            , IAccountService accountService
            , IMapper mapper, IWebHostEnvironment hostEnvironment)
        {
            _auctionService = auctionService;
            _accountService = accountService;
            _mapper = mapper;
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet("/auction")]
        public ActionResult Index([FromQuery] Query query)
        {
            var auctions = _auctionService.GetAuctions(query);

            ViewData["auctions"] = auctions;
            return View();
        }

        [HttpGet("/admin/auctions")]
        [Authorize(Roles = "Admin, Owner")]
        public ActionResult ManageAuctions([FromQuery] Query query)
        {
            var auctions = _auctionService.GetAuctions(query, true, null);

            ViewData["auctions"] = auctions;
            return View("ManageAuctions");
        }

        [HttpGet("/auction/user")]
        public ActionResult UserAuctions([FromQuery] Query query)
        {
            var userId = User?.Identity?.GetUserId();

            var auctions = _auctionService.GetAuctions(query, false, userId);

            ViewData["auctions"] = auctions;
            return View("UserAuctions");
        }

        [HttpGet("/auction/{auctionId}/details")]
        public async Task<ActionResult> Details(string auctionId)
        {
            var result = _auctionService.GetAuction(auctionId);

            var userId = User?.Identity?.GetUserId();
            var user = await _accountService.GetUserById(userId);

            ViewData["user"] = user;
            return View(result);
        }

        [HttpGet("/auction/create")]
        public ActionResult Create()
        {
            var categories = _auctionService.GetAllCategories();

            ViewData["categories"] = categories;
            return View();
        }

        // POST: AuctionController/Create
        [HttpPost("/auction/create")]
        public async Task<ActionResult> Create(ItemDto dto)
        {
            var categories = _auctionService.GetAllCategories();
            ViewData["categories"] = categories;

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var userId = User?.Identity?.GetUserId();

            var imageName = await SaveImage(dto);

            dto.ImageName = imageName;

            var auction = await _auctionService.CreateAuction(dto, userId);

            if (auction)
            {
                TempData["Success"] = "Auction added";
                return Redirect("/auction/user");
            }

            TempData["Error"] = "Auction creating failed";
            return View(dto);
        }

        [HttpGet("/auction/{auctionId}/edit")]
        public ActionResult Edit(string auctionId)
        {
            var auction = _auctionService.GetAuction(auctionId);
            if(auction is null)
            {
                TempData["Error"] = "Auction not found";
                return Redirect("/auction");
            }

            var categories = _auctionService.GetAllCategories();
            ViewData["categories"] = categories;

            var auctionDto = _mapper.Map<ItemDto>(auction);
            return View(auctionDto);
        }

        [HttpPost("/auction/{auctionId}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([FromForm] ItemDto dto, [FromRoute] string auctionId)
        {
            dto.Id = auctionId;

            var categories = _auctionService.GetAllCategories();
            ViewData["categories"] = categories;

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var userId = User?.Identity?.GetUserId();

            var auctionUpdated = await _auctionService.UpdateAuction(auctionId, dto, userId);

            if (auctionUpdated)
            {
                TempData["Success"] = "Auction Updated Successfully";
                return Redirect("/auction/user");
            }

            TempData["Error"] = "Auction Update Failed";
            return View(dto);
        }

        [HttpGet("/auction/{auctionId}/delete")]
        public ActionResult DeleteConfirm([FromRoute]string auctionId)
        {
            var auction = _auctionService.GetAuction(auctionId);
            ViewData["auction"] = auction;

            return View("ConfirmDeleteAuction");
        }

        [HttpPost("/auction/{auctionId}/delete/confirmed")]
        public async Task<ActionResult> Delete([FromRoute] string auctionId)
        {
            var userId = User?.Identity?.GetUserId();

            var auction = _auctionService.GetAuction(auctionId);
            var deletedAuction = await _auctionService.DeleteAuction(auctionId, userId);



            var imagePath = Path.Combine(_hostEnvironment.WebRootPath, "Image", auction.ImageName);

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }


            if (deletedAuction)
            {
                TempData["Success"] = "Auction Deleted";
            }
            else
            {
                TempData["Error"] = "Auction delete failed";
            }

            return Redirect("/auction/user");
        }

        [HttpGet("/auction/category")]
        public ActionResult Categories()
        {
            var result = _auctionService.GetAllCategories();
            ViewData["categories"] = result;

            return View("Categories");
        }

        [HttpPost("/auction/category/create")]
        public async Task<ActionResult> CreateCategory([FromForm] CategoryModel category)
        {
            var result = _auctionService.GetAllCategories();
            ViewData["categories"] = result;

            if (!ModelState.IsValid)
            {
                return View("Categories", category);
            }
            var newCategory = await _auctionService.CreateCategory(category);

            if (newCategory)
            {
                TempData["Success"] = "Category created";
                return Redirect("/auction/category");
            }

            TempData["Error"] = "Category creating failed";
            return View("Categories");
        }

        [HttpPost("/auction/category/{categoryId}")]
        public async Task<ActionResult> DeleteCategory([FromRoute]string categoryId)
        {
            var deletedCategory = await _auctionService.DeleteCategory(categoryId);

            var result = _auctionService.GetAllCategories();
            ViewData["categories"] = result;

            if (deletedCategory)
            {
                TempData["Success"] = "Category deleted";
                return Redirect("/auction/category");
            }

            TempData["Error"] = "Category deleting failed";
            return View("Categories");
        }

        public async Task<string> SaveImage(ItemDto dto)
        {
            try
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string fileName = Path.GetFileNameWithoutExtension(dto.ImageFile.FileName);
                string extension = Path.GetExtension(dto.ImageFile.FileName);
                fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                string path = Path.Combine(wwwRootPath + "/Image/" + fileName);

                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(fileStream);
                }

                return fileName;
            }catch (Exception ex)
            {
                return "";
            }
            
        }
    }
}
