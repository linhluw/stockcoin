using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using System.ComponentModel;
using GrpcServiceStock.Response;

namespace GrpcServiceStock.Common
{
    public static class ConfigurationHelper
    {
        private static IConfiguration config;
        public static IConfiguration Configuration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                //.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
                .AddJsonFile($"appsettings.json", true)
                .AddEnvironmentVariables();
            config = builder.Build();
            return config;
        }

        public static T GetConfiguration<T>(string SectionName) where T : class
        {
            if (typeof(T).Name == "String")
            {
                return (T)Convert.ChangeType(config.GetSection(SectionName).Value, typeof(T));
            }
            else
            {
                var section = (T)Activator.CreateInstance(typeof(T));
                config.GetSection(SectionName).Bind(section);
                return section;
            }
        }

        /// <summary>
        /// Link kết nối API SSI
        /// </summary>
        public static string LinkTelegram
        {
            get
            {
                return GetConfigValue("LinkTelegram", string.Empty);
            }
        }


        /// <summary>
        /// Room Telegram về chứng khoán Việt Nam
        /// </summary>
        public static RoomStockVN RoomStockVN
        {
            get
            {
                return GetConfiguration<RoomStockVN>("RoomStockVN");
            }
        }

        /// <summary>
        /// Room Telegram về coin
        /// </summary>
        public static RoomCoin RoomCoin
        {
            get
            {
                return GetConfiguration<RoomCoin>("RoomCoin");
            }
        }

        /// <summary>
        /// Doc thong tin cau hinh trong appSettings
        /// </summary>
        /// <param name="configKey"></param>
        /// <returns></returns>
        private static string GetConfigValueAsString(string configKey)
        {
            var value = ConfigurationHelper.GetConfiguration<string>(configKey);

            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
            return null;
        }

        private static T GetConfigValue<T>(string configKey, T defaultValue)
        {
            var value = defaultValue;

            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null)
            {
                var setting = GetConfigValueAsString(configKey.ToString());
                if (!string.IsNullOrEmpty(setting))
                {
                    value = (T)converter.ConvertFromString(setting);
                }
            }
            return value;
        }

        /// <summary>
        /// Link kết nối API SSI
        /// </summary>
        public static string DefaultConnection
        {
            get
            {
                return GetConfigValue("DefaultConnection", string.Empty);
            }
        }

        /// <summary>
        /// Link kết nối API SSI
        /// </summary>
        public static string FastConnectUrl
        {
            get
            {
                return GetConfigValue("FastConnectUrl", string.Empty);
            }
        }

        /// <summary>
        /// Tài khoản kết nối
        /// </summary>
        public static string ConsumerId
        {
            get
            {
                return GetConfigValue("ConsumerId", string.Empty);
            }
        }

        /// <summary>
        /// Khóa kết nối
        /// </summary>
        public static string ConsumerSecret
        {
            get
            {
                return GetConfigValue("ConsumerSecret", string.Empty);
            }
        }

        /// <summary>
        /// Chạy Module SSI
        /// </summary>
        public static bool IsModuleSSI
        {
            get
            {
                return GetConfigValue<bool>("IsModuleSSI", false);
            }
        }

        /// <summary>
        /// Chạy Binance Coin
        /// </summary>
        public static bool IsModuleCoin
        {
            get
            {
                return GetConfigValue<bool>("IsModuleCoin", false);
            }
        }

    }
}
