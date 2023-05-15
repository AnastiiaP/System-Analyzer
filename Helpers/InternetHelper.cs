using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using SystemAnalyzer.Configurations;

namespace SystemAnalyzer.Helpers
{
    public  class InternetHelper
    {
        private AppSetting _appSetting;
        private ILogger<Worker> _logger;

        public InternetHelper(ILogger<Worker> logger, AppSetting appSetting)
        {
            _logger = logger;
            _appSetting = appSetting;
        }
        public async Task<bool> IsConnectedToInternetAsync()
        {
            int maxHops = Convert.ToInt32(_appSetting.MaxInternetHops);
            string someFarAwayIpAddress = _appSetting.FarAwayIpAddress;
            int timeOut = Convert.ToInt32(_appSetting.TimeoutForHops);

            for (int ttl = 1; ttl <= maxHops; ttl++)
            {
                var options = new PingOptions(ttl, true);
                byte[] buffer = new byte[32];
                PingReply reply;
                try
                {
                    using (var pinger = new Ping())
                    {
                        reply = await pinger.SendPingAsync(someFarAwayIpAddress, timeOut, buffer, options);
                    }
                }
                catch (PingException pingex)
                {
                    return false;
                }

                string address = reply.Address?.ToString() ?? null;

                if (reply.Status != IPStatus.TtlExpired && reply.Status != IPStatus.Success)
                {
                    return false;
                }

                if (IsRoutableAddress(reply.Address))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsRoutableAddress(IPAddress addr)
        {
            if (addr == null)
            {
                return false;
            }
            else if (addr.AddressFamily == AddressFamily.InterNetworkV6)
            {
                return !addr.IsIPv6LinkLocal && !addr.IsIPv6SiteLocal;
            }
            else
            {
                byte[] bytes = addr.GetAddressBytes();
                if (bytes[0] == 10)
                {
                    return false;
                }
                else if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                {
                    return false;
                }
                else if (bytes[0] == 192 && bytes[1] == 168)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public async Task<bool> AnalyzeInternetConnection(bool lastInternetState)
        {
            bool isConnected = await IsConnectedToInternetAsync();
            if (isConnected && !lastInternetState)
            {
                _logger.LogInformation("{time} зміна стану Інтернет зєднання - Підключено", DateTimeOffset.Now);
                return true;
            }
            else if (!isConnected && lastInternetState)
            {
                _logger.LogInformation("{time} зміна стану Інтернет зєднання - Відключено", DateTimeOffset.Now);
                return false;
            }
            return lastInternetState;
        }

        public async Task<bool> GetInternetConnectionState()
        {
            bool isConnected = await IsConnectedToInternetAsync();
            if (isConnected)
            {
                _logger.LogInformation("{time} cтан Інтернет зєднання - Підключено", DateTimeOffset.Now);
                return true;
            }
            else
            {
                _logger.LogInformation("{time} cтан Інтернет зєднання - Відключено", DateTimeOffset.Now);
                return false;
            }

        }
    }
}
