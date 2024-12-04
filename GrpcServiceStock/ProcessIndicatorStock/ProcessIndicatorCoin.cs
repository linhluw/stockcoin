using GrpcServiceStock.Common;
using GrpcServiceStock.Modules;
using GrpcServiceStock.Response;
using Skender.Stock.Indicators;
using SSI.FCData.Models.Response;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcServiceStock.ProcessIndicatorStock
{
    public class ProcessIndicatorCoin
    {
        /// <summary>
        /// Chỉ báo dòng tiền MFI > 50
        /// MACD Phân kỳ hội tụ trung bình động dương > 0
        /// </summary>
        /// <returns></returns>
        public static Task IndicatorCoin(string coin, List<MarketDataQuote> quotes)
        {
            var price = GetSellBuyPriceHelper.GetBuySellPrice(quotes);

            CoinDataStock.listCoinStock.Add(new IndicatorStock
            {
                Symbol = coin,

                PurchasePrice1 = PriceHelper.DoubleCoin(price.PointBuy1),

                PurchasePrice2 = PriceHelper.DoubleCoin(price.PointBuy2),

                PurchasePrice3 = PriceHelper.DoubleCoin(price.PointBuy3),

                SellingPrice1 = PriceHelper.DoubleCoin(price.PointSell1),

                SellingPrice2 = PriceHelper.DoubleCoin(price.PointSell2),

                SellingPrice3 = PriceHelper.DoubleCoin(price.PointSell3),
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Chỉ báo dòng tiền MFI > 50
        /// MACD Phân kỳ hội tụ trung bình động dương > 0
        /// </summary>
        /// <returns></returns>
        public static bool ConditionStockCoin(List<MarketDataQuote> quotes)
        {
            var value = false;
            if (quotes != null && quotes.Count > 14)
            {
                //var lookbackPeriods = 14;

                var resultsMfi = quotes.GetMfi(); // lấy chuỗi Mfi

                var resultsMfiCurrent = resultsMfi.Where(x => x.Mfi != null).LastOrDefault(); // Mfi hiện tại

                var resultsMfiPre = resultsMfi.Where(x => x.Mfi != null && x != resultsMfiCurrent).LastOrDefault(); // Mfi trước đó

                var resultsMacd = quotes.GetMacd(); // lấy chuỗi Macd

                var resultsMacdCurrent = resultsMacd.Where(x => x.Histogram != null).LastOrDefault(); // Macd hiện tại

                var resultsMacdPre = resultsMacd.Where(x => x.Histogram != null && x != resultsMacdCurrent).LastOrDefault(); // Macd trước đó

                if ((resultsMfiCurrent.Mfi >= 30 && resultsMfiCurrent.Mfi <= 60 && resultsMfiCurrent.Mfi > resultsMfiPre.Mfi)
                    && (resultsMacdCurrent.Histogram > 0 && resultsMacdPre.Histogram <= 0))
                {
                    value = true;
                }
            }

            return value;
        }
    }
}

