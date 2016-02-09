using Phidgets; //Needed for the RFID class and the PhidgetException class

namespace Ja.SignalR.Phidget.Models
{
    public abstract class PhidgetModuleProperties
    {
        public bool Attached { get; set; }
        public string Name { get; set; }
        public string ID { get; set; }
        public string Class { get; set; }
        public int SerialNumber { get; set; }
        public int Version { get; set; }
        public string PhidgetModuleType { get; set; }
        public abstract PhidgetModuleProperties GetData(PhidgetModuleProperties phidgetData);
        public abstract void SetData(dynamic phidgetData, dynamic phidgetModule);
        public PhidgetModuleProperties(dynamic phidgetModule)
        {
            this.Attached = phidgetModule.Attached;
            this.Name = phidgetModule.Name;
            this.ID = phidgetModule.ID.ToString();
            this.Class = phidgetModule.Class.ToString();
            this.SerialNumber = phidgetModule.SerialNumber;
            this.Version = phidgetModule.Version;
        }
    }
}