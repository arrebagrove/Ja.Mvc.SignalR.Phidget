namespace Ja.SignalR.Phidget.Models
{
    /// <summary>
    /// SignalRClient can be a PhidgetServer or a PhidgetClient.
    /// </summary>
    public static class SignalRClientType
    {
        public const string PhidgetServer = "PhidgetServer";
        public const string PhidgetClient = "PhidgetClient";
    }
    /// <summary>
    /// We use 3 Phidget module type.
    /// </summary>
    /// 
    public static class PhidgetModuleType
    {
        public const string PhidgetStarterKit = "PhidgetStarterKit";
        public const string PhidgetRFID = "PhidgetRFID";
        public const string PhidgetServo = "PhidgetServo";
    }
}
