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

		//Capture pos variables
		List<Point3F> accS = new List<Point3F>();
		List<Point3> rawV = new List<Point3>();
		WiimoteState wsTest;


		//Gabos variables
		float rx = 50;
		float ry = 0;
		float rz = 0;
		float vx = 0;
		float vy = 0;
		float vz = 0;
		bool first = true;
		float kx = 147;
		float ky = 149;
		float kz = 149;
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
			//Zmazat potom
			wsTest = ws;

			int RX;
			int RY;
			int RZ;
			if (first||calFlag)
            {
				if (cal == 0 && first)
                {
					kx = ws.MotionPlusState.RawValues.X;
				}
                if (cal != 10)
                {
					avrX += ws.MotionPlusState.RawValues.X;
					deltaX += Math.Abs(ws.MotionPlusState.RawValues.X - kx);
					cal++;
                }
                else
                {
					first = false;
					calFlag = false;
					cal = 0;
					kx = avrX / 10;
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
			if (ws.MotionPlusState.YawFast)
            {
				if (kx - ws.MotionPlusState.RawValues.X > deltaX)
                {
					rx += (kx- ws.MotionPlusState.RawValues.X)/750;
                }
				else if (kx-ws.MotionPlusState.RawValues.X<-deltaX)
                {
					rx += (kx - ws.MotionPlusState.RawValues.X)/750;
                }
            }
			rz = 2 * (kz - ws.AccelState.RawValues.Z);
			ry = 2 * (ky - ws.AccelState.RawValues.Y);
			//rx = 2 * (kx - ws.AccelState.RawValues.X);


			RX = (int)(32767 * rx / 100);
			RY = (int)(32767 * ry / 100); // Y je pseudo hotovy 
			RZ = (int)(32767 * rz / 100);

			//Debug.WriteLine(2*(ky - ws.AccelState.RawValues.Y) + " asdas "+rx);
			//Debug.WriteLine(ws.MotionPlusState.RawValues.Z + "Got MPS" + ws.AccelState.RawValues.Z);
			vjoy.SetAxis(RX, id, HID_USAGES.HID_USAGE_RX);
			vjoy.SetAxis(RY, id, HID_USAGES.HID_USAGE_RY);
			vjoy.SetAxis(RZ, id, HID_USAGES.HID_USAGE_RZ);

		}

        public void setIR(WiimoteState ws)
        {
			float dotDistanceInMM = 215.9f;//width of the wii sensor bar
			float screenHeightinMM = 8.5f * 25.4f; 
			float radiansPerPixel = 0.84098780112944162841859845373893f / 1016.0f; //45 degree field of view with a 1024x768 camera
			float movementScaling = 1.0f;
			float relativeVerticalAngle = 0; //current head position view angle
			float cameraVerticaleAngle = 0; //begins assuming the camera is point straight forward
			Point2D[] wiimotePointsNormalized = new Point2D[4];
			int[] wiimotePointIDMap = new int[4];
			float px;
			float py;
			float pz;



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
				wiimotePointsNormalized[0].x = 1.0f - ws.IRState.IRSensors[0].RawPosition.X / 760.0f;
				wiimotePointsNormalized[0].y = ws.IRState.IRSensors[0].RawPosition.Y / 760.0f;
				firstPoint.x = ws.IRState.IRSensors[0].RawPosition.X;
				firstPoint.y = ws.IRState.IRSensors[0].RawPosition.Y;
				numvisible = 1;
			}

			if (ws.IRState.IRSensors[1].Found)
			{
				wiimotePointsNormalized[1].x = 1.0f - ws.IRState.IRSensors[1].RawPosition.X / 760.0f;
				wiimotePointsNormalized[1].y = ws.IRState.IRSensors[1].RawPosition.Y / 760.0f;
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
				wiimotePointsNormalized[2].x = 1.0f - ws.IRState.IRSensors[2].RawPosition.X / 760.0f;
				wiimotePointsNormalized[2].y = ws.IRState.IRSensors[2].RawPosition.Y / 760.0f;
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
				wiimotePointsNormalized[3].x = 1.0f - ws.IRState.IRSensors[3].RawPosition.X / 760.0f;
				wiimotePointsNormalized[3].y = ws.IRState.IRSensors[3].RawPosition.Y / 760.0f;
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
				pZ = (float)((dotDistanceInMM / 2) / Math.Sin(angle)) ;
				//Debug.WriteLine(pZ);


				float avgX = (firstPoint.x + secondPoint.x) / 2.0f;
				float avgY = (firstPoint.y + secondPoint.y) / 2.0f;


				//should  calaculate based on distance

				pX = (float)(movementScaling * Math.Sin(radiansPerPixel * (avgX - 512)) * pZ);
				//Debug.WriteLine(pX);
				relativeVerticalAngle = (avgY - 384) * radiansPerPixel;//relative angle to camera axis


				pY = -.5f + (float)(movementScaling * Math.Sin(relativeVerticalAngle + cameraVerticaleAngle) * pZ);
			}
            else
            {
				pZ = 1500;
            }
			Debug.WriteLine(pZ);
			pz = 32767 * pZ / 3000;
			px = 32767 * (750 - pX) / 1500;
			py = 32767 * (750 - pY) / 1500;
			vjoy.SetAxis((int)px, id, HID_USAGES.HID_USAGE_X);
			vjoy.SetAxis((int)py, id, HID_USAGES.HID_USAGE_Y);
			vjoy.SetAxis((int)pz, id, HID_USAGES.HID_USAGE_Z);
			

		}

        public void setMPSCalibration()
        {
            
        }

        public void capturePos()
        {

			if (wsTest == null) return; //Pre istotu
			Debug.WriteLine("Position captured");

			accS.Add(wsTest.AccelState.Values);
			rawV.Add(wsTest.MotionPlusState.RawValues);

			if(accS.Count > 1)	//Porovnavame ak mame 2+ zaznamov
            {
				for(int i=0; i<accS.Count;i++)
                {
					Debug.WriteLine("Capture " + i);
					Debug.WriteLine("AccesState: " + accS[i].ToString());
					Debug.WriteLine("RawValues: " + rawV[i].ToString());
					Debug.WriteLine("--------------------------");
					
				}
            }
        }
    }
}
