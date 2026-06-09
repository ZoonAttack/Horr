using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTOs.Category;

namespace Horr.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Retrieves all categories.
        /// </summary>
        /// <returns>A list of all categories.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAllCategoriesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a category by its ID.
        /// </summary>
        /// <param name="id">The ID of the category.</param>
        /// <returns>The category details.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            if (!result.Succeeded) return NotFound(result);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new category. Only accessible by Admins.
        /// </summary>
        /// <param name="dto">The details of the category to create.</param>
        /// <returns>The created category details.</returns>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            var result = await _categoryService.CreateCategoryAsync(dto);
            if (!result.Succeeded) return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
        }

        /// <summary>
        /// Updates an existing category. Only accessible by Admins.
        /// </summary>
        /// <param name="id">The ID of the category to update.</param>
        /// <param name="dto">The updated details of the category.</param>
        /// <returns>The updated category details.</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateCategoryDto dto)
        {
            var result = await _categoryService.UpdateCategoryAsync(id, dto);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a category by its ID. Only accessible by Admins.
        /// </summary>
        /// <param name="id">The ID of the category to delete.</param>
        /// <returns>A success result indicating the category was deleted.</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }
    }
}
