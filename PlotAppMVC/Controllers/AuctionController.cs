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

        public AuctionController(IAuctionService auctionService, IAccountService accountService, IMapper mapper)
        {
            _auctionService = auctionService;
            _accountService = accountService;
            _mapper = mapper;
        }

        [HttpGet("/auction")]
        public ActionResult Index([FromQuery] Query query)
        {
            var auctions = _auctionService.GetAuctions(query);

            ViewData["auctions"] = auctions;
            return View();
        }

        [HttpGet("/auction/user")]
        public ActionResult UserAuctions([FromQuery] Query query)
        {
            var userId = User?.Identity?.GetUserId();

            var auctions = _auctionService.GetAuctions(query, userId);

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
            var types = _auctionService.GetAllTypes();
            ViewData["types"] = types;
            return View();
        }

        // POST: AuctionController/Create
        [HttpPost("/auction/create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ItemDto dto)
        {
            var types = _auctionService.GetAllTypes();
            ViewData["types"] = types;

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var userId = User?.Identity?.GetUserId();

            var auction = await _auctionService.CreateAuction(dto, userId);

            if(auction)
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

            var types = _auctionService.GetAllTypes();
            ViewData["types"] = types;

            var auctionDto = _mapper.Map<ItemDto>(auction);
            return View(auctionDto);
        }

        [HttpPost("/auction/{auctionId}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([FromForm] ItemDto dto, [FromRoute] string auctionId)
        {
            dto.Id = auctionId;

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
            var deletedAuction = await _auctionService.DeleteAuction(auctionId, userId);

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

        [HttpGet("/auction/type")]
        public ActionResult Types()
        {
            var result = _auctionService.GetAllTypes();
            ViewData["types"] = result;

            return View("Types");
        }

        [HttpPost("/auction/type/create")]
        public async Task<ActionResult> CreateType([FromForm] CategoryModel type)
        {
            var result = _auctionService.GetAllTypes();
            ViewData["types"] = result;

            if (!ModelState.IsValid)
            {
                return View("Types", type);
            }
            var newType = await _auctionService.CreateType(type);

            if (newType)
            {
                TempData["Success"] = "Type created";
                return Redirect("/auction/type");
            }

            TempData["Error"] = "Type creating failed";
            return View("Types");
        }

        [HttpPost("/auction/type/{typeId}")]
        public async Task<ActionResult> DeleteType([FromRoute]string typeId)
        {
            var deletedType = await _auctionService.DeleteType(typeId);

            var result = _auctionService.GetAllTypes();
            ViewData["types"] = result;

            if (deletedType)
            {
                TempData["Success"] = "Type deleted";
                return Redirect("/auction/type");
            }

            TempData["Error"] = "Type deleting failed";
            return View("Types");
        }
    }
}
