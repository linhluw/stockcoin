using GrpcServiceStock.Response;
using static GrpcServiceStock.Enum.EnumHelper;
using System;

namespace GrpcServiceStock.Common
{
    public class StringHelper
    {
        public static string NotifyDays(IndicatorEnum type)
        {
            var value = string.Empty;

            switch (type)
            {
                case IndicatorEnum.BatDayPhuDM:
                    value = string.Format("{0} | Chỉ báo mã BẮT ĐÁY by PHUDM\r\n", DateTime.Today.ToString("dd/MM/yyyy"));
                    break;
                case IndicatorEnum.CEPhuDM:
                    value = string.Format("{0} | Chỉ báo mã TIỀM NĂNG CE by PHUDM\r\n", DateTime.Today.ToString("dd/MM/yyyy"));
                    break;
                case IndicatorEnum.AnToanPhuDM:
                    value = string.Format("{0} | Chỉ báo mã AN TOÀN by PHUDM\r\n", DateTime.Today.ToString("dd/MM/yyyy"));
                    break;
                case IndicatorEnum.CoinLinhLV:
                    value = string.Format("{0} | Chỉ báo mã COIN by LINHLV\r\n", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                    break;
                default:
                    break;
            }

            return value;
        }

        public static string NotifyIndicator(IndicatorStock item)
        {
            return string.Format("● {0}: Mua 1: {1}, Mua 2: {2}, Mua 3: {3} | Bán 1: {4}, Bán 2: {5}, Bán 3: {6}", 
                item.Symbol, item.PurchasePrice1, item.PurchasePrice2, item.PurchasePrice3, item.SellingPrice1, item.SellingPrice2, item.SellingPrice3);
        }
    }
}
