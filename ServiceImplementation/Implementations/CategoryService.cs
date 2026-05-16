using Entities;
using Entities.Project;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTOs.Category;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _db;

        public CategoryService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Result<List<CategoryDto>>> GetAllCategoriesAsync()
        {
            var categories = await _db.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IconUrl = c.IconUrl
                })
                .ToListAsync();

            return new Result<List<CategoryDto>> { Succeeded = true, Data = categories };
        }

        public async Task<Result<CategoryDto>> GetCategoryByIdAsync(string id)
        {
            var category = await _db.Categories
                .Where(c => c.Id == id)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IconUrl = c.IconUrl
                })
                .FirstOrDefaultAsync();

            if (category == null)
                return new Result<CategoryDto> { Succeeded = false, ErrorCode = ErrorCodes.CategoryNotFound, Message = "Category not found." };

            return new Result<CategoryDto> { Succeeded = true, Data = category };
        }

        public async Task<Result<CategoryDto>> CreateCategoryAsync(CreateCategoryDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                IconUrl = dto.IconUrl
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            return new Result<CategoryDto> { Succeeded = true, Data = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IconUrl = category.IconUrl
            }};
        }

        public async Task<Result<CategoryDto>> UpdateCategoryAsync(string id, UpdateCategoryDto dto)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
                return new Result<CategoryDto> { Succeeded = false, ErrorCode = ErrorCodes.CategoryNotFound, Message = "Category not found." };

            if (!string.IsNullOrEmpty(dto.Name)) category.Name = dto.Name;
            if (dto.Description != null) category.Description = dto.Description;
            if (dto.IconUrl != null) category.IconUrl = dto.IconUrl;

            await _db.SaveChangesAsync();

            return new Result<CategoryDto> { Succeeded = true, Data = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IconUrl = category.IconUrl
            }};
        }

        public async Task<Result<bool>> DeleteCategoryAsync(string id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
                return new Result<bool> { Succeeded = false, ErrorCode = ErrorCodes.CategoryNotFound, Message = "Category not found." };

            // Check if any jobs or skills are using this category
            if (await _db.JobPosts.AnyAsync(j => j.CategoryId == id) || await _db.Skills.AnyAsync(s => s.CategoryId == id))
                return new Result<bool> { Succeeded = false, ErrorCode = ErrorCodes.CategoryAlreadyExists, Message = "Category is in use and cannot be deleted." };

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            return new Result<bool> { Succeeded = true, Data = true };
        }
    }
}
