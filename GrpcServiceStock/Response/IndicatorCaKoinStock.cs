using System;

namespace GrpcServiceStock.Response
{
    public class IndicatorStock
    {
        public string Symbol { get; set; } // mã CK

        public double PurchasePrice1 { get; set; } // giá mua thấp nhất

        public double PurchasePrice2 { get; set; } // giá mua thấp nhất

        public double PurchasePrice3 { get; set; } // giá mua cao nhất

        public double SellingPrice1 { get; set; } // giá bán

        public double SellingPrice2 { get; set; } // giá bán

        public double SellingPrice3 { get; set; } // giá bán
    }
}
