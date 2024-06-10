using System;
using AuctionService.Data;
using AuctionService.Dtos;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;


[ApiController]
[Route("api/auctions")]
public class AuctionController : ControllerBase
{

    private readonly AuctionDbContext _auctionDbContext;
    private readonly IMapper _mapper;
    public AuctionController(AuctionDbContext auctionDbContext, IMapper mapper)
    {
        this._auctionDbContext = auctionDbContext;
        this._mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        var query = _auctionDbContext.Auctions.OrderBy(x => x.Item.Make).AsQueryable();
        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }
        // var auctions = await _auctionDbContext.Auctions
        //                 .Include(x => x.Item).OrderBy(x => x.Item.Make).ToListAsync();

        // return _mapper.Map<List<AuctionDto>>(auctions);
        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auctions = await _auctionDbContext.Auctions
        .Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
        if (auctions == null)
        {
            return NotFound();
        }
        return _mapper.Map<AuctionDto>(auctions);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        // TODO Add CURRENT USER
        auction.Seller = "test";
        _auctionDbContext.Auctions.Add(auction);

        var result = await _auctionDbContext.SaveChangesAsync() > 0;
        if (!result) return BadRequest("Could not save to DB");
        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, _mapper.Map<AuctionDto>(auction));

    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _auctionDbContext.Auctions.Include(x => x.Item).FirstOrDefaultAsync(y => y.Id == id);
        if (auction == null) return NotFound();

        // TODO : CHECK AUTH

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;

        var result = await _auctionDbContext.SaveChangesAsync() > 0;
        if (result) return Ok();
        return BadRequest("Something wrong");
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _auctionDbContext.Auctions.FindAsync(id);
        if (auction == null) return NotFound();
        // TODO : CHECK SEller
        _auctionDbContext.Auctions.Remove(auction);
        var result = await _auctionDbContext.SaveChangesAsync() > 0;
        if (!result) return BadRequest("Could not update DB");
        return Ok();

    }

}
