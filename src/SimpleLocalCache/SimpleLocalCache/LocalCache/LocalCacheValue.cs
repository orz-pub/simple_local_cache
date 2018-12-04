using System;
using System.Threading.Tasks;

namespace SimpleLocalCache
{
    // LocalCache 로 캐시하려면 상속받아 구현.
    public abstract class LocalCacheable
    {
        public static Int32 MaxAgeSec { get; set; } = 60;   // 캐시수명(static이 낫다고 판단).
        public bool IsUpdateEnd { get; set; } = false;      // Update 함수가 끝났다는 것이지 데이터가 온전함과 무관.
        public bool IsUpdateSuccess { get; set; } = false;  // Update 함수가 예외없이 끝나서 데이터가 온전함.
        public DateTime ExpireDateTime { get; set; }        // 캐시만료 시간.

        public abstract Task UpdateSelf();                  // 스스로를 업데이트 해야한다.
        public abstract void SetMaxAgeSec();                // 캐시수명 설정.
    }
}
