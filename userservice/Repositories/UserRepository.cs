using MongoDB.Driver;
using userservice.Dto;
using userservice.Models;

namespace userservice.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(IConfiguration config, IMongoClient mongoClient)
        {
            var mongoDB = mongoClient.GetDatabase(config.GetSection("UserDbSettings:DatabaseName").Value);
            _users = mongoDB.GetCollection<User>(config.GetSection("UserDbSettings:CollectionName").Value);
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
        public Task<Tuple<bool, string>> SaveUser(UserRegisterDto userDto, string firebaseId)

        {
            return _users.Find(user => user.Email == userDto.Email).FirstOrDefaultAsync().ContinueWith(task =>
            {
                if (task.Result != null)
                {
                    return new Tuple<bool, string>(false, "A user with this email already exist. try to log-in instead");
                }
                else
                {
                    User user = new User
                    {
                        Id = firebaseId,
                        Email = userDto.Email,
                        Name = userDto.Name

                    };
                    _users.InsertOneAsync(user);
                    return new Tuple<bool, string>(true, "Registration successful, please verify your email!");
                }
            }); 
        }
    }
}
