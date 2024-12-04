using Google.Protobuf.WellKnownTypes;
using GrpcServiceStock.Common;
using GrpcServiceStock.Response;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using static GrpcServiceStock.Enum.EnumHelper;

namespace GrpcServiceStock.SendTelegram
{
    public class SendIndicator
    {
        private static readonly HttpClient client = new HttpClient();

        private static string _LinkTelegram = null;

        private static string LinkTelegram
        {
            get
            {
                if (_LinkTelegram == null)
                {
                    _LinkTelegram = ConfigurationHelper.LinkTelegram;
                }
                return _LinkTelegram;
            }
        }

        private static RoomStockVN _RoomStockVN = null;

        private static RoomStockVN RoomStockVN
        {
            get
            {
                if (_RoomStockVN == null)
                {
                    _RoomStockVN = ConfigurationHelper.RoomStockVN;
                }
                return _RoomStockVN;
            }
        }

        private static RoomCoin _RoomCoin = null;

        private static RoomCoin RoomCoin
        {
            get
            {
                if (_RoomCoin == null)
                {
                    _RoomCoin = ConfigurationHelper.RoomCoin;
                }
                return _RoomCoin;
            }
        }

        public static async Task Send(RoomType roomType, string text)
        {
            try
            {
                var strUrl = string.Empty;
                switch (roomType)
                {
                    case RoomType.Stock:
                        strUrl = string.Format("{0}/{1}/sendMessage?chat_id=@{2}&text={3}", LinkTelegram, RoomStockVN.BotCode, RoomStockVN.ChatId, text);
                        break;
                    case RoomType.Coin:
                        strUrl = string.Format("{0}/{1}/sendMessage?chat_id=@{2}&text={3}", LinkTelegram, RoomCoin.BotCode, RoomCoin.ChatId, text);
                        break;
                    default:
                        break;
                }
                await client.GetAsync(strUrl);
            }
            catch (HttpRequestException e)
            {
                // Handle request exceptions
                Console.WriteLine($"Request error: {e.Message}");
            }
        }
    }
}
