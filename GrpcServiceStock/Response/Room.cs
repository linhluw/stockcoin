namespace GrpcServiceStock.Response
{
    public class RoomStockVN : Room
    {

    }

    public class RoomCoin : Room
    {

    }

    public class Room
    {
        public string BotCode { get; set; }

        public string ChatId { get; set; }
    }
}
