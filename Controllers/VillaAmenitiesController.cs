using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebAPIDemoNew.Models;
using WebAPIDemoNew.Data;
using Microsoft.EntityFrameworkCore;
using WEBAPI_DTOS.DTO;

namespace WebAPIDemoNew.Controllers
{
    [Route("api/villa-maenities")]
	[ApiController]
	public class VillaAmenitiesController : ControllerBase
	{
		private readonly IMapper _mapper;
		private readonly ApplicationDBContext _db;
		public VillaAmenitiesController(ApplicationDBContext db, IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
		}
		[HttpGet]
		//[Authorize(Roles = "Admin")]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<VIllaAmenetiesDTO>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<ApiResponse<IEnumerable<VIllaAmenetiesDTO>>>> GetVillaAmenitiess()
		{
			var villas = await _db.VillaAmenities.ToListAsync();
			var responseDto = _mapper.Map<List<VIllaAmenetiesDTO>>(villas);
			var response = ApiResponse<IEnumerable<VIllaAmenetiesDTO>>.OK(responseDto, "Villas Amenities retrived successfully");
			return Ok(response);
		}
		[HttpGet("{id:int}")]
		//[Authorize(Roles = "Customer, Admin")]
		//[AllowAnonymous]
		[ProducesResponseType(typeof(ApiResponse<VIllaAmenetiesDTO>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<ApiResponse<VIllaAmenetiesDTO>>> GetVillaAmenitiesById(int id)
		{
			try
			{
				if (id <= 0)
				{
					return NotFound(ApiResponse<object>.NotFound($"VillaAmenities ID must be greater than 0."));
				}
				var villaAmenities = await _db.VillaAmenities.FirstOrDefaultAsync(x => x.Id == id);
				if (villaAmenities == null)
				{
					//return NotFound($"VillaAmenities with ID {id} was not found.");
					return NotFound(ApiResponse<object>.NotFound($"VillaAmenities {id} must be greater than 0."));
				}
				return Ok(ApiResponse<VIllaAmenetiesDTO>.OK(_mapper.Map<VIllaAmenetiesDTO>(villaAmenities), "Records retrived succesfully"));
			}
			catch (Exception e)
			{
				var errorMessage = ApiResponse<object>.Error(500, "", e.Message);
				return StatusCode(500, errorMessage);
			}
		}
		[HttpPost]
		[ProducesResponseType(typeof(ApiResponse<VIllaAmenetiesDTO>), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<VIllaAmenetiesDTO>> CreateVillaAmenities(VIllaAmenetiesDTO villaAmenitiesDTO)
		{
			try
			{
				if (villaAmenitiesDTO == null)
				{
					return BadRequest(ApiResponse<object>.BadRequest("VillaAmenities object is required"));
				}

				var villaExists = await _db.Villas.FirstOrDefaultAsync(u => u.Id == villaAmenitiesDTO.VillaId);
				if (villaExists == null)
				{
					return Conflict(ApiResponse<object>.Conflict($"Villa with ID '{villaAmenitiesDTO.VillaId}'does not exists"));
				}
				VillaAmenities villaAmenities = _mapper.Map<VillaAmenities>(villaAmenitiesDTO);
				villaAmenities.CreatedDate = DateTime.Now;
				await _db.VillaAmenities.AddAsync(villaAmenities);
				await _db.SaveChangesAsync();
				//return Ok(villaDTO);
				var response = ApiResponse<VIllaAmenetiesDTO>.CreatedAt(_mapper.Map<VIllaAmenetiesDTO>(villaAmenities), "VillaAmenities Created Successfully");

				return CreatedAtAction(nameof(CreateVillaAmenities), new { id = villaAmenities.Id }, response);
			}
			catch (Exception e)
			{
				var errorMessage = ApiResponse<object>.Error(500,
					"An error cooured while creating the Villa Amenities", e.Message);
				return StatusCode(500, errorMessage);
			}
		}
		[HttpPut("{id:int}")]
		[ProducesResponseType(typeof(ApiResponse<VIllaAmenetiesDTO>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<VIllaAmenetiesDTO>>> UpdateVillaAmenities(int id, VillaAmenitiesUpdateDTO villaAmenitiesDTO)
		{
			try
			{
				if (villaAmenitiesDTO == null)
				{
					return BadRequest(ApiResponse<object>.BadRequest("VillaAmenities object is required"));
				}
				if (id != villaAmenitiesDTO.Id)
				{
					return BadRequest(ApiResponse<object>.BadRequest("VillaAmenities ID in the URL dont match VillaAmenities ID in the request body"));
					//return BadRequest("VillaAmenities ID in the URL dont match VillaAmenities ID in the request body");
				}
				var villaExists = await _db.Villas.FirstOrDefaultAsync(u => u.Id == villaAmenitiesDTO.VillaId);
				if (villaExists == null)
				{
					return Conflict(ApiResponse<object>.Conflict($"Villa Amenities with ID '{villaAmenitiesDTO.VillaId}'does not exists"));
				}

				var existingVillaAmenities = await _db.VillaAmenities.FirstOrDefaultAsync(u => u.Id == id);
				if (existingVillaAmenities == null)
				{
					//return NotFound($"VillaAmenities with ID {id} was not found");
					return NotFound(ApiResponse<object>.NotFound($"Villa Amenities with ID {id} was not found"));

				}

				//Rate = 20, Occupancy = 4
				_mapper.Map(villaAmenitiesDTO, existingVillaAmenities);
				existingVillaAmenities.UpdatedDate = DateTime.Now;
				await _db.SaveChangesAsync();
				var response = ApiResponse<VIllaAmenetiesDTO>.OK(_mapper.Map<VIllaAmenetiesDTO>(existingVillaAmenities), "Villa Amenities Update Succesfully");
				return Ok(response);
			}
			catch (Exception e)
			{
				var errorMessage = ApiResponse<object>.Error(500, "", e.Message);
				return StatusCode(500, errorMessage);
			}
		}
		[HttpDelete("{id:int}")]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<ApiResponse<object>>> DeleteVillaAmenities(int id)
		{
			try
			{
				var existingVillaAmenities = await _db.VillaAmenities.FirstOrDefaultAsync(u => u.Id == id);
				if (existingVillaAmenities == null)
				{
					return NotFound(ApiResponse<object>.NotFound($"Villa Amenities with ID {id} was not found"));
				}
				_db.VillaAmenities.Remove(existingVillaAmenities);
				await _db.SaveChangesAsync();
				var response = ApiResponse<object>.NoContent("Villa Amenities Deleted Successfully");
				return Ok(response);
			}
			catch (Exception e)
			{
				var errorMessage = ApiResponse<object>.Error(500, "", e.Message);
				return StatusCode(500, errorMessage);
			}
		}
	}
}
