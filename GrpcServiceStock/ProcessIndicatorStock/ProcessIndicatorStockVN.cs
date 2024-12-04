using GrpcServiceStock.Common;
using GrpcServiceStock.Modules;
using GrpcServiceStock.Response;
using Skender.Stock.Indicators;
using SSI.FCData.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using static GrpcServiceStock.Enum.EnumHelper;

namespace GrpcServiceStock.ProcessIndicatorStock
{
    public class ProcessIndicatorStockVN
    {
        public static int DefaultVolume = 200000;

        #region Bắt đáy
        /// <summary>
        /// Công thức 1: bắt đáy
        /// Giá trị giao dịch (giá đóng cửa nhân với khối lượng) phải lớn hơn hoặc bằng 5 triệu.
        /// Giá đóng cửa của cổ phiếu phải lớn hơn hoặc bằng 5.
        /// Giá đóng cửa hôm nay phải tăng ít nhất 1% so với giá đóng cửa ngày hôm qua.
        /// Giá đóng cửa hôm nay thấp hơn đường trung bình động 30 ngày của giá.
        /// Đường trung bình khối lượng giao dịch 15 ngày phải lớn hơn hoặc bằng 100.000.
        /// Khối lượng giao dịch tại các mốc -5, -10, -15, -20 ngày phải lớn hơn hoặc bằng 50.000.
        /// Giá trị giao dịch (giá đóng cửa × khối lượng) phải nhỏ hơn 1 tỷ.
        /// Giá cao nhất trong 15 ngày qua phải lớn hơn ít nhất 20% so với giá đóng cửa hiện tại.
        /// C* V>=5000000
        /// AND C>=5
        /// AND C>= 1.01*Ref(C,-1)
        /// AND C<MA(C,30)
        /// AND MA(V,15) >=100000
        /// AND Ref(V,-5)>=50000
        /// AND Ref(V,-10)>=50000
        /// AND Ref(V,-15)>=50000
        /// AND Ref(V,-20)>=50000
        /// AND C*V<1000000000
        /// AND HHV(H,15) >1.2*C
        /// </summary>
        /// <param name="close"></param>
        /// <param name="volume"></param>
        /// <param name="high"></param>
        /// <returns></returns>
        public static Task IndicatorBatDay(SecuritiesListResponseModel company, List<MarketDataQuote> quotes)
        {
            var avgVolume = quotes.Average(x => x.Volume);
            if (avgVolume > DefaultVolume)
            {
                double[] close = quotes.Select(x => (double)x.Close).ToArray();
                double[] volume = quotes.Select(x => (double)x.Volume).ToArray();
                double[] high = quotes.Select(x => (double)x.High).ToArray();

                if (CheckConditionsBatDay(close, volume, high))
                {
                    var price = GetSellBuyPriceHelper.GetBuySellPrice(quotes);

                    SSIDataStock.listIndicatorStockBatDay.Add(new IndicatorStock
                    {
                        Symbol = company.Symbol,

                        PurchasePrice1 = PriceHelper.Int50Money(price.PointBuy1),

                        PurchasePrice2 = PriceHelper.Int50Money(price.PointBuy2),

                        PurchasePrice3 = PriceHelper.Int50Money(price.PointBuy3),

                        SellingPrice1 = PriceHelper.Int50Money(price.PointSell1),

                        SellingPrice2 = PriceHelper.Int50Money(price.PointSell2),

                        SellingPrice3 = PriceHelper.Int50Money(price.PointSell3),
                    });
                }
            }
            else
            {
                if (!SSIDataStock._dicCompanyNotProcessStock.ContainsKey(company.Symbol))
                    SSIDataStock._dicCompanyNotProcessStock.Add(company.Symbol, DateTime.Today.AddDays(14));
            }
            return Task.CompletedTask;
        }

        // Assuming the price data (close, volume, high) are passed as arrays
        public static bool CheckConditionsBatDay(double[] close, double[] volume, double[] high)
        {
            // Length of the data (assuming all arrays have the same length)
            int n = close.Length;

            // Calculate Moving Average and Ref values
            double[] ma30 = TechnicalIndicatorsHelper.MovingAverage(close, 30);
            double[] ma15Volume = TechnicalIndicatorsHelper.MovingAverage(volume, 15);

            // Check the conditions
            for (int i = 1; i < n; i++)  // Starting from 1 because we need Ref(i,-1) and other previous values
            {
                // Condition 1: C*V >= 5000000
                if (close[i] * volume[i] < 5000000)
                    return false;

                // Condition 2: C >= 5
                if (close[i] < 5)
                    return false;

                // Condition 3: C >= 1.01*Ref(C, -1)
                if (close[i] < 1.01 * close[i - 1])
                    return false;

                // Condition 4: C < MA(C, 30)
                if (close[i] >= ma30[i])
                    return false;

                // Condition 5: MA(V, 15) >= 100000
                if (ma15Volume[i] < 100000)
                    return false;

                // Condition 6: Ref(V, -5) >= 50000
                if (i >= 5 && volume[i - 5] < 50000)
                    return false;

                // Condition 7: Ref(V, -10) >= 50000
                if (i >= 10 && volume[i - 10] < 50000)
                    return false;

                // Condition 8: Ref(V, -15) >= 50000
                if (i >= 15 && volume[i - 15] < 50000)
                    return false;

                // Condition 9: Ref(V, -20) >= 50000
                if (i >= 20 && volume[i - 20] < 50000)
                    return false;

                // Condition 10: C*V < 1000000000
                if (close[i] * volume[i] >= 1000000000)
                    return false;

                // Condition 11: HHV(H, 15) > 1.2*C
                if (TechnicalIndicatorsHelper.HighHighest(high, 15, i) <= 1.2 * close[i])
                    return false;
            }

            // If all conditions are satisfied
            return true;
        }
        #endregion

        #region Mã tiềm năng ăn CE
        /// <summary>
        /// Công thức 2: mã tiềm năng ăn CE
        /// Nhóm 1
        /// Giá đóng cửa hiện tại không thấp hơn 70% giá cao nhất trong 120 ngày và 20 ngày qua
        /// Giá tăng không quá 3% so với ngày hôm trước và các biến động giá trong 3 ngày qua đều không vượt quá 3%
        /// Giá hiện tại tăng ít nhất 10% so với giá cách đây 120 ngày và 20 ngày
        /// Giá trị giao dịch lớn hơn 5 triệu
        /// Giá hiện tại không thấp hơn 5
        /// Khối lượng giao dịch của ngày gần đây và các mốc -5, -10 phải trên 100.000
        /// Nhóm 2
        /// Giá dao động trong biên độ hẹp trong 5 ngày qua (HHV(C,5) < 1.055*LLV(C,5)). Giá trị giao dịch nằm trong khoảng từ 5 triệu đến 500 triệu
        /// Giá đóng cửa hiện tại nằm ở nửa trên của khoảng dao động 5 ngày qua
        /// Khối lượng giao dịch trung bình 30 ngày >= 100.000 và khối lượng tại các mốc -5, -10, -20 cũng >= 100.000
        /// RSI (14 ngày) >= 40
        /// Giá lớn hơn đường trung bình giá 30 ngà
        /// (C>= 0.7*HHV(C,120)
        /// AND C>= 0.7*HHV(C,20)
        /// AND(C- Ref(C,-1))/Ref(C,-1)<= 0.03
        /// AND(Ref(C,-1)- Ref(C,-2))/Ref(C,-2)<= 0.03
        /// AND(Ref(C,-2)-Ref(C,-1))/Ref(C,-1)<= 0.03
        /// AND((C - Ref(C,-120))/Ref(C,-120))*100>= 10
        /// AND((C - Ref(C,-20))/Ref(C,-20))*100>= 10
        /// AND C*V >= 5000000
        /// AND C>=5
        /// AND Ref(V,-1)>=100000
        /// AND Ref(V,-5)>=100000
        /// AND Ref(V,-10)>=100000)
        /// OR
        /// (HHV(C,5) <1.055* LLV(C,5)
        /// AND C * V >= 5000000 
        /// AND C*V< 500000000
        /// AND C>= (HHV(C,5)+LLV(C,5))/2
        /// AND MA(V,30)>=100000
        /// AND Ref(V,-5)>=100000
        /// AND Ref(V,-10)>=100000
        /// AND Ref(V,-20)>=100000
        /// AND RSI(14)>=40
        /// AND C>=5
        /// AND C>MA(C,30))
        /// </summary>
        /// <param name="company"></param>
        /// <param name="quotes"></param>
        /// <returns></returns>
        public static Task IndicatorCE(SecuritiesListResponseModel company, List<MarketDataQuote> quotes)
        {
            var avgVolume = quotes.Average(x => x.Volume);
            if (avgVolume > DefaultVolume)
            {
                double[] close = quotes.Select(x => (double)x.Close).ToArray();
                double[] volume = quotes.Select(x => (double)x.Volume).ToArray();

                if (EvaluateConditionCE(close, volume))
                {
                    var price = GetSellBuyPriceHelper.GetBuySellPrice(quotes);

                    SSIDataStock.listIndicatorStockCE.Add(new IndicatorStock
                    {
                        Symbol = company.Symbol,

                        PurchasePrice1 = PriceHelper.Int50Money(price.PointBuy1),

                        PurchasePrice2 = PriceHelper.Int50Money(price.PointBuy2),

                        PurchasePrice3 = PriceHelper.Int50Money(price.PointBuy3),

                        SellingPrice1 = PriceHelper.Int50Money(price.PointSell1),

                        SellingPrice2 = PriceHelper.Int50Money(price.PointSell2),

                        SellingPrice3 = PriceHelper.Int50Money(price.PointSell3),
                    });
                }
            }
            else
            {
                if (!SSIDataStock._dicCompanyNotProcessStock.ContainsKey(company.Symbol))
                    SSIDataStock._dicCompanyNotProcessStock.Add(company.Symbol, DateTime.Today.AddDays(14));
            }
            return Task.CompletedTask;
        }

        public static bool Condition1CE(double[] C, double[] V)
        {
            // C >= 0.7 * HHV(C, 120) AND C >= 0.7 * HHV(C, 20)
            if (!(C.Last() >= 0.7 * TechnicalIndicatorsHelper.HHV(C, 120) && C.Last() >= 0.7 * TechnicalIndicatorsHelper.HHV(C, 20)))
                return false;

            // (C - Ref(C,-1))/Ref(C,-1) <= 0.03
            if (!((C.Last() - TechnicalIndicatorsHelper.Ref(C, -1)) / TechnicalIndicatorsHelper.Ref(C, -1) <= 0.03))
                return false;

            // (Ref(C,-1) - Ref(C,-2))/Ref(C,-2) <= 0.03
            if (!((TechnicalIndicatorsHelper.Ref(C, -1) - TechnicalIndicatorsHelper.Ref(C, -2)) / TechnicalIndicatorsHelper.Ref(C, -2) <= 0.03))
                return false;

            // (Ref(C,-2) - Ref(C,-1))/Ref(C,-1) <= 0.03
            if (!((TechnicalIndicatorsHelper.Ref(C, -2) - TechnicalIndicatorsHelper.Ref(C, -1)) / TechnicalIndicatorsHelper.Ref(C, -1) <= 0.03))
                return false;

            // ((C - Ref(C,-120))/Ref(C,-120)) * 100 >= 10
            if (!(((C.Last() - TechnicalIndicatorsHelper.Ref(C, -120)) / TechnicalIndicatorsHelper.Ref(C, -120)) * 100 >= 10))
                return false;

            // ((C - Ref(C,-20))/Ref(C,-20)) * 100 >= 10
            if (!(((C.Last() - TechnicalIndicatorsHelper.Ref(C, -20)) / TechnicalIndicatorsHelper.Ref(C, -20)) * 100 >= 10))
                return false;

            // C * V >= 5000000
            if (!(C.Last() * V.Last() >= 5000000))
                return false;

            // C >= 5
            if (!(C.Last() >= 5))
                return false;

            // Ref(V,-1) >= 100000
            if (!(TechnicalIndicatorsHelper.Ref(V, -1) >= 100000))
                return false;

            // Ref(V,-5) >= 100000
            if (!(TechnicalIndicatorsHelper.Ref(V, -5) >= 100000))
                return false;

            // Ref(V,-10) >= 100000
            if (!(TechnicalIndicatorsHelper.Ref(V, -10) >= 100000))
                return false;

            return true;
        }

        public static bool Condition2CE(double[] C, double[] V)
        {
            // HHV(C, 5) < 1.055 * LLV(C, 5)
            if (!(TechnicalIndicatorsHelper.HHV(C, 5) < 1.055 * TechnicalIndicatorsHelper.LLV(C, 5)))
                return false;

            // C * V >= 5000000
            if (!(C.Last() * V.Last() >= 5000000))
                return false;

            // C * V < 500000000
            if (!(C.Last() * V.Last() < 500000000))
                return false;

            // C >= (HHV(C,5) + LLV(C,5)) / 2
            if (!(C.Last() >= (TechnicalIndicatorsHelper.HHV(C, 5) + TechnicalIndicatorsHelper.LLV(C, 5)) / 2))
                return false;

            // MA(V, 30) >= 100000
            if (!(TechnicalIndicatorsHelper.MA(V, 30) >= 100000))
                return false;

            // Ref(V,-5) >= 100000
            if (!(TechnicalIndicatorsHelper.Ref(V, -5) >= 100000))
                return false;

            // Ref(V,-10) >= 100000
            if (!(TechnicalIndicatorsHelper.Ref(V, -10) >= 100000))
                return false;

            // Ref(V,-20) >= 100000
            if (!(TechnicalIndicatorsHelper.Ref(V, -20) >= 100000))
                return false;

            // RSI(14) >= 40
            if (!(TechnicalIndicatorsHelper.RSI(C, 14) >= 40))
                return false;

            // C >= 5
            if (!(C.Last() >= 5))
                return false;

            // C > MA(C, 30)
            if (!(C.Last() > TechnicalIndicatorsHelper.MA(C, 30)))
                return false;

            return true;
        }

        public static bool EvaluateConditionCE(double[] C, double[] V)
        {
            // The OR condition
            return Condition1CE(C, V) || Condition2CE(C, V);
        }
        #endregion

        #region
        /// <summary>
        /// Công thức 3: chọn mã an toàn 1-2 tuần
        /// Nhóm 1
        /// Giá đóng cửa(C) vượt đỉnh giá cao nhất của 4 ngày gần nhất(Ref(H, -1)...Ref(H, -4)).
        /// Giá đóng cửa(C) lớn hơn giá mở cửa(O).
        /// Giá tăng ít nhất 1% so với ngày trước đó(C >= 1.01*Ref(C,-1)).
        /// Khối lượng giao dịch tăng so với ngày trước đó(V >= Ref(V,-1)).
        /// Giá trị giao dịch nằm trong khoảng từ 5 triệu đến 500 triệu.
        /// Khối lượng giao dịch ở các ngày -5, -10, -15, -20 đều lớn hơn hoặc bằng 50.000.
        /// Giá duy trì tăng đều đặn trong 4 ngày qua(Ref(C,-1) <= 1.04*Ref(C,-2), ...).
        /// Giá đóng cửa vượt ít nhất 30% giá thấp nhất trong 50 ngày(C > 1.3*LLV(L,50))
        /// Nhóm 2
        /// Giá tăng hơn 1% so với ngày trước(C > 1.01*Ref(C,-1)).
        /// Khối lượng giao dịch tăng mạnh, vượt 1.3 lần đường trung bình 50 hoặc 15 ngày(V >= 1.3*MA(V,50) OR V >= 1.3*MA(V,15)).
        /// Giá đóng cửa lớn hơn đường trung bình giá 15 ngày(C > MA(C,15)).
        /// Giá đóng cửa nằm ở nửa trên của thanh giá ngày hôm nay(C >= (H + L) / 2).
        /// Chỉ số RSI(14 ngày) đạt ít nhất 58
        /// Nhóm 3
        /// Giá đóng cửa hôm nay và ngày trước đó đều nằm trên đường trung bình giá 15 ngày(C > MA(C,15) và Ref(C,-1) > MA(C,15))
        /// Nhóm 4
        /// RSI(14 ngày) phải vượt 60 (RSI(14) > 60).
        /// Giá tăng không quá mạnh trong 2 ngày gần đây(Ref(C,-1) <= 1.05*Ref(C,-2)).
        /// </summary>
        /// <param name="company"></param>
        /// <param name="quotes"></param>
        /// <returns></returns>
        public static Task IndicatorAnToan(SecuritiesListResponseModel company, List<MarketDataQuote> quotes)
        {
            var avgVolume = quotes.Average(x => x.Volume);
            if (avgVolume > DefaultVolume)
            {
                double[] close = quotes.Select(x => (double)x.Close).ToArray();
                double[] volume = quotes.Select(x => (double)x.Volume).ToArray();
                double[] high = quotes.Select(x => (double)x.High).ToArray();
                double[] low = quotes.Select(x => (double)x.Low).ToArray();
                double[] open = quotes.Select(x => (double)x.Open).ToArray();

                for (int i = 0; i < quotes.Count; i++)
                {
                    if (CheckConditionsAnToan(close, high, low, open, volume, i))
                    {
                        var price = GetSellBuyPriceHelper.GetBuySellPrice(quotes);

                        SSIDataStock.listIndicatorStockAnToan.Add(new IndicatorStock
                        {
                            Symbol = company.Symbol,

                            PurchasePrice1 = PriceHelper.Int50Money(price.PointBuy1),

                            PurchasePrice2 = PriceHelper.Int50Money(price.PointBuy2),

                            PurchasePrice3 = PriceHelper.Int50Money(price.PointBuy3),

                            SellingPrice1 = PriceHelper.Int50Money(price.PointSell1),

                            SellingPrice2 = PriceHelper.Int50Money(price.PointSell2),

                            SellingPrice3 = PriceHelper.Int50Money(price.PointSell3),
                        });

                        break;
                    }
                }


            }
            else
            {
                if (!SSIDataStock._dicCompanyNotProcessStock.ContainsKey(company.Symbol))
                    SSIDataStock._dicCompanyNotProcessStock.Add(company.Symbol, DateTime.Today.AddDays(14));
            }
            return Task.CompletedTask;
        }

        public static bool CheckConditionsAnToan(double[] closePrices, double[] highPrices, double[] lowPrices, double[] openPrices, double[] volume, int index)
        {
            double C = closePrices[index];
            double H = highPrices[index];
            double L = lowPrices[index];
            double O = openPrices[index];
            double V = volume[index];

            // Ensure we have enough data for our analysis
            if (index < 20) return false;

            // Check first condition block
            if (C > TechnicalIndicatorsHelper.Ref(closePrices, -1) && C > TechnicalIndicatorsHelper.Ref(closePrices, -2) && C > TechnicalIndicatorsHelper.Ref(closePrices, -3) && C > TechnicalIndicatorsHelper.Ref(closePrices, -4) &&
                C >= 5 && C >= O && C >= 1.01 * TechnicalIndicatorsHelper.Ref(closePrices, -1) && C >= TechnicalIndicatorsHelper.Ref(closePrices, -2) &&
                V >= TechnicalIndicatorsHelper.Ref(volume, -1) && C * V >= 5000000 && C * V < 500000000 &&
                TechnicalIndicatorsHelper.Ref(volume, -5) >= 50000 && TechnicalIndicatorsHelper.Ref(volume, -10) >= 50000 && TechnicalIndicatorsHelper.Ref(volume, -15) >= 50000 && TechnicalIndicatorsHelper.Ref(volume, -20) >= 50000 &&
                TechnicalIndicatorsHelper.Ref(closePrices, -1) <= 1.04 * TechnicalIndicatorsHelper.Ref(closePrices, -2) && TechnicalIndicatorsHelper.Ref(closePrices, -2) <= 1.04 * TechnicalIndicatorsHelper.Ref(closePrices, -3) &&
                TechnicalIndicatorsHelper.Ref(closePrices, -3) <= 1.04 * TechnicalIndicatorsHelper.Ref(closePrices, -4) && TechnicalIndicatorsHelper.Ref(volume, -1) >= 100000 &&
                C >= 5 && C > 1.3 * TechnicalIndicatorsHelper.LLV(lowPrices, 50))
            {
                return true;
            }

            // Check second condition block
            if (C > 1.01 * TechnicalIndicatorsHelper.Ref(closePrices, -1) && C >= TechnicalIndicatorsHelper.Ref(closePrices, -2) && C * V < 500000000 &&
                (V >= 1.3 * TechnicalIndicatorsHelper.MA(volume, 50) || V >= 1.3 * TechnicalIndicatorsHelper.MA(volume, 15)) && TechnicalIndicatorsHelper.MA(volume, 15) >= 100000 &&
                TechnicalIndicatorsHelper.MA(volume, 50) >= 100000 && C > TechnicalIndicatorsHelper.MA(closePrices, 15) && V > TechnicalIndicatorsHelper.Ref(volume, -1) &&
                C >= (H + L) / 2 && C > O && C >= 5 && C * V >= 5000000 && C * V < 500000000 &&
                C > 1.3 * TechnicalIndicatorsHelper.LLV(lowPrices, 50) && TechnicalIndicatorsHelper.Ref(closePrices, -1) <= 1.04 * TechnicalIndicatorsHelper.Ref(closePrices, -2) &&
                TechnicalIndicatorsHelper.Ref(closePrices, -2) <= 1.04 * TechnicalIndicatorsHelper.Ref(closePrices, -3) && TechnicalIndicatorsHelper.Ref(closePrices, -3) <= 1.04 * TechnicalIndicatorsHelper.Ref(closePrices, -4) &&
                TechnicalIndicatorsHelper.Ref(volume, -1) >= 30000 && TechnicalIndicatorsHelper.RSI(closePrices) >= 58 && TechnicalIndicatorsHelper.Ref(volume, -5) >= 50000 &&
                TechnicalIndicatorsHelper.Ref(volume, -10) >= 50000 && TechnicalIndicatorsHelper.Ref(volume, -15) >= 50000 && TechnicalIndicatorsHelper.Ref(volume, -20) >= 50000)
            {
                return true;
            }

            // Check third condition block
            if (C > TechnicalIndicatorsHelper.Ref(closePrices, -1) && C > TechnicalIndicatorsHelper.Ref(closePrices, -2) && C > TechnicalIndicatorsHelper.Ref(closePrices, -3) && C > TechnicalIndicatorsHelper.Ref(closePrices, -4) &&
                C > TechnicalIndicatorsHelper.MA(closePrices, 15) && TechnicalIndicatorsHelper.Ref(closePrices, -1) > TechnicalIndicatorsHelper.MA(closePrices, 15) && C >= O && C >= 5 &&
                C >= 1.01 * TechnicalIndicatorsHelper.Ref(closePrices, -1) && C * V >= 5000000 && C * V < 500000000 &&
                C > 1.3 * TechnicalIndicatorsHelper.LLV(lowPrices, 50) && V >= TechnicalIndicatorsHelper.Ref(volume, -1) && TechnicalIndicatorsHelper.Ref(volume, -5) >= 50000 &&
                TechnicalIndicatorsHelper.Ref(volume, -10) >= 50000 && TechnicalIndicatorsHelper.Ref(volume, -15) >= 50000 && TechnicalIndicatorsHelper.Ref(volume, -20) >= 50000)
            {
                return true;
            }

            // Check fourth condition block
            if (C > 1.01 * TechnicalIndicatorsHelper.Ref(closePrices, -1) && C >= TechnicalIndicatorsHelper.Ref(closePrices, -2) && TechnicalIndicatorsHelper.RSI(closePrices) > 60 &&
                V >= 1.3 * TechnicalIndicatorsHelper.MA(volume, 30) && V >= 0.8 * TechnicalIndicatorsHelper.Ref(volume, -1) && TechnicalIndicatorsHelper.MA(volume, 15) >= 50000 &&
                TechnicalIndicatorsHelper.MA(volume, 50) >= 50000 && C > TechnicalIndicatorsHelper.MA(closePrices, 15) && C >= (H + L) / 2 && C > O &&
                C >= 5 && C * V >= 5000000 && C * V < 500000000 && C > 1.3 * TechnicalIndicatorsHelper.LLV(lowPrices, 50) &&
                TechnicalIndicatorsHelper.Ref(closePrices, -1) <= 1.05 * TechnicalIndicatorsHelper.Ref(closePrices, -2) && TechnicalIndicatorsHelper.Ref(volume, -5) >= 50000 &&
                TechnicalIndicatorsHelper.Ref(volume, -10) >= 50000 && TechnicalIndicatorsHelper.Ref(volume, -15) >= 50000 && TechnicalIndicatorsHelper.Ref(volume, -20) >= 50000)
            {
                return true;
            }

            return false; // No conditions matched
        }
        #endregion
    }
}

