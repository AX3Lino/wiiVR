using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using vJoyInterfaceWrap;
using WiimoteLib;

namespace WiimoteTest
{
    class CalcLogic
    {
        private vJoy vjoy;
        private uint id;

        public CalcLogic(vJoy vjoy, uint id)
        {
            this.vjoy = vjoy;
            this.id = id;
        }

        public void setMPS(Point3 values)
        {
            Debug.WriteLine("Got MPS");
        }

        public void setIR(WiimoteLib.Point value)
        {
            Debug.WriteLine("Got IR");
        }

    }
}
