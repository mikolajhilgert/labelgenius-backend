using MongoDB.Driver;
using userservice.Dto;
using userservice.Models;

namespace userservice.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;
        private readonly string _userCollectionName = "Users";

        public UserRepository(IConfiguration config, IMongoClient mongoClient)
        {
            var mongoDB = mongoClient.GetDatabase(config.GetSection("MongoDbSettings:DatabaseName").Value);
            _users = mongoDB.GetCollection<User>(_userCollectionName);
        }
        public IMongoCollection<User> GetUserCollection()
        {
            return _users;
        }
        public Task<User> GetUserByEmail(string userEmail)
        {
            return _users.Find(user => user.Email == userEmail).FirstOrDefaultAsync();
        }
        public Task<bool> DeleteUser(string userEmail)
        {
            return _users.DeleteOneAsync(user => user.Email == userEmail).ContinueWith(task => task.Result.DeletedCount == 1);
        }
        public async Task<(bool Result, string Message)> SaveUser(UserRegisterDto userDto, string firebaseId)
        {
            // Check if user already exists
            var existingUser = await _users.Find(user => user.Email == userDto.Email).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return (false, "A user with this email already exists. Try to log-in instead");
            }
            User user = new()
            {
                Id = firebaseId,
                Email = userDto.Email,
                Name = userDto.Name
            };
            await _users.InsertOneAsync(user);
            return (true, "Registration successful, please verify your email!");
        }
    }
}
