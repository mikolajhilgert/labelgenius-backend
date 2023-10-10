using MongoDB.Driver;
using userservice.Models;
using userservice.Dto;

namespace userservice.Repositories
{
    public interface IUserRepository
    {
        IMongoCollection<User> GetUserCollection();
        Task<User> GetUserByEmail(string userEmail);
        Task<Tuple<bool, string>> SaveUser(UserRegisterDto userDto, string firebaseId);
        Task<bool> DeleteUser(string userEmail);
    }
}
