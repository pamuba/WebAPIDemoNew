using System.Collections;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBAPI_DTOS.DTO;
using WebAPIDemoNew.Data;
using WebAPIDemoNew.Models;

namespace WebAPIDemoNew.Controllers
{
    [Route("api/[controller]")]
    //[ApiController]
	[Authorize(Roles = "Customer, Admin")]
	public class VillaController : ControllerBase
    {
        private readonly ApplicationDBContext _db;
        private readonly IMapper _mapper;

		public VillaController(ApplicationDBContext db, IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
		}

		[HttpGet]
		//[Authorize(Roles = "Admin")]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDTO>>),StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDTO>>),StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaDTO>>>> GetVillas([FromQuery]string? filterBy,
			[FromQuery]string?filterQuery, [FromQuery]string? sortBy, [FromQuery]string? sortOrder="asc",
			[FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
			if (page < 1) page = 1;
			if (pageSize < 1) pageSize = 10;
			if (pageSize > 100) pageSize = 100;

			var villaQuery = _db.Villas.AsQueryable();
			if (!string.IsNullOrEmpty(filterQuery) && !string.IsNullOrEmpty(filterBy)) {
				switch (filterBy.ToLower()) {
					case "name":
						villaQuery = villaQuery.Where(u => u.Name.ToLower().Contains(filterQuery.ToLower()));
						break;

					case "details":
						villaQuery = villaQuery.Where(u => u.Details.ToLower().Contains(filterQuery.ToLower()));
						break;

					case "rate":
						if (double.TryParse(filterQuery, out double rate)) {
							villaQuery = villaQuery.Where(u => u.Rate == rate);
						}
						break;

					case "minrate":
						if (double.TryParse(filterQuery, out double minrate))
						{
							villaQuery = villaQuery.Where(u => u.Rate >= minrate);
						}
						break;
					case "maxrate":
						if (double.TryParse(filterQuery, out double maxrate))
						{
							villaQuery = villaQuery.Where(u => u.Rate <= maxrate);
						}
						break;
					case "occupancy":
						if (double.TryParse(filterQuery, out double occupancy))
						{
							villaQuery = villaQuery.Where(u => u.Occupancy == occupancy);
						}
						break;
				}
			}

			//implement the sorting code 
			if (!string.IsNullOrEmpty(sortBy))
			{
				var isDescending = sortOrder?.ToLower() == "desc";
				villaQuery = sortBy.ToLower() switch
				{
					"name" => isDescending ? villaQuery.OrderByDescending(u => u.Name)
										   : villaQuery.OrderBy(u => u.Name),
					"rate" => isDescending ? villaQuery.OrderByDescending(u => u.Rate)
										   : villaQuery.OrderBy(u => u.Rate),
					"occupancy" => isDescending ? villaQuery.OrderByDescending(u => u.Occupancy)
										   : villaQuery.OrderBy(u => u.Occupancy),
					"sqft" => isDescending ? villaQuery.OrderByDescending(u => u.Sqft)
										   : villaQuery.OrderBy(u => u.Sqft),
					"id" => isDescending ? villaQuery.OrderByDescending(u => u.Id)
										   : villaQuery.OrderBy(u => u.Id)

				};
			}
			else {
				villaQuery = villaQuery.OrderBy(u => u.Id);
			}

			//page 5 pageSize 10, skip = 40
			int skip = (page - 1) * pageSize;

			var totalCount = await villaQuery.CountAsync(); //5.4, pageSize:10
			var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

			var villas = await villaQuery.Skip(skip).Take(pageSize).ToListAsync();
			var dtoResponse = _mapper.Map<List<VillaDTO>>(villas);
			

			var messageBuilder = new StringBuilder();
			messageBuilder.Append($"Successfully retrieved {dtoResponse.Count} villa(s).");
			messageBuilder.Append($" (Page {page} of {totalPages}), {totalCount} total records.");

			if (!string.IsNullOrEmpty(filterQuery) && !string.IsNullOrEmpty(filterBy)) { 
				messageBuilder.Append($" Filtered by - '{filterBy}' : '{filterQuery}'");
			}
			if (!string.IsNullOrEmpty(sortBy))
			{
				messageBuilder.Append($" Sorted by by '{sortBy}' : '{sortOrder?.ToLower() ?? "asc"}'");
			}

			Response.Headers.Append("X-Pageination-CurreentPage", page.ToString());
			Response.Headers.Append("X-Pageination-PageSize", pageSize.ToString());
			Response.Headers.Append("X-Pageination-TotalCount", totalCount.ToString());
			Response.Headers.Append("X-Pageination-TotalPages", totalPages.ToString());

			var response = ApiResponse<IEnumerable<VillaDTO>>.OK(dtoResponse, messageBuilder.ToString());
			return Ok(response);
        }
        [HttpGet("{id:int}")]
		[ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<ApiResponse<VillaDTO>>> GetVillaById(int id)
        {
            try
            {
				if (id < 0) {
					return NotFound(ApiResponse<object>.NotFound("Villa ID must be greater than 0"));
				}
				var villa = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
				if (villa == null)
					return NotFound(ApiResponse<object>.NotFound($"Villa with {id} was not found"));

				return Ok(ApiResponse<VillaDTO>.OK(_mapper.Map<VillaDTO>(villa), "Records retrieved successfully"));

			}
			catch (Exception ex){
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occured while retirieving villa with ID {id}:{ex.Message}");
            }
        }
        [HttpPost]
		[ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status409Conflict)]
		[ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<VillaDTO>>> CreateVilla([FromBody]CreateVillaDTO villaDTO)
		{
			try
			{
				if (villaDTO == null)
					return BadRequest(ApiResponse<object>.BadRequest($"Villa data is required."));

				var duplicateVilla = await _db.Villas.FirstOrDefaultAsync(u => u.Name.ToLower() == villaDTO.Name.ToLower());

				if (duplicateVilla != null)
				{
					return Conflict(ApiResponse<object>.Conflict($"A villa with the same name '{villaDTO.Name}' already exists"));
				}

				Villa villa = _mapper.Map<Villa>(villaDTO); 

                await _db.Villas.AddAsync(villa);
                await _db.SaveChangesAsync();
				//return Ok(villaDTO);
				var response = ApiResponse<VillaDTO>.CreatedAt(_mapper.Map<VillaDTO>(villa), "Villa created successfully");
                return CreatedAtAction(nameof(CreateVilla), new { id = villa.Id }, response);

			}
			catch (Exception ex)
			{
				var errorResponse = ApiResponse<object>.Error(500, $"An error occured while creating the villa:{ex.Message}");
				return StatusCode(500, errorResponse);
				//return StatusCode(StatusCodes.Status500InternalServerError,
				//	$"An error occured while creating the villa:{ex.Message}");
			}
		}

		[HttpPut("{id:int}")]
		[ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status409Conflict)]
		[ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<ApiResponse<VillaDTO>>> UpdateVilla(int id, [FromBody] UpdateVillaDTO villaDTO)
		{
			try
			{
				if (villaDTO == null)
					return BadRequest(ApiResponse<object>.BadRequest($"Villa data object is required."));

				if(id != villaDTO.Id)
					return BadRequest(ApiResponse<object>.BadRequest($"Villa ID in the URL doesnt match with Villa ID in request body."));

				var existingVilla = await _db.Villas.FirstOrDefaultAsync(u=>u.Id == id);
				
				if (existingVilla == null) {
					return NotFound(ApiResponse<object>.NotFound($"Villa with ID {id} was not found."));
				}

				var duplicateVilla = await _db.Villas.FirstOrDefaultAsync(u => u.Name.ToLower() == villaDTO.Name.ToLower() 
				&& u.Id != id);

				if (duplicateVilla != null) {
					return Conflict(ApiResponse<object>.Conflict($"A villa with the same name '{villaDTO.Name}' already exists"));
				}

				_mapper.Map(villaDTO,existingVilla);
				existingVilla.UpdatedDate = DateTime.Now;

				await _db.SaveChangesAsync();
				var response = ApiResponse<VillaDTO>.OK(_mapper.Map<VillaDTO>(villaDTO), "Villa Updated Successfully");
				return Ok(response);

			}
			catch (Exception ex)
			{
				var errorResponse = ApiResponse<object>.Error(500, $"An error occured while updating the villa:{ex.Message}");
				//return StatusCode(StatusCodes.Status500InternalServerError,
				//	$"An error occured while updating the villa:{ex.Message}");
				return StatusCode(500, errorResponse);
			}
		}
		[HttpDelete("{id:int}")]
		[ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<object>>> DeleteVilla(int id)
		{
			try
			{
				if (id <= 0)
					return BadRequest(ApiResponse<object>.BadRequest($"Villa ID must be grater than 0."));
				
				var existingVilla = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
				
				if (existingVilla == null)
				{
					return NotFound(ApiResponse<object>.NotFound($"Villa with ID {id} was not found."));
				}
				_db.Villas.Remove(existingVilla);
				await _db.SaveChangesAsync();
				var response = ApiResponse<object>.NoContent("Villa Deleted Successfully");
				return Ok(response);

			}
			catch (Exception ex)
			{
				//return StatusCode(StatusCodes.Status500InternalServerError,
				//	$"An error occured while deleting the villa:{ex.Message}");
				var errorResponse = ApiResponse<object>.Error(500, $"An error occured while deleting the villa:{ex.Message}");
				return StatusCode(500, errorResponse);

			}
		}

	}
}
