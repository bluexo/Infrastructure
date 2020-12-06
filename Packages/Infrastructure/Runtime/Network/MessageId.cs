namespace Mecha.Network
{
    public static class MessageId
    {
        public const byte Login = 1;

        public const byte ClientEnterGame = 10;
        public const byte ClientReady = 11;
        public const byte ClientNotReady = 12;
        public const byte ClientInput = 13;

        public const byte CallFunction = 18;
        public const byte UpdateSyncField = 19;
        public const byte InitialSyncField = 20;
        public const byte OperateSyncList = 21;

        public const byte ServerGameStart = 113;
        public const byte ServerSpawnPlayer = 115;
        public const byte ServerSpawnObject = 116;
        public const byte ServerDestroyObject = 117;
        public const byte ServerTime = 122;
        public const byte ServerError = 123;
        public const byte ServerReplication = 124;
    }

    public class DestroyObjectReasons
    {
        public const byte RequestedToDestroy = 0;
        public const byte RemovedFromSubscribing = 1;
    }
}