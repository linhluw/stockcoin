using GrpcServiceStock.Enum;
using GrpcServiceStock.Response;
using Serilog;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace GrpcServiceStock.Common
{
    public class GetSellBuyPriceHelper
    {
        public static PointBuySell GetBuySellPrice(List<MarketDataQuote> quotes)
        {
            var price = new PointBuySell();

            var resultsBollingerBands = quotes.GetBollingerBands().LastOrDefault();

            price.PointBuy1 = resultsBollingerBands.LowerBand.Value;

            price.PointBuy2 = resultsBollingerBands.Sma.Value;

            price.PointBuy3 = resultsBollingerBands.UpperBand.Value;

            var highestPrice = quotes.Max(x => x.High);

            var lowestPrice = quotes.Min(x => x.Low);

            price.PointSell1 = FibonacciRetracement((double)highestPrice, (double)lowestPrice, EnumHelper.FibonacciLevel.DiemBan1);

            price.PointSell2 = FibonacciRetracement((double)highestPrice, (double)lowestPrice, EnumHelper.FibonacciLevel.DiemBan2);

            price.PointSell3 = FibonacciRetracement((double)highestPrice, (double)lowestPrice, EnumHelper.FibonacciLevel.DiemBan3);

            return price;
        }

        public static double FibonacciRetracement(double highestPrice, double lowestPrice, EnumHelper.FibonacciLevel fibonacciLevels)
        {
            FieldInfo field = fibonacciLevels.GetType().GetField(fibonacciLevels.ToString());

            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));

            double range = lowestPrice - highestPrice;

            double level = lowestPrice - (range * double.Parse(attribute.Description));

            return level;
        }
    }

    public class PointBuySell
    {
        // điểm mua thấp nhất
        public double PointBuy1 { get; set; }

        // điểm mua trung bình
        public double PointBuy2 { get; set; }

        // điểm mua cao nhất
        public double PointBuy3 { get; set; }

        // điểm bán thấp nhất
        public double PointSell1 { get; set; }

        // điểm bán trung bình
        public double PointSell2 { get; set; }

        // điểm bán cao nhất
        public double PointSell3 { get; set; }
    }
}
