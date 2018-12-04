# Simple Local Cache C#

다음 조건을 만족하는 경우 LocalCache 사용을 고려해 본다
  - 데이터 요청이 잦다
  - 데이터를 가져오는데 부하가 있다(디비, 웹요청 등)
  - 데이터가 원본과 실시간 동기화되지 않아도 괜찮다(딜레이 허용)

```csharp
// 이 객체의 값이 자주 필요한데 얻는 작업에 부하가 있으며, 실시간 동기화는 하지 않아도 된다고 하자.
class MyData : LocalCacheable
{
  // 멤버 변수들...

  public override void SetMaxAgeSec()
  {
    // 캐시 수명 설정.
  }

  public override async Task UpdateSelf()
  {
    // 멤버 변수들 업데이트.
  }
}

var localCache = new LocalCache();
var myData1 = await localCache.Get<MyData>(CACHE_KEY); // UpdateSelf() 호출.
var myData2 = await localCache.Get<MyData>(CACHE_KEY); // 캐시에서 읽음.
// 시간이 지나 캐시 만료
var myData3 = await localCache.Get<MyData>(CACHE_KEY); // UpdateSelf() 호출.
```

[예제코드](./src/SimpleLocalCache/SimpleLocalCache/Program.cs)
