using Skender.Stock.Indicators;
using System;

namespace GrpcServiceStock.Response
{
    public class MarketDataQuote : IQuote
    {
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }
    }

    public class SymbolQuote : MarketDataQuote
    {
        public string Symbol { get; set; }
    }
}
