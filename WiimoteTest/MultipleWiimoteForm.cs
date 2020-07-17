using System;
using System.Collections.Generic;
using System.Windows.Forms;
using vJoyInterfaceWrap;
using WiimoteLib;

namespace WiimoteTest
{
    public partial class MultipleWiimoteForm : Form
    {
        // map a wiimote to a specific state user control dealie
        Dictionary<Guid, WiimoteInfo> mWiimoteMap = new Dictionary<Guid, WiimoteInfo>();
        WiimoteCollection mWC;

        public MultipleWiimoteForm()
        {
            InitializeComponent();
        }

        private void MultipleWiimoteForm_Load(object sender, EventArgs e)
        {
            // find all wiimotes connected to the system
            mWC = new WiimoteCollection();
            int index = 1;

            try
            {
                mWC.FindAllWiimotes();
            }
            catch (WiimoteNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Wiimote not found error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (WiimoteException ex)
            {
                MessageBox.Show(ex.Message, "Wiimote error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unknown error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            foreach (Wiimote wm in mWC)
            {
                // create a new tab
                TabPage tp = new TabPage("Wiimote " + index);
                tabWiimotes.TabPages.Add(tp);

                // create a new user control
                WiimoteInfo wi = new WiimoteInfo(wm);
                tp.Controls.Add(wi);

                // setup the map from this wiimote's ID to that control
                mWiimoteMap[wm.ID] = wi;

                // connect it and set it up as always
                wm.WiimoteChanged += wm_WiimoteChanged;
                wm.WiimoteExtensionChanged += wm_WiimoteExtensionChanged;

                wm.Connect();
                if (wm.WiimoteState.ExtensionType != ExtensionType.BalanceBoard)
                    wm.SetReportType(InputReport.IRExtensionAccel, IRSensitivity.Maximum, true);

                wm.SetLEDs(index++);
                wm.InitializeMotionPlus();

            }
        }

        void wm_WiimoteChanged(object sender, WiimoteChangedEventArgs e)
        {
            WiimoteInfo wi = mWiimoteMap[((Wiimote)sender).ID];
            wi.UpdateState(e);
        }

        void wm_WiimoteExtensionChanged(object sender, WiimoteExtensionChangedEventArgs e)
        {
            // find the control for this Wiimote
            WiimoteInfo wi = mWiimoteMap[((Wiimote)sender).ID];
            wi.UpdateExtension(e);

            if (e.Inserted)
                ((Wiimote)sender).SetReportType(InputReport.IRExtensionAccel, true);
            else
                ((Wiimote)sender).SetReportType(InputReport.IRAccel, true);
        }

        private void MultipleWiimoteForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            foreach (Wiimote wm in mWC)
                wm.Disconnect();
        }

        private void initializeVJoy()
        {
            // Declaring one joystick (Device id 1) and a position structure. 
            vJoy joystick;
            uint id = 1;

            // Create one joystick object and a position structure.
            joystick = new vJoy();

            if (id <= 0 || id > 16)
            {
                Console.WriteLine("Illegal device ID {0}\nExit!", id);
                return;
            }

            // Get the driver attributes (Vendor ID, Product ID, Version Number)
            if (!joystick.vJoyEnabled())
            {
                Console.WriteLine("vJoy driver not enabled: Failed Getting vJoy attributes.\n");
                return;
            }
            else
                Console.WriteLine("Vendor: {0}\nProduct :{1}\nVersion Number:{2}\n", joystick.GetvJoyManufacturerString(), joystick.GetvJoyProductString(), joystick.GetvJoySerialNumberString());

            // Get the state of the requested device
            VjdStat status = joystick.GetVJDStatus(id);
            switch (status)
            {
                case VjdStat.VJD_STAT_OWN:
                    Console.WriteLine("vJoy Device {0} is already owned by this feeder\n", id);
                    break;
                case VjdStat.VJD_STAT_FREE:
                    Console.WriteLine("vJoy Device {0} is free\n", id);
                    break;
                case VjdStat.VJD_STAT_BUSY:
                    Console.WriteLine("vJoy Device {0} is already owned by another feeder\nCannot continue\n", id);
                    return;
                case VjdStat.VJD_STAT_MISS:
                    Console.WriteLine("vJoy Device {0} is not installed or disabled\nCannot continue\n", id);
                    return;
                default:
                    Console.WriteLine("vJoy Device {0} general error\nCannot continue\n", id);
                    return;
            };

            // Check which axes are supported
            bool AxisX = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_X);
            bool AxisY = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Y);
            bool AxisZ = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Z);
            bool AxisRX = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RX);
            bool AxisRZ = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RZ);
            // Get the number of buttons and POV Hat switchessupported by this vJoy device
            int nButtons = joystick.GetVJDButtonNumber(id);
            int ContPovNumber = joystick.GetVJDContPovNumber(id);
            int DiscPovNumber = joystick.GetVJDDiscPovNumber(id);

            // Print results
            Console.WriteLine("\nvJoy Device {0} capabilities:\n", id);
            Console.WriteLine("Numner of buttons\t\t{0}\n", nButtons);
            Console.WriteLine("Numner of Continuous POVs\t{0}\n", ContPovNumber);
            Console.WriteLine("Numner of Descrete POVs\t\t{0}\n", DiscPovNumber);
            Console.WriteLine("Axis X\t\t{0}\n", AxisX ? "Yes" : "No");
            Console.WriteLine("Axis Y\t\t{0}\n", AxisX ? "Yes" : "No");
            Console.WriteLine("Axis Z\t\t{0}\n", AxisX ? "Yes" : "No");
            Console.WriteLine("Axis Rx\t\t{0}\n", AxisRX ? "Yes" : "No");
            Console.WriteLine("Axis Rz\t\t{0}\n", AxisRZ ? "Yes" : "No");

            // Test if DLL matches the driver
            UInt32 DllVer = 0, DrvVer = 0;
            bool match = joystick.DriverMatch(ref DllVer, ref DrvVer);
            if (match)
                Console.WriteLine("Version of Driver Matches DLL Version ({0:X})\n", DllVer);
            else
                Console.WriteLine("Version of Driver ({0:X}) does NOT match DLL Version ({1:X})\n", DrvVer, DllVer);


            // Acquire the target
            if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!joystick.AcquireVJD(id))))
            {
                Console.WriteLine("Failed to acquire vJoy device number {0}.\n", id);
                return;
            }
            else
                Console.WriteLine("Acquired: vJoy device number {0}.\n", id);

            Console.WriteLine("\npress enter to stat feeding");
            Console.ReadKey(true);

            int X, Y, Z, ZR, XR;
            uint count = 0;
            long maxval = 0;

            X = 20;
            Y = 30;
            Z = 40;
            XR = 60;
            ZR = 80;

            joystick.GetVJDAxisMax(id, HID_USAGES.HID_USAGE_X, ref maxval);

            bool res;
            // Reset this device to default values
            joystick.ResetVJD(id);

            Console.WriteLine("Main Method");

            Wiimote wiiDevice = new Wiimote();
            Console.WriteLine("2");

            try
            {
                Console.WriteLine("3");
                Wiimote test = Connect(mWC[0]);
                if (test == null)
                    Console.WriteLine("ERROR CONENCTION!");
                else
                {
                    Console.WriteLine("conenction fine");
                    wiiDevice = test;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Not found!");
            }


            try
            {
                if (wiiDevice.WiimoteState.ExtensionType != ExtensionType.BalanceBoard)
                {
                    Console.WriteLine("NOT BB!");
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("EXCEPTION! " + e.Message);
                Console.Read();
            }
        }


            private Wiimote Connect(Wiimote wiiDeviceInc)
            {
                try
                {
                    // Find all connected Wii devices.

                    var deviceCollection = new WiimoteCollection();
                    deviceCollection.FindAllWiimotes();

                    Console.WriteLine(deviceCollection.Count);

                    Wiimote wiiDevice = deviceCollection[0];

                    // Device type can only be found after connection, so prompt for multiple devices.

                    // Setup update handlers. LATER

                    wiiDevice.WiimoteChanged += wiiDevice_WiimoteChanged;
                    wiiDevice.WiimoteExtensionChanged += wiiDevice_WiimoteExtensionChanged;

                    // Connect and send a request to verify it worked.

                    wiiDevice.Connect();
                    wiiDevice.SetReportType(InputReport.IRAccel, false); // FALSE = DEVICE ONLY SENDS UPDATES WHEN VALUES CHANGE!
                    wiiDevice.SetLEDs(true, false, false, false);

                    return wiiDevice;

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }

            }

            static private void wiiDevice_WiimoteChanged(object sender, WiimoteChangedEventArgs e)
            {
                // Called every time there is a sensor update, values available using e.WiimoteState.
                // Use this for tracking and filtering rapid accelerometer and gyroscope sensor data.
                // The balance board values are basic, so can be accessed directly only when needed.
            }

            static private void wiiDevice_WiimoteExtensionChanged(object sender, WiimoteExtensionChangedEventArgs e)
            {
                // This is not needed for balance boards.
            }
        
    }
}
