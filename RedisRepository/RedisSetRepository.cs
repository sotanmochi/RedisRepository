using System;
using StackExchange.Redis;
using RedisRepository.Converter;

namespace RedisSampleApp
{
    public class RedisSetRepository<T>
    {
        private readonly ConnectionMultiplexer _connection;
        private readonly IDatabase _database;
        private readonly IValueConverter<T> _converter;

        public RedisSetRepository(ConnectionMultiplexer connection, int db)
        {
            _connection = connection;
            _database = _connection.GetDatabase(db);

            if (typeof(T) == typeof(string))
            {
                _converter = (IValueConverter<T>)new StringConverter();
            }
            else
            {
                _converter = new JsonConverter<T>();
            }
        }

        public IEnumerable<T> FindAllOrderByRank(string key, long start = 0, long stop = -1, bool descending = false)
        {
            try
            {
                var order = descending ? Order.Descending : Order.Ascending;

                var redisValues = _database.SortedSetRangeByRank(key, start, stop, order);

                var values = redisValues.Where(redisValue => redisValue.HasValue)
                                        .Select(redisValue => 
                                        {
                                            try
                                            {
                                                return _converter.Deserialize(redisValue);
                                            }
                                            catch (Exception e)
                                            {
                                                return default(T);
                                            }
                                        })
                                        .Where(value => value != null);

                return values;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public bool Add(string key, T value, double score)
        {
            try
            {
                var member = _converter.Serialize(value);
                return _database.SortedSetAdd(key, member, score);
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool Remove(string key, T value)
        {
            try
            {
                var member = _converter.Serialize(value);
                return _database.SortedSetRemove(key, member);
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}