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
        private Point3 calibration;

        bool calFlag = false;

        public CalcLogic(vJoy vjoy, uint id)
        {
            this.vjoy = vjoy;
            this.id = id;
        }

        public void setMPS(Point3 values)
        {
            //For better performance change this to something else!
            if(calFlag) {
                calibration = values;
                calFlag = false;
            }

            Debug.WriteLine("Got MPS");

        }

        public void setIR(WiimoteLib.Point value)
        {
            Debug.WriteLine("Got IR");
        }

        public void setMPSCalibration()
        {
            calFlag = true;
        }

    }
}
