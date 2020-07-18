using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using vJoyInterfaceWrap;
using WiimoteLib;

namespace WiimoteTest
{
    class Point2D
    {
        public float x = 0.0f;
        public float y = 0.0f;
        public void set(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }
    class CalcLogic
    {
        private vJoy vjoy;
        private uint id;

        bool calFlag = false;

		float rx = 0;
		float ry = 7500;
		float rz = 7500;
		float vx = 0;
		float vy = 0;
		float vz = 0;
		bool first = true;
		float kx = 7500;
		float ky = 7500;
		float kz = 7500;
		int cal = 0;
		int cp = 0;
		int[] calX = new int[10];
		int[] calY = new int[10];
		int[] calZ = new int[10];
		int[] cpX = new int[10];
		int[] cpY = new int[10];
		int[] cpZ = new int[10];
		float deltaX = 0;
		float deltaY = 0;
		float deltaZ = 0;
		float dX = 0;
		float dY = 0;
		float dZ = 0;
		float exX = 0;
		float exY = 0;
		float exZ = 0;
		int avrX = 0;
		int avrY = 0;
		int avrZ = 0;



		public CalcLogic(vJoy vjoy, uint id)
        {
            this.vjoy = vjoy;
            this.id = id;
        }

        public void setMPS(WiimoteState ws)
        {
			int RX;
			int RY;
			int RZ;
			int rz0;
			if (first)
            {
                if (cal != 10)
                {
                    if (cal == 0)
                    {
						kz = ws.MotionPlusState.RawValues.Y;
					}

					avrZ += ws.MotionPlusState.RawValues.Y;
					deltaZ += Math.Abs(kz - ws.MotionPlusState.RawValues.Y);
					cal++;
                }
                else
                {
					dZ = deltaZ / 5;
					kz = avrZ / 10;
					first = false;
					Debug.WriteLine("end");
                }
            }
			//rz += kz - ws.MotionPlusState.RawValues.Y;
			/*
			//Debug.WriteLine("Z:" + rz);
			if (ws.AccelState.Values.Z<0.99)
            {
                if (ws.MotionPlusState.RawValues.Y > kz)
                {
					RZ = (int)(32767 / 2 + (1- ws.AccelState.Values.Z) * 32767 / 2);
					//Debug.WriteLine(ws.MotionPlusState.RawValues.Z - avrZ + " hore ");
				}
                else
                {
					RZ = (int)(32767 / 2 - (1- ws.AccelState.Values.Z) * 32767 / 2);
					//Debug.WriteLine(ws.MotionPlusState.RawValues.Z - avrZ + " dole ");
				}
            }
            else
            {
				//Debug.WriteLine("bola1");
				RZ = (int)(32767/2 );
			}
			*/
			RX = (int)(32767 / 2 + ws.AccelState.Values.X * 32767 / 2);
			RY = (int)(32767 / 2 - ws.AccelState.Values.Y * 32767 / 2); // Y je pseudo hotovy 
			RZ = (int)(32767 / 2 - ws.AccelState.Values.Z * 32767 / 2);

			//Debug.WriteLine(ws.MotionPlusState.RawValues.Z + "Got MPS" + ws.AccelState.RawValues.Z);
			vjoy.SetAxis(RX, id, HID_USAGES.HID_USAGE_RX);
			vjoy.SetAxis(RY, id, HID_USAGES.HID_USAGE_RY);
			vjoy.SetAxis(RZ, id, HID_USAGES.HID_USAGE_RZ);

		}

        public void setIR(WiimoteState ws)
        {
			float dotDistanceInMM = 8.5f * 25.4f;//width of the wii sensor bar
			float screenHeightinMM = 20 * 25.4f; 
			float radiansPerPixel = (float)(Math.PI / 4) / 1024.0f; //45 degree field of view with a 1024x768 camera
			float movementScaling = 1.0f;
			float relativeVerticalAngle = 0; //current head position view angle
			float cameraVerticaleAngle = 0; //begins assuming the camera is point straight forward
			Point2D[] wiimotePointsNormalized = new Point2D[4];
			int[] wiimotePointIDMap = new int[4];



			//Debug.WriteLine("Got IR");
			
			for (int i = 0; i < 4; i++)
			{
				wiimotePointsNormalized[i] = new Point2D();
				wiimotePointIDMap[i] = i;
			}
			float pX = 0;
			float pY = 0;
			float pZ = 2;

			Point2D firstPoint = new Point2D();
			Point2D secondPoint = new Point2D();
			int numvisible = 0;
			if (ws.IRState.IRSensors[0].Found)
			{
				wiimotePointsNormalized[0].x = 1.0f - ws.IRState.IRSensors[0].RawPosition.X / 768.0f;
				wiimotePointsNormalized[0].y = ws.IRState.IRSensors[0].RawPosition.Y / 768.0f;
				firstPoint.x = ws.IRState.IRSensors[0].RawPosition.X;
				firstPoint.y = ws.IRState.IRSensors[0].RawPosition.Y;
				numvisible = 1;
			}

			if (ws.IRState.IRSensors[1].Found)
			{
				wiimotePointsNormalized[1].x = 1.0f - ws.IRState.IRSensors[1].RawPosition.X / 768.0f;
				wiimotePointsNormalized[1].y = ws.IRState.IRSensors[1].RawPosition.Y / 768.0f;
				if (numvisible == 0)
				{
					firstPoint.x = ws.IRState.IRSensors[1].RawPosition.X;
					firstPoint.y = ws.IRState.IRSensors[1].RawPosition.Y;
					numvisible = 1;
				}
				else
				{
					secondPoint.x = ws.IRState.IRSensors[1].RawPosition.X;
					secondPoint.y = ws.IRState.IRSensors[1].RawPosition.Y;
					numvisible = 2;
				}
			}

			if (ws.IRState.IRSensors[2].Found)
			{
				wiimotePointsNormalized[2].x = 1.0f - ws.IRState.IRSensors[2].RawPosition.X / 768.0f;
				wiimotePointsNormalized[2].y = ws.IRState.IRSensors[2].RawPosition.Y / 768.0f;
				if (numvisible == 0)
				{
					firstPoint.x = ws.IRState.IRSensors[2].RawPosition.X;
					firstPoint.y = ws.IRState.IRSensors[2].RawPosition.Y;
					numvisible = 1;
				}
				else if (numvisible == 1)
				{
					secondPoint.x = ws.IRState.IRSensors[2].RawPosition.X;
					secondPoint.y = ws.IRState.IRSensors[2].RawPosition.Y;
					numvisible = 2;
				}
			}

			if (ws.IRState.IRSensors[3].Found)
			{
				wiimotePointsNormalized[3].x = 1.0f - ws.IRState.IRSensors[3].RawPosition.X / 768.0f;
				wiimotePointsNormalized[3].y = ws.IRState.IRSensors[3].RawPosition.Y / 768.0f;
				if (numvisible == 1)
				{
					secondPoint.x = ws.IRState.IRSensors[3].RawPosition.X;
					secondPoint.y = ws.IRState.IRSensors[3].RawPosition.Y;
					numvisible = 2;
				}

			}

			if (numvisible == 2)
			{


				float dx = firstPoint.x - secondPoint.x;
				float dy = firstPoint.y - secondPoint.y;
				float pointDist = (float)Math.Sqrt(dx * dx + dy * dy);

				float angle = radiansPerPixel * pointDist / 2;
				//in units of screen hieght since the box is a unit cube and box hieght is 1
				pZ = movementScaling * (float)((dotDistanceInMM / 2) / Math.Tan(angle)) / screenHeightinMM;


				float avgX = (firstPoint.x + secondPoint.x) / 2.0f;
				float avgY = (firstPoint.y + secondPoint.y) / 2.0f;


				//should  calaculate based on distance

				pX = (float)(movementScaling * Math.Sin(radiansPerPixel * (avgX - 512)) * pZ);

				relativeVerticalAngle = (avgY - 384) * radiansPerPixel;//relative angle to camera axis


				pY = -.5f + (float)(movementScaling * Math.Sin(relativeVerticalAngle + cameraVerticaleAngle) * pZ);
			}
			//Debug.WriteLine(pZ);
			vjoy.SetAxis((int)pX, id, HID_USAGES.HID_USAGE_X);
			vjoy.SetAxis((int)pY, id, HID_USAGES.HID_USAGE_Y);
			vjoy.SetAxis((int)pZ, id, HID_USAGES.HID_USAGE_Z);
			

		}

        public void setMPSCalibration()
        {
            calFlag = true;
        }


    }
}
