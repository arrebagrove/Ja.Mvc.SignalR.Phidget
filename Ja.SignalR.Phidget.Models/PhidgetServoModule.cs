using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Phidgets;

namespace Ja.SignalR.Phidget.Models
{
    public class PhidgetServoModule : PhidgetModuleProperties
    {
        public int ServoNumber { get; set; }
        public int Count { get; set; }
        public string[] Type { get; set; }
        public double[] Position { get; set; }
        //public double[] Degrees { get; set; }
        //public double[] MinimumPulseWidth { get; set; }
        //public double[] MaximumPulseWidth { get; set; }
        public double[] PositionMin { get; set; }
        public double[] PositionMax { get; set; }
        public bool[] Engaged { get; set; }
        public double[] PositionTrackBar { get; set; }

        public PhidgetServoModule(Servo servo) : base(servo)
        {
            const double InitPosition = 10.00;
            //const double InitMinimumPulseWidth = 600.00;
            //const double InitMaximumPulseWidth = 2000.00;
            //const double InitDegrees = 120.00;
            const bool InitEngaged = true;

            // We have only one motor
            this.ServoNumber = 0;

            this.Count = servo.servos.Count;
            this.Type = new string[servo.servos.Count];
            this.Position = new double[servo.servos.Count];
            //this.Degrees = new double[servo.servos.Count];
            //this.MinimumPulseWidth = new double[servo.servos.Count];
            //this.MaximumPulseWidth = new double[servo.servos.Count];
            this.PositionMin = new double[servo.servos.Count];
            this.PositionMax = new double[servo.servos.Count];
            this.Engaged = new bool[servo.servos.Count];

            //Set the default servo type to the one Phidgets sells
            foreach (ServoServo motor in servo.servos)
                motor.Type = ServoServo.ServoType.HITEC_HS322HD;

            for (var i = 0; i < servo.servos.Count; i++)
            {
                this.Type[i] = servo.servos[i].Type.ToString();
                this.Position[i] = servo.servos[i].Position = InitPosition;
                //this.Degrees[i] = InitDegrees;
                //this.MinimumPulseWidth[i] = InitMinimumPulseWidth;
                //this.MaximumPulseWidth[i] = InitMaximumPulseWidth;
                this.PositionMin[i] = servo.servos[i].PositionMin;
                this.PositionMax[i] = servo.servos[i].PositionMax;
                this.Engaged[i] = InitEngaged;
            }

            //Setting custom servo parameters example - 600us-2000us == 120 degrees
            //servo.servos[this.ServoNumber].setServoParameters(InitMinimumPulseWidth, InitMaximumPulseWidth, InitDegrees);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="phidgetData"></param>
        /// <returns></returns>
        public override PhidgetModuleProperties GetData(PhidgetModuleProperties phidgetData)
        {
            // Data to return.
            PhidgetServoModule pd = (PhidgetServoModule)phidgetData;

            pd.Engaged[pd.ServoNumber] = this.Engaged[pd.ServoNumber];
            pd.Position[pd.ServoNumber] = this.Position[pd.ServoNumber];

            return pd;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="phidgetData"></param>
        /// <returns></returns>
        public override void SetData(dynamic phidgetData, dynamic phidgetModule)
        {
            // Data to set and to cache.
            Servo _phidgetModule = (Servo)phidgetModule;
            var i = Convert.ToInt32(phidgetData.ServoNumber);

            //A PhidgetException will be thrown if you 
            //try to set the position to any value NOT between -23 and 232
            try
            {
                _phidgetModule.servos[i].Engaged = this.Engaged[i] = phidgetData.Engaged[i];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            this.Position[i] = Convert.ToDouble(phidgetData.Position[i]);

            if (this.Engaged[i])
            {
                try
                {
                    _phidgetModule.servos[i].Position = this.Position[i];
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

        }
    }
}