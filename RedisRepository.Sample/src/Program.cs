using System;
using StackExchange.Redis;
using RedisRepository;

namespace RedisSampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("[RedisSampleApp] Start");

            var connection = ConnectionMultiplexer.Connect("localhost");
            Console.WriteLine("[RedisSampleApp] Connected redis server");

            var options = new RedisRepositoryOptions()
            {
                ConnectionMultiplexer = connection,
                Db = 1,
            };

            var userRepository = new RedisValueRepository<User>(options);

            var testUser = new User()
            {
                Id = "user-12345",
                Name = "TestUser",
                Description = $"Now: {DateTime.Now}",
                OrganizationId = "org-zzz001"
            };

            var key = testUser.Id;
            userRepository.Save(testUser.Id, testUser);
            Console.WriteLine($"[RedisSampleApp] Save a user \"{key}\"");

            var result = userRepository.Find(key);
            if (result != null)
            {
                Console.WriteLine($"[RedisSampleApp] Find a user \"{key}\"");
                Console.WriteLine($"    {{");
                Console.WriteLine($"        \"{result.Id}\", ");
                Console.WriteLine($"        \"{result.Name}\", ");
                Console.WriteLine($"        \"{result.Description}\" ");
                Console.WriteLine($"        \"{result.OrganizationId}\" ");
                Console.WriteLine($"    }}");
            }

            userRepository.Delete(key);
            Console.WriteLine($"[RedisSampleApp] Delete a user \"{key}\"");

            connection.Close();
            Console.WriteLine("[RedisSampleApp] End");
        }
    }

    public class User
    {
        public string Id;
        public string Name;
        public string Description;
        public string OrganizationId;
    }
}