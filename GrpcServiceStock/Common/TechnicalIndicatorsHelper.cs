using System;
using System.Collections.Generic;
using System.Linq;

namespace GrpcServiceStock.Common
{
    public class TechnicalIndicatorsHelper
    {
        // Moving Average function
        public static double[] MovingAverage(double[] data, int period)
        {
            int n = data.Length;
            double[] ma = new double[n];

            for (int i = period - 1; i < n; i++)
            {
                ma[i] = data.Skip(i - period + 1).Take(period).Average();
            }

            return ma;
        }

        // Function to calculate the highest high in a period (HHV)
        public static double HighHighest(double[] high, int period, int currentIndex)
        {
            int start = Math.Max(currentIndex - period + 1, 0);
            return high.Skip(start).Take(currentIndex - start + 1).Max();
        }

        public static double HHV(double[] values, int period)
        {
            return values.Take(period).Max();
        }

        // Helper function to get the lowest value of the array (LLV)
        public static double LLV(double[] values, int period)
        {
            return values.Take(period).Min();
        }

        // Moving Average (MA)
        public static double MA(double[] values, int period)
        {
            return values.Take(period).Average();
        }

        // Relative Strength Index (RSI)
        public static double RSI(double[] values, int period = 14)
        {
            var gains = new List<double>();
            var losses = new List<double>();

            for (int i = 1; i < period; i++)
            {
                double change = values[i] - values[i - 1];
                if (change > 0) gains.Add(change);
                else losses.Add(-change);
            }

            double avgGain = gains.Any() ? gains.Average() : 0;
            double avgLoss = losses.Any() ? losses.Average() : 0;

            if (avgLoss == 0) return 100;
            double rs = avgGain / avgLoss;
            return 100 - (100 / (1 + rs));
        }

        // Ref function (Reference function to get past data point)
        public static double Ref(double[] values, int offset)
        {
            int index = values.Length + offset;
            if (index >= 0 && index < values.Length)
                return values[index];
            return double.NaN; // or handle out-of-bounds
        }
    }
}
