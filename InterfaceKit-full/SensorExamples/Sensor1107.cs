using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace InterfaceKit_full.SensorExamples
{
    public partial class Sensor1107 : UserControl
    {
        public int sensorValue;
        public Sensor1107()
        {
            InitializeComponent();
        }

        public void changeDisplay(int val)
        {
            double tmp = (val * 0.1906) - 40.2;
            textBox1.Text = tmp.ToString() + "%";
        }

    }
}
