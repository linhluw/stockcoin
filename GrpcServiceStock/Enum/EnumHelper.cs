using System.ComponentModel;

namespace GrpcServiceStock.Enum
{
    public class EnumHelper
    {
        public enum RoomType
        {
            Stock = 0,
            Coin = 1
        }

        public enum IndicatorEnum
        {
            BatDayPhuDM = 0,
            CEPhuDM = 1,
            AnToanPhuDM = 2,
            CoinLinhLV = 3,
        }

        public enum FibonacciLevel
        {
            [Description("0")]
            DiemMua1,

            [Description("0.236")]
            DiemMua2,

            [Description("0.382")]
            DiemMua3,

            [Description("0.768")]
            DiemBan1,

            [Description("1")]
            DiemBan2,

            [Description("1.618")]
            DiemBan3
        }
    }
}
