using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Phidgets;
using Phidgets.Events; //Needed for the phidget event handling classes

namespace Ja.SignalR.Phidget.Models
{
    public class PhidgetRFIDModule : PhidgetModuleProperties
    {
        public int Outputs { get; set; }
        public bool LED { get; set; }
        public bool Antenna { get; set; }
        public bool KeyBoardOutPutEnable { get; set; }
        public bool[] DigitalOutPuts { get; set; }
        public string Tag { get; set; }
        public bool TagPresent { get; set; }
        public string LastTag { get; set; }
        public string Protocol { get; set; }
        public string LastTagProtocol { get; set; }
        public PhidgetRFIDModule(RFID rfid) : base(rfid)
        {
            this.Antenna = rfid.Antenna;
            this.LED = rfid.LED;
            this.Outputs = rfid.outputs.Count;
            this.DigitalOutPuts = new bool[this.Outputs];
            // External LED
            this.DigitalOutPuts[0] = rfid.outputs[0];
            // External +5v
            this.DigitalOutPuts[1] = rfid.outputs[1];
        }
        public override PhidgetModuleProperties GetData(PhidgetModuleProperties phidgetData)
        {
            // Data to return.
            PhidgetRFIDModule pd = (PhidgetRFIDModule)phidgetData;

            pd.Antenna = this.Antenna;
            pd.LED = this.LED;
            pd.Outputs = this.Outputs;
            pd.DigitalOutPuts[0] = this.DigitalOutPuts[0];
            pd.DigitalOutPuts[1] = this.DigitalOutPuts[1];
            pd.KeyBoardOutPutEnable = this.KeyBoardOutPutEnable;
            pd.LastTag = this.LastTag;
            pd.LastTagProtocol = this.LastTag;

            return pd;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="phidgetData"></param>
        /// <param name="phidgetModule"></param>
        public override void SetData(dynamic phidgetData, dynamic phidgetModule)
        {
            // Data to set and to cache.
            RFID _phidgetModule = (RFID)phidgetModule;

            _phidgetModule.Antenna = this.Antenna = phidgetData.Antenna;
            _phidgetModule.LED = this.LED = phidgetData.LED;
            _phidgetModule.outputs[0] = this.DigitalOutPuts[0] = phidgetData.DigitalOutPuts[0];
            _phidgetModule.outputs[1] = this.DigitalOutPuts[1] = phidgetData.DigitalOutPuts[1];
        }
    }
}