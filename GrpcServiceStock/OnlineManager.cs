using System;
using System.Timers;
using GrpcServiceStock.Modules;
using System.Collections.Generic;
using GrpcServiceStock.Response;
using GrpcServiceStock.SendTelegram;
using GrpcServiceStock.Common;
using System.IO;
using GrpcServiceStock.Enum;

namespace GrpcServiceStock
{
    public class OnlineManager
    {
        private static System.Timers.Timer _RefreshSSITimer = new System.Timers.Timer();

        private static System.Timers.Timer _RefreshCoinTimer = new System.Timers.Timer();

        private static DateTime refreshCoinTime = DateTime.Today;

        private static DateTime refreshSSITime = DateTime.Today.AddHours(18); // xử lý vào lúc 4h chiều

        /// <summary>
        /// Gọi hàm khởi tạo
        /// </summary>
        public static async void Init()
        {
            try
            {
                Console.WriteLine($"{DateTime.Now} | Bắt đầu khởi tạo service");
                GenFileClass.CreateLogDataEvent("Bắt đầu khởi tạo service");

                if (ConfigurationHelper.IsModuleSSI)
                {
                    // Reset Timer => 5 phút kiểm tra 1 lần 300000
                    _RefreshSSITimer.Interval = 300000;
                    _RefreshSSITimer.Elapsed += new System.Timers.ElapsedEventHandler(RefreshSSITimer_Elapsed);
                    _RefreshSSITimer.Enabled = true;

                    // lấy danh sách công ty trên hệ thông SSI
                    await SSIDataStock.GetCompany();

                    Console.WriteLine($"{DateTime.Now} | Lấy dữ liệu DataStock Company thành công");
                    GenFileClass.CreateLogDataEvent("Lấy dữ liệu DataStock Company thành công");

                    await SSIDataStock.GetPriceStock();

                    Console.WriteLine($"{DateTime.Now} | Lấy dữ liệu PriceStock thành công");
                    GenFileClass.CreateLogDataEvent("Lấy dữ liệu PriceStock Company thành công");
                }

                if (ConfigurationHelper.IsModuleCoin)
                {
                    // Reset Timer => 30 phút kiểm tra 1 lần 1800000
                    _RefreshCoinTimer.Interval = 1800000;
                    _RefreshCoinTimer.Elapsed += new System.Timers.ElapsedEventHandler(RefreshCoinTimer_Elapsed);
                    _RefreshCoinTimer.Enabled = true;

                    string filePath = AppDomain.CurrentDomain.BaseDirectory + string.Format(@"{0}", "CoinList.txt");

                    // Read all lines into an array
                    string[] lines = File.ReadAllLines(filePath);

                    foreach (string line in lines)
                    {
                        var item = line.ToUpper();
                        CoinDataStock._dicCoin.Add(item.Replace("/", string.Empty), item);
                    }

                    Console.WriteLine($"{DateTime.Now} | Lấy dữ liệu DataCoin thành công");
                    GenFileClass.CreateLogDataEvent("Lấy dữ liệu DataCoin thành công");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} | Khởi tạo phiên bản lỗi");
            }
        }

        /// <summary>
        /// Thực hiện quét
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void RefreshSSITimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // xử lý SSI vào 4h chiều
            if (DateTime.Now > refreshSSITime)
            {
                // xử lý xong add thời gian vào ngày hôm sau
                refreshSSITime = DateTime.Today.AddDays(1).AddHours(18);

                // thứ 7 CN nghỉ nên ko cần chạy phân tích
                if ((DateTime.Now.DayOfWeek != DayOfWeek.Saturday) || (DateTime.Now.DayOfWeek == DayOfWeek.Sunday))
                {
                    GenFileClass.CreateLogDataEvent("Bắt đầu tiến trình Stock SSI");
                    SSIDataStock.ProcessIndicatorDaily();
                    GenFileClass.CreateLogDataEvent("Kết thúc tiến trình Stock SSI");
                    GC.Collect();
                }
            }
        }

        /// <summary>
        /// Thực hiện quét
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void RefreshCoinTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // xử lý qua ngày
            if (DateTime.Now > refreshCoinTime)
            {
                CoinDataStock._dicCoinNotProcessStock.Clear();
                refreshCoinTime = DateTime.Today.AddDays(1);
            }
            GenFileClass.CreateLogDataEvent("Bắt đầu tiến trình Coin Binance");
            CoinDataStock.ProcessIndicatorDaily();
            GenFileClass.CreateLogDataEvent("Kết thúc tiến trình Coin Binance");
            GC.Collect();
        }
    }
}
