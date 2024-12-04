using System.Threading.Tasks;
using System;
using GrpcServiceStock.Common;
using System.Collections.Generic;
using System.Linq;
using GrpcServiceStock.Response;
using static GrpcServiceStock.Enum.EnumHelper;
using System.Net.Http;
using Binance.Spot;
using Binance.Spot.Models;
using Newtonsoft.Json;
using GrpcServiceStock.ProcessIndicatorStock;

namespace GrpcServiceStock.Modules
{
    public class CoinDataStock
    {
        public static Dictionary<string, string> _dicCoin = new Dictionary<string, string>();

        private static HttpClient httpClient = new HttpClient();

        public static List<IndicatorStock> listCoinStock = new List<IndicatorStock>(); // các mã sẽ gửi room

        /// <summary>
        /// Dic các công ty không càn quét trong vòng 1 ngày
        /// </summary>
        public static Dictionary<string, DateTime> _dicCoinNotProcessStock = new Dictionary<string, DateTime>();

        /// <summary>
        /// Xử lý tìm kiếm
        /// </summary>
        /// <returns></returns>
        public static Task ProcessIndicatorDaily()
        {
            try
            {
                var market = new Market(httpClient);

                _dicCoin.ToList().ForEach(coin =>
                {
                    if (!_dicCoinNotProcessStock.ContainsKey(coin.Value))
                    {
                        var json = market.KlineCandlestickData(coin.Key, Interval.FOUR_HOUR, null, null, 1000).Result;

                        var jsonArray = JsonConvert.DeserializeObject<object[]>(json);

                        var quotes = new List<MarketDataQuote>();

                        foreach (var item in jsonArray)
                        {
                            var data = JsonConvert.DeserializeObject<List<object>>(item.ToString());

                            MarketDataQuote quote = new MarketDataQuote
                            {
                                Date = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(data[0].ToString())).DateTime.ToLocalTime(),
                                Open = decimal.Parse(data[1].ToString()),
                                High = decimal.Parse(data[2].ToString()),
                                Low = decimal.Parse(data[3].ToString()),
                                Close = decimal.Parse(data[4].ToString()),
                                Volume = decimal.Parse(data[5].ToString())
                            };

                            quotes.Add(quote);
                        }

                        var check = ProcessIndicatorCoin.ConditionStockCoin(quotes);
                        if (check)
                        {
                            ProcessIndicatorCoin.IndicatorCoin(coin.Value, quotes);

                            // add vào để thông báo 1 lần trong ngày thôi
                            CoinDataStock._dicCoinNotProcessStock.Add(coin.Value, DateTime.Now);
                        }
                    }
                });

                SendIndicator();
            }
            catch (Exception ex)
            {
                GenFileClass.CreateLogErrorEvent(string.Format("ProcessIndicatorDaily: {0}", ex.Message));
            }
            return Task.CompletedTask;
        }

        private static Task SendIndicator()
        {
            try
            {
                // Quét gửi thông báo
                if (listCoinStock != null && listCoinStock.Count > 0)
                {
                    var text = string.Empty;
                    SendTelegram.SendIndicator.Send(RoomType.Coin, StringHelper.NotifyDays(IndicatorEnum.CoinLinhLV));
                    System.Threading.Thread.Sleep(2000); // 2s
                    for (int i = 0; i < listCoinStock.Count; i++)
                    {
                        SendTelegram.SendIndicator.Send(RoomType.Coin, StringHelper.NotifyIndicator(listCoinStock[i]));
                        System.Threading.Thread.Sleep(15000); // 15s
                    }
                }
                listCoinStock.Clear();
            }
            catch (Exception ex)
            {
                GenFileClass.CreateLogErrorEvent(string.Format("SendIndicator: {0}", ex.Message));
            }
            return Task.CompletedTask;
        }
    }
}
