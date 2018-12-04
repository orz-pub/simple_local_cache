/*
	다음 조건을 만족하는 경우 LocalCache 사용을 고려해 본다.
		- 데이터 요청이 매우 잦다.
		- 데이터를 가져오는데 부하가 있다(디비, 웹요청 등).
		- 데이터가 원본과 실시간 동기화되지 않아도 괜찮다(딜레이 허용).
*/

using System;
using System.Threading.Tasks;

namespace SimpleLocalCache
{
    public class LocalCache
    {
        private const Int32 DELAY_UPDATE_END_MS = 50;

        private readonly KeyValueStore<String, LocalCacheable> _keyValueStore = new KeyValueStore<String, LocalCacheable>();

        public async Task<T> Get<T>(String key) where T : LocalCacheable, new()
        {
            if (_keyValueStore.TryGetValue(key, out var value))
            {
                if (DateTime.Now < value.ExpireDateTime)
                {
                    return (T)value;    // 캐시가 존재하고 유효.
                }
                else
                {
                    if (value.IsUpdateEnd)
                    {
                        return await Update((T)value);  // 캐시가 존재하고 유효하지만 기간이 만료되어 업데이트.
                    }
                    else
                    {
                        while (value.IsUpdateEnd == false)
                        {
                            await Task.Delay(DELAY_UPDATE_END_MS);
                        }

                        return (T)value;     // 캐시가 존재하지만 유효하지 않은 경우, 다른 task의 업데이트를 기다림.
                    }
                }
            }
            else
            {
                // 캐시가 존재한적 없음.
                var newValue = new T();
                newValue.SetMaxAgeSec();

                // Update 하기 전에 저장해야 await 중에 들어오는 요청이 새 객체를 만들지 않고 완성되기를 기다리게 된다.
                _keyValueStore.Add(key, newValue);

                return await Update(newValue);
            }
        }

        private async Task<T> Update<T>(T value) where T : LocalCacheable, new()
        {
            try
            {
                value.IsUpdateEnd = false;
                value.IsUpdateSuccess = false;

                await value.UpdateSelf();

                // UpdateSelf에서 예외가 발생하면 만료시간이 갱신되지 않기 때문에 다음 Get에서는 바로 재시도한다.
                value.ExpireDateTime = DateTime.Now.AddSeconds(LocalCacheable.MaxAgeSec);
                value.IsUpdateSuccess = true;
            }
            catch (Exception)
            {
                // UpdateSelf의 구현에 따라서 데이터가 온전할 수도 있고 아닐 수도 있다.
                // 여기서 이것을 보장하려면, value를 복제했다가 실패하는 경우에 롤백 해야 한다.
                // 하지만 깊은 복사를 강제해야 하고, 복사 오버헤드 문제도 있다.
                // 따라서 UpdateSelf를 구현하는 쪽에서 예외 처리를 하도록 하는 것이 낫다고 판단하였다.
                // 대신 IsUpdateSuccess로 성공 여부를 알 수 있게 한다.
                value.IsUpdateSuccess = false;
            }
            finally
            {
                value.IsUpdateEnd = true;
            }

            return value;
        }
    }
}
