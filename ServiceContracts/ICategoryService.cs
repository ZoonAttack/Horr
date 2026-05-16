using ServiceContracts.DTOs.Category;
using ServiceContracts.DTOs.Responses;

namespace ServiceContracts
{
    public interface ICategoryService
    {
        Task<Result<List<CategoryDto>>> GetAllCategoriesAsync();
        Task<Result<CategoryDto>> GetCategoryByIdAsync(string id);
        Task<Result<CategoryDto>> CreateCategoryAsync(CreateCategoryDto dto);
        Task<Result<CategoryDto>> UpdateCategoryAsync(string id, UpdateCategoryDto dto);
        Task<Result<bool>> DeleteCategoryAsync(string id);
    }
}
