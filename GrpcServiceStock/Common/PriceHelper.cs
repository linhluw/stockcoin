using System;

namespace GrpcServiceStock.Common
{
    public class PriceHelper
    {
        /// <summary>
        /// làm tròn 50 đồng
        /// </summary>
        /// <param name="money"></param>
        /// <returns></returns>
        public static int Int50Money(double money)
        {
            return (int)Math.Round(money / 50.0) * 50;
        }

        /// <summary>
        /// làm tròn giá cho coin
        /// </summary>
        /// <param name="money"></param>
        /// <returns></returns>
        public static double DoubleCoin(double money)
        {
            var value = money;
            if (money >= 10)
            {
                value = Math.Round(money, 2);
            }
            else if (money >= 0.1)
            {
                value = Math.Round(money, 4);
            }
            else if (money >= 0.01)
            {
                value = Math.Round(money, 5);
            }
            else if (money >= 0.001)
            {
                value = Math.Round(money, 6);
            }
            else if (money >= 0.0001)
            {
                value = Math.Round(money, 7);
            }
            else if (money >= 0.00001)
            {
                value = Math.Round(money, 8);
            }
            return value;
        }
    }
}
