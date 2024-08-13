using DAIKIN.CheckSheetPortal.DTO;
using DAIKIN.CheckSheetPortal.Entities;

namespace DAIKIN.CheckSheetPortal.Infrastructure.Services
{
    public interface IUserService : IBaseService<User>
    {
        Task<UserDTO> GetByEmailAsync(string email);
        Task<IOperationResponse> CreateAsync(UserDTO model, UserDTO loggedinuser);
        Task<IOperationResponse> UpdateAsync(string id, UserDTO model, UserDTO loggedinuser);
        Task<IOperationResponse> UpdatePasswordAsync(string id, string newPassword, string confirmPassword);
    }
}
