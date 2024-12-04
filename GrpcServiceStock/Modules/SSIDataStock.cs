using System.Threading.Tasks;
using System;
using GrpcServiceStock.Common;
using SSI.FCData;
using SSI.FCData.Models.Request;
using SSI.FCData.Models.Response;
using System.Collections.Generic;
using System.Linq;
using GrpcServiceStock.Response;
using System.Globalization;
using static GrpcServiceStock.Enum.EnumHelper;
using GrpcServiceStock.SQL;

namespace GrpcServiceStock.Modules
{
    public class SSIDataStock
    {
        private static string _FastConnectUrl = null;

        private static string FastConnectUrl
        {
            get
            {
                if (_FastConnectUrl == null)
                {
                    _FastConnectUrl = ConfigurationHelper.FastConnectUrl;
                }
                return _FastConnectUrl;
            }
        }

        private static string _ConsumerId = null;

        private static string ConsumerId
        {
            get
            {
                if (_ConsumerId == null)
                {
                    _ConsumerId = ConfigurationHelper.ConsumerId;
                }
                return _ConsumerId;
            }
        }

        private static string _ConsumerSecret = null;
        private static string ConsumerSecret
        {
            get
            {
                if (_ConsumerSecret == null)
                {
                    _ConsumerSecret = ConfigurationHelper.ConsumerSecret;
                }
                return _ConsumerSecret;
            }
        }

        private static APIClient apiClient = new APIClient(FastConnectUrl, ConsumerId, ConsumerSecret);

        public static Dictionary<string, SecuritiesListResponseModel> _dicCompanyStock = new Dictionary<string, SecuritiesListResponseModel>();

        public static Dictionary<string, List<MarketDataQuote>> _dicPriceStock = new Dictionary<string, List<MarketDataQuote>>();

        public static List<IndicatorStock> listIndicatorStockBatDay = new List<IndicatorStock>(); // các mã sẽ gửi room

        public static List<IndicatorStock> listIndicatorStockCE = new List<IndicatorStock>(); // các mã sẽ gửi room

        public static List<IndicatorStock> listIndicatorStockAnToan = new List<IndicatorStock>(); // các mã sẽ gửi room


        /// <summary>
        /// Dic các công ty không càn quét trong vòng 14 ngày
        /// </summary>
        public static Dictionary<string, DateTime> _dicCompanyNotProcessStock = new Dictionary<string, DateTime>();

        public static async Task GetCompany()
        {
            try
            {
                string[] market = { "HOSE", "HNX", "UPCOM" };

                foreach (var item in market)
                {
                    int index = 1;
                    int pagesize = 1000;
                    var re = await GetSecuritiesList(apiClient, item, index, pagesize);
                    if (re != null && re.Data != null && re.Data.Count > 0)
                    {
                        re.Data.ForEach(x =>
                        {
                            AddDicCompany(x);
                        });

                        //Nếu tổng số sản phẩm TotalRecord lớn hơn pagezie thì tiếp tục gọi trang tiếp theo
                        if (re.TotalRecord > pagesize)
                        {
                            var countIndex = (re.TotalRecord / pagesize) + 1;

                            if (countIndex > index)
                            {
                                for (int i = 0; i < re.Data.Count; i++)
                                {
                                    index++;
                                    var reII = await GetSecuritiesList(apiClient, item, index, pagesize);
                                    if (reII != null && reII.Data != null && reII.Data.Count > 0)
                                    {
                                        reII.Data.ForEach(x =>
                                        {
                                            AddDicCompany(x);
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                GenFileClass.CreateLogDataEvent($"Lấy danh sách trên hệ thống SSI thành công");
                Console.WriteLine($"{DateTime.Now} | Lấy danh sách trên hệ thống SSI thành công");
            }
            catch (Exception ex)
            {
                GenFileClass.CreateLogErrorEvent($"Lấy danh sách trên hệ thống SSI không thành công");
                Console.WriteLine($"{DateTime.Now} | Lấy danh sách trên hệ thống SSI không thành công");
            }
        }

        /// <summary>
        /// Gọi API danh sách công ty
        /// </summary>
        /// <param name="apiClient"></param>
        /// <param name="market"></param>
        /// <param name="index"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        private static async Task<SecuritiesListResponse> GetSecuritiesList(APIClient apiClient, string market, int index, int pagesize)
        {
            var rq = new SecuritiesListRequest()
            {
                Market = market,
                PageIndex = index,
                PageSize = pagesize
            };

            return await apiClient.GetSecuritiesList(rq);
        }

        public static Task GetPriceStock()
        {
            var date = DateTime.Today.AddDays(-200);

            var lstPrice = SqlData.GetAllPriceStock(date);
            if (lstPrice != null && lstPrice.Count > 0)
            {
                lstPrice.ForEach(item =>
                {
                    var obj = new MarketDataQuote
                    {
                        Date = item.Date,
                        Open = item.Open,
                        High = item.High,
                        Low = item.Low,
                        Close = item.Close,
                        Volume = item.Volume,
                    };

                    // chưa có thì tạo list mới
                    if (!_dicPriceStock.ContainsKey(item.Symbol))
                    {
                        _dicPriceStock.Add(item.Symbol, new List<MarketDataQuote>());
                    }
                    _dicPriceStock[item.Symbol].Add(obj);
                });
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Add công ty vào dic
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static Task AddDicCompany(SecuritiesListResponseModel item)
        {
            if (item != null && !_dicCompanyStock.ContainsKey(item.Symbol))
            {
                _dicCompanyStock.Add(item.Symbol, item);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Get lấy dữ liệu theo ngày
        /// </summary>
        /// <returns></returns>
        public static Task ProcessGet()
        {
            try
            {
                var fromDate = SqlData.GetMaxDate();

                if (fromDate != DateTime.MinValue && DateTime.Today > fromDate)
                {
                    var toDate = DateTime.Today;

                    while (fromDate < toDate)
                    {
                        var endDate = fromDate.AddDays(30);

                        if (endDate > DateTime.Today)
                        {
                            endDate = DateTime.Today;
                        }

                        var rq = new DailyOHLCRequest
                        {
                            FromDate = fromDate.ToString("dd/MM/yyyy"),
                            ToDate = endDate.ToString("dd/MM/yyyy"),
                            PageIndex = 1,
                            PageSize = 100,
                            ascending = true,
                        };

                        _dicCompanyStock.ToList().ForEach(x =>
                        {
                            if (x.Value.Symbol.Length == 3)
                            {
                                rq.Symbol = x.Value.Symbol; // mã CK

                                var re = apiClient.GetDailyOhlc(rq).Result;
                                System.Threading.Thread.Sleep(1000);
                                if (re != null && re.Data != null && re.Data.Count > 0)
                                {
                                    foreach (var item in re.Data)
                                    {
                                        SymbolQuote quote = new SymbolQuote
                                        {
                                            Symbol = item.Symbol,
                                            Date = DateTime.ParseExact(item.TradingDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                            Open = decimal.Parse(item.Open),
                                            High = decimal.Parse(item.High),
                                            Low = decimal.Parse(item.Low),
                                            Close = decimal.Parse(item.Close),
                                            Volume = decimal.Parse(item.Volume)
                                        };

                                        SqlData.Insert(quote);

                                        // chưa có thì tạo list mới
                                        if (!_dicPriceStock.ContainsKey(quote.Symbol))
                                        {
                                            _dicPriceStock.Add(quote.Symbol, new List<MarketDataQuote>());
                                        }
                                        _dicPriceStock[quote.Symbol].Add(new MarketDataQuote
                                        {
                                            Date = quote.Date,
                                            Open = quote.Open,
                                            High = quote.High,
                                            Low = quote.Low,
                                            Close = quote.Close,
                                            Volume = quote.Volume
                                        });
                                    }
                                }
                            }
                        });

                        //
                        fromDate = endDate.AddDays(1);
                    }
                }
            }
            catch (Exception ex)
            {
                GenFileClass.CreateLogErrorEvent(string.Format("ProcessGet: {0}", ex.Message));
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Xử lý tìm kiếm
        /// </summary>
        /// <returns></returns>
        public static Task ProcessIndicatorDaily()
        {
            try
            {
                // kiểm tra và lấy thêm dữ liệu nếu thiếu
                ProcessGet();

                _dicCompanyStock.ToList().ForEach(x =>
                {
                    if (x.Value.Symbol.Length == 3)
                    {
                        if (!_dicCompanyNotProcessStock.ContainsKey(x.Value.Symbol))
                        {
                            var quotes = _dicPriceStock[x.Value.Symbol];

                            ProcessIndicatorStock.ProcessIndicatorStockVN.IndicatorBatDay(x.Value, quotes);

                            ProcessIndicatorStock.ProcessIndicatorStockVN.IndicatorCE(x.Value, quotes);

                            ProcessIndicatorStock.ProcessIndicatorStockVN.IndicatorAnToan(x.Value, quotes);
                        }
                        else
                        {
                            if (_dicCompanyNotProcessStock.TryGetValue(x.Value.Symbol, out var value) && DateTime.Now > value)
                            {
                                _dicCompanyNotProcessStock.Remove(x.Value.Symbol);
                            }
                        }
                    }
                });

                SendIndicator();
            }
            catch (Exception ex)
            {
                GenFileClass.CreateLogErrorEvent(string.Format("ProcessIndicatorDaily: {0}", ex.Message));
            }
            return Task.CompletedTask;
        }

        private static Task SendIndicator()
        {
            try
            {
                // Quét gửi thông báo mã bắt đáy
                if (listIndicatorStockBatDay != null && listIndicatorStockBatDay.Count > 0)
                {
                    SendTelegram.SendIndicator.Send(RoomType.Stock, StringHelper.NotifyDays(IndicatorEnum.BatDayPhuDM));
                    System.Threading.Thread.Sleep(2000); // 2s
                    for (int i = 0; i < listIndicatorStockBatDay.Count; i++)
                    {
                        SendTelegram.SendIndicator.Send(RoomType.Stock, StringHelper.NotifyIndicator(listIndicatorStockBatDay[i]));
                        System.Threading.Thread.Sleep(15000); // 15s
                    }
                }
                listIndicatorStockBatDay.Clear();

                // Quét gửi thông báo mã ce
                if (listIndicatorStockCE != null && listIndicatorStockCE.Count > 0)
                {
                    SendTelegram.SendIndicator.Send(RoomType.Stock, StringHelper.NotifyDays(IndicatorEnum.CEPhuDM));
                    System.Threading.Thread.Sleep(2000); // 2s
                    for (int i = 0; i < listIndicatorStockCE.Count; i++)
                    {
                        SendTelegram.SendIndicator.Send(RoomType.Stock, StringHelper.NotifyIndicator(listIndicatorStockCE[i]));
                        System.Threading.Thread.Sleep(15000); // 15s
                    }
                }
                listIndicatorStockCE.Clear();

                // Quét gửi thông báo mã an toàn
                if (listIndicatorStockAnToan != null && listIndicatorStockAnToan.Count > 0)
                {
                    SendTelegram.SendIndicator.Send(RoomType.Stock, StringHelper.NotifyDays(IndicatorEnum.AnToanPhuDM));
                    System.Threading.Thread.Sleep(2000); // 2s
                    for (int i = 0; i < listIndicatorStockAnToan.Count; i++)
                    {
                        SendTelegram.SendIndicator.Send(RoomType.Stock, StringHelper.NotifyIndicator(listIndicatorStockAnToan[i]));
                        System.Threading.Thread.Sleep(15000); // 15s
                    }
                }
                listIndicatorStockAnToan.Clear();
            }
            catch (Exception ex)
            {
                GenFileClass.CreateLogErrorEvent(string.Format("SendIndicator: {0}", ex.Message));
            }
            return Task.CompletedTask;
        }
    }
}
