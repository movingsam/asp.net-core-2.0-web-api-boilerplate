﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Features.Common;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesApi.Models.Retail;
using SalesApi.Repositories.Retail;
using SalesApi.Services.Retail;
using SalesApi.ViewModels.Retail;
using SalesApi.Web.Controllers.Bases;

namespace SalesApi.Web.Controllers.Retail
{
    [Route("api/sales/[controller]")]
    public class RetailerController : RetailController<RetailerController>
    {
        private readonly IRetailerRepository _retailerRepository;
        public RetailerController(IRetailService<RetailerController> retailService,
            IRetailerRepository retailerRepository) : base(retailService)
        {
            _retailerRepository = retailerRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _retailerRepository.All.ToListAsync();
            var results = Mapper.Map<IEnumerable<RetailerViewModel>>(items);
            return Ok(results);
        }

        [HttpGet]
        [Route("{id}", Name = "GetRetailer")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _retailerRepository.GetSingleAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            var result = Mapper.Map<RetailerViewModel>(item);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RetailerViewModel retailerVm)
        {
            if (retailerVm == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newItem = Mapper.Map<Retailer>(retailerVm);
            _retailerRepository.SetPinyin(newItem);
            newItem.SetCreation(UserName);
            _retailerRepository.Add(newItem);
            if (!await UnitOfWork.SaveAsync())
            {
                return StatusCode(500, "保存时出错");
            }

            var vm = Mapper.Map<RetailerViewModel>(newItem);

            return CreatedAtRoute("GetRetailer", new { id = vm.Id }, vm);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] RetailerViewModel retailerVm)
        {
            if (retailerVm == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var dbItem = await _retailerRepository.GetSingleAsync(id);
            if (dbItem == null)
            {
                return NotFound();
            }
            Mapper.Map(retailerVm, dbItem);
            _retailerRepository.SetPinyin(dbItem);
            dbItem.SetModification(UserName);
            _retailerRepository.Update(dbItem);
            if (!await UnitOfWork.SaveAsync())
            {
                return StatusCode(500, "保存时出错");
            }
            var vm = Mapper.Map<RetailerViewModel>(dbItem);
            return Ok(vm);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<RetailerViewModel> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }
            var dbItem = await _retailerRepository.GetSingleAsync(id);
            if (dbItem == null)
            {
                return NotFound();
            }
            var toPatchVm = Mapper.Map<RetailerViewModel>(dbItem);
            patchDoc.ApplyTo(toPatchVm, ModelState);

            TryValidateModel(toPatchVm);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Mapper.Map(toPatchVm, dbItem);

            if (!await UnitOfWork.SaveAsync())
            {
                return StatusCode(500, "更新时出错");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _retailerRepository.GetSingleAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            _retailerRepository.Delete(model);
            if (!await UnitOfWork.SaveAsync())
            {
                return StatusCode(500, "删除时出错");
            }
            return NoContent();
        }

        [HttpGet]
        [Route("ByDeliveryVehicle/{deliveryVehicleId}")]
        public async Task<IActionResult> GetByDeliveryVehicle(int deliveryVehicleId)
        {
            var items = await _retailerRepository.All.Where(x => x.SubArea.DeliveryVehicleId == deliveryVehicleId).ToListAsync();
            var results = Mapper.Map<IEnumerable<RetailerViewModel>>(items);
            return Ok(results);
        }

        [HttpGet]
        [Route("BySubArea/{subAreaId}")]
        public async Task<IActionResult> GetBySubArea(int subAreaId)
        {
            var items = await _retailerRepository.All.Where(x => x.SubAreaId == subAreaId).ToListAsync();
            var results = Mapper.Map<IEnumerable<RetailerViewModel>>(items);
            return Ok(results);
        }

        [HttpGet]
        [Route("NotDeleted")]
        public async Task<IActionResult> GetNotDeleted()
        {
            var items = await _retailerRepository.All.Where(x => !x.Deleted).ToListAsync();
            var results = Mapper.Map<IEnumerable<RetailerViewModel>>(items);
            return Ok(results);
        }

    }
}
