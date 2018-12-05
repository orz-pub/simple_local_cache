using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleLocalCache
{
    // 이 객체의 값이 자주 필요한데 얻는 작업에 부하가 있으며, 실시간 동기화는 하지 않아도 된다고 하자.
    class BTCPrice : LocalCacheable
    {
        public Double Price;

        // 캐시 수명.
        public override void SetMaxAgeSec()
        {
            MaxAgeSec = 10;
        }

        // 부하가 있는 작업.
        public override async Task UpdateSelf()
        {
            Console.WriteLine("START UPDATE-SELF");

            var httpClient = new HttpClient();
            Task<String> task = httpClient.GetStringAsync("https://api.bithumb.com/public/ticker/btc");

            // 일정한 확률로 예외 발생.
            var random = new Random((Int32)DateTime.Now.Ticks);
            if (random.Next(0, 100) < 50)
            {
                Console.WriteLine("FAIL UPDATE-SELF");
                throw new Exception();
            }

            // 업데이트에 3초 정도 걸리도록 함.
            await Task.Delay(3000);
            String result = await task;

            // 테스트 코드이므로 문자열 처리.
            var regex = new Regex(@"buy_price\D+(\d+)");
            Match match = regex.Match(result);

            Price = Double.Parse(match.Groups[1].Value);

            Console.WriteLine("SUCCESS UPDATE-SELF");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var localCache = new LocalCache();
            Test(localCache).Wait();
        }

        static async Task Test(LocalCache localCache)
        {
            const String CACHE_KEY = "btcprice";

            while (true)
            {
                // 첫 번째 요청이 끝나기 전에 두 번째 요청이 들어가는 것 테스트.
                var task1 = localCache.Get<BTCPrice>(CACHE_KEY);
                var task2 = localCache.Get<BTCPrice>(CACHE_KEY);

                var btcPrice1 = await task1;
                var btcPrice2 = await task2;

                Console.WriteLine(btcPrice1.IsUpdateSuccess ? $"PRICE: {btcPrice1.Price}" : "FAIL GET PRICE");

                Thread.Sleep(1000);
            }
        }
    }
}
