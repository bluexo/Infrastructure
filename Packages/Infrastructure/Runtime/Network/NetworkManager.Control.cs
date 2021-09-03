using System;
using System.Timers;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Love.Network
{
    /// <summary>
    /// 网络管理器
    /// </summary>
    public partial class NetworkManager
    {
        public event EventHandler<int> ClientEnterGameEvent;
        public event EventHandler<(int, string)> ClientReadyEvent;
        public event EventHandler<(short, string)> ServerErrorEvent;

        private readonly DateTime MinDateTime = new DateTime(1970, 1, 1);
        public long Timestamp => DateTime.UtcNow.Subtract(MinDateTime).Ticks / 10000;
        public long ServerUnixTime => IsServer ? Timestamp : Timestamp + ServerUnixTimeOffset;
        public long ServerUnixTimeOffset { get; protected set; }

        public long ServerTime => IsServer ? Timer.UnscaledTime : Timer.UnscaledTime + ServerTimeOffset;
        public long ServerTimeOffset { get; protected set; }

        public INetworkedAssets NetworkAssets { get; protected set; }
        public NetworkTransport Transport { get; protected set; }

        public bool IsServer => Transport.IsServer;
        public bool IsNetworkActive => Transport != null && Transport.IsStarted;

        public readonly NetworkTimer Timer = new NetworkTimer();
        private const float updateServerTimeDuration = 1000f;
        public string _networkAddress = "localhost";
        private int _networkPort = 9050;
        private float lastSendServerTime, lastUpdateTime;

        public NetworkManager(NetworkTransport network, INetworkedAssets networkAssets)
        {
            Transport = network;
            NetworkAssets = networkAssets;
        }

        public void StartServer(int port)
        {
            Timer.Start();
            RegisterServerMessages();
            _networkPort = port;
            if (Transport == null || !(Transport is NetworkServer server))
                throw new NullReferenceException($"{Transport.GetType()}!");
            server.Start(_networkPort);
            NetworkAssets.Initialize();
            Log.Info(this, $"Server start at port: {_networkPort}");
        }

        public void StartClient(string networkAddress, int networkPort)
        {
            Timer.Start();
            RegisterClientMessages();
            _networkAddress = networkAddress;
            _networkPort = networkPort;
            if (Transport == null || !(Transport is NetworkClient client))
                throw new NullReferenceException($"{Transport.GetType()}!");
            if (!client.Start(networkAddress, networkPort))
            {
                Log.Error(this, $"Client start fail!");
                return;
            }
            NetworkAssets.Initialize();
            Log.Info(this, "Client connecting to " + networkAddress + ":" + networkPort);
        }

        public void Update()
        {
            if (!IsNetworkActive) return;

            var deltaTime = Timer.UnscaledTime - lastUpdateTime;

            Transport.Update();

            foreach (NetworkObject spawnedObject in NetworkAssets.NetworkObjects.Values)
            {
                if (spawnedObject == null)
                    continue;
                spawnedObject.NetworkUpdate(deltaTime);
            }

            if (IsServer && Timer.UnscaledTime - lastSendServerTime > updateServerTimeDuration)
            {
                SendServerTime();
                lastSendServerTime = Timer.UnscaledTime;
            }

            lastUpdateTime = Timer.UnscaledTime;
        }

        public void Reset(int disconnectDelayMs = 1000)
        {
            NetworkAssets.Dispose();
            _ = Task.Factory.StartNew(async () =>
           {
               await Task.Delay(disconnectDelayMs);
               Transport.Reset();
           });
        }

        public void Stop()
        {
            if (Transport == null)
                return;

            Log.Info(this, "Stopped!");
            Transport.Close();
            Transport = null;
        }
    }
}
