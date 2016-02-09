using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Phidgets;

namespace Ja.SignalR.Phidget.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class PhidgetStarterKitModule : PhidgetModuleProperties
    {
        public int DigitalInputCount { get; set; }
        public int DigitalOutputCount { get; set; }
        public int AnalogInputCount { get; set; }
        public bool Ratiometric { get; set; }
        public bool[] DigitalInput { get; set; }
        public bool[] DigitalOutput { get; set; }
        public decimal[] AnalogInput { get; set; }
        public int Deadband { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ifKit"></param>
        public PhidgetStarterKitModule(InterfaceKit ifKit) : base(ifKit)
        {
            this.Ratiometric = ifKit.ratiometric;

            this.DigitalInputCount = ifKit.inputs.Count;
            this.DigitalInput = new bool[this.DigitalInputCount];
            for (int i = 0; i< this.DigitalInputCount; i++)
            {
                this.DigitalInput[i] = ifKit.inputs[i];
            }

            this.DigitalOutputCount = ifKit.outputs.Count;
            this.DigitalOutput = new bool[this.DigitalOutputCount];
            for (int i = 0; i < this.DigitalOutputCount; i++)
            {
                this.DigitalOutput[i] = ifKit.outputs[i];
            }

            this.AnalogInputCount = ifKit.sensors.Count;
            this.AnalogInput = new decimal[this.AnalogInputCount];
            for (int i = 0; i < this.AnalogInputCount; i++)
            {
                this.AnalogInput[i] = ifKit.sensors[i].Value;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="phidgetData"></param>
        /// <returns></returns>
        public override PhidgetModuleProperties GetData(PhidgetModuleProperties phidgetData)
        {
            // Data to return.
            PhidgetStarterKitModule pd = (PhidgetStarterKitModule)phidgetData;

            for (int i = 0; i < this.DigitalOutputCount; i++)
            {
                pd.DigitalOutput[i] = this.DigitalOutput[i];
            }
            for (int i = 0; i < this.DigitalInputCount; i++)
            {
                pd.DigitalInput[i] = this.DigitalInput[i];
            }
            for (int i = 0; i < this.AnalogInputCount; i++)
            {
                pd.AnalogInput[i] = this.AnalogInput[i];
            }
            pd.Ratiometric = this.Ratiometric;

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
            InterfaceKit _phidgetModule = (InterfaceKit)phidgetModule;

            for (int i = 0; i < this.DigitalOutputCount; i++)
            {
                _phidgetModule.outputs[i] = this.DigitalOutput[i] = phidgetData.DigitalOutput[i];
            }
            _phidgetModule.ratiometric = this.Ratiometric = phidgetData.Ratiometric;
        }

    }
}