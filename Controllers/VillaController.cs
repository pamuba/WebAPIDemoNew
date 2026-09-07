using System.Collections;
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
	//[Authorize(Roles = "Customer, Admin")]
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
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaDTO>>>> GetVillas()
        {
			var villas = await _db.Villas.ToListAsync();
			var dtoResponse = _mapper.Map<List<VillaDTO>>(villas);
			var response = ApiResponse<IEnumerable<VillaDTO>>.OK(dtoResponse, "Villas retrieved Successfully");
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
