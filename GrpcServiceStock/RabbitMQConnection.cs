namespace GrpcServiceLab
{
    public class RabbitMQConnection
    {
        public string HostName { get; set; }

        public int Port { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string VirtualHost { get; set; }

        public string Exchange { get; set; }

        public string RouteKey { get; set; }

        public string System { get; set; }
    }
}
