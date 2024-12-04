using System;
using System.Linq;

namespace GrpcServiceStock.Common
{
    public class StockAnalysis
    {
        // Example method for calculating Moving Average (MA)
        public static double MA(double[] values, int period)
        {
            return values.TakeLast(period).Average();
        }

        // Example method for calculating RSI
        public static double RSI(double[] closePrices, int period = 14)
        {
            double[] gains = new double[closePrices.Length];
            double[] losses = new double[closePrices.Length];

            for (int i = 1; i < closePrices.Length; i++)
            {
                double change = closePrices[i] - closePrices[i - 1];
                gains[i] = (change > 0) ? change : 0;
                losses[i] = (change < 0) ? -change : 0;
            }

            double avgGain = gains.TakeLast(period).Average();
            double avgLoss = losses.TakeLast(period).Average();

            double rs = avgGain / avgLoss;
            return 100 - (100 / (1 + rs));
        }

        // Example method for calculating Lowest Low Value (LLV)
        public static double LLV(double[] lowPrices, int period)
        {
            return lowPrices.TakeLast(period).Min();
        }

        // Example method for calculating Ref (value from n bars ago)
        public static double Ref(double[] values, int n)
        {
            if (n < 0 || n >= values.Length) return double.NaN;
            return values[values.Length - 1 - n];
        }

        public static bool AnalyzeStock(double[] closePrices, double[] highPrices, double[] lowPrices, double[] openPrices, double[] volume, int index)
        {
            double C = closePrices[index];
            double H = highPrices[index];
            double L = lowPrices[index];
            double O = openPrices[index];
            double V = volume[index];

            // Ensure we have enough data for our analysis
            if (index < 20) return false;

            // Check first condition block
            if (C > Ref(closePrices, -1) && C > Ref(closePrices, -2) && C > Ref(closePrices, -3) && C > Ref(closePrices, -4) &&
                C >= 5 && C >= O && C >= 1.01 * Ref(closePrices, -1) && C >= Ref(closePrices, -2) &&
                V >= Ref(volume, -1) && C * V >= 5000000 && C * V < 500000000 &&
                Ref(volume, -5) >= 50000 && Ref(volume, -10) >= 50000 && Ref(volume, -15) >= 50000 && Ref(volume, -20) >= 50000 &&
                Ref(closePrices, -1) <= 1.04 * Ref(closePrices, -2) && Ref(closePrices, -2) <= 1.04 * Ref(closePrices, -3) &&
                Ref(closePrices, -3) <= 1.04 * Ref(closePrices, -4) && Ref(volume, -1) >= 100000 &&
                C >= 5 && C > 1.3 * LLV(lowPrices, 50))
            {
                return true;
            }

            // Check second condition block
            if (C > 1.01 * Ref(closePrices, -1) && C >= Ref(closePrices, -2) && C * V < 500000000 &&
                (V >= 1.3 * MA(volume, 50) || V >= 1.3 * MA(volume, 15)) && MA(volume, 15) >= 100000 &&
                MA(volume, 50) >= 100000 && C > MA(closePrices, 15) && V > Ref(volume, -1) &&
                C >= (H + L) / 2 && C > O && C >= 5 && C * V >= 5000000 && C * V < 500000000 &&
                C > 1.3 * LLV(lowPrices, 50) && Ref(closePrices, -1) <= 1.04 * Ref(closePrices, -2) &&
                Ref(closePrices, -2) <= 1.04 * Ref(closePrices, -3) && Ref(closePrices, -3) <= 1.04 * Ref(closePrices, -4) &&
                Ref(volume, -1) >= 30000 && RSI(closePrices) >= 58 && Ref(volume, -5) >= 50000 &&
                Ref(volume, -10) >= 50000 && Ref(volume, -15) >= 50000 && Ref(volume, -20) >= 50000)
            {
                return true;
            }

            // Check third condition block
            if (C > Ref(closePrices, -1) && C > Ref(closePrices, -2) && C > Ref(closePrices, -3) && C > Ref(closePrices, -4) &&
                C > MA(closePrices, 15) && Ref(closePrices, -1) > MA(closePrices, 15) && C >= O && C >= 5 &&
                C >= 1.01 * Ref(closePrices, -1) && C * V >= 5000000 && C * V < 500000000 &&
                C > 1.3 * LLV(lowPrices, 50) && V >= Ref(volume, -1) && Ref(volume, -5) >= 50000 &&
                Ref(volume, -10) >= 50000 && Ref(volume, -15) >= 50000 && Ref(volume, -20) >= 50000)
            {
                return true;
            }

            // Check fourth condition block
            if (C > 1.01 * Ref(closePrices, -1) && C >= Ref(closePrices, -2) && RSI(closePrices) > 60 &&
                V >= 1.3 * MA(volume, 30) && V >= 0.8 * Ref(volume, -1) && MA(volume, 15) >= 50000 &&
                MA(volume, 50) >= 50000 && C > MA(closePrices, 15) && C >= (H + L) / 2 && C > O &&
                C >= 5 && C * V >= 5000000 && C * V < 500000000 && C > 1.3 * LLV(lowPrices, 50) &&
                Ref(closePrices, -1) <= 1.05 * Ref(closePrices, -2) && Ref(volume, -5) >= 50000 &&
                Ref(volume, -10) >= 50000 && Ref(volume, -15) >= 50000 && Ref(volume, -20) >= 50000)
            {
                return true;
            }

            return false; // No conditions matched
        }
    }

}
