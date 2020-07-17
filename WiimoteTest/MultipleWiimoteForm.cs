using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        vJoy con1, con2;
        Wiimote dev1, dev2;
        WiimoteInfo[] wInfo = new WiimoteInfo[2];

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

            int count = 0;
            foreach (Wiimote wm in mWC)
            {
                // create a new tab
                TabPage tp = new TabPage("Wiimote " + index);
                tabWiimotes.TabPages.Add(tp);

                // create a new user control
                WiimoteInfo wi = new WiimoteInfo(wm);
                tp.Controls.Add(wi);

                wInfo[count++] = wi;

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

            startVJoy();
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

        private void startVJoy()
        {
            //Allow for second controller

            dev1 = mWC[0];
            //dev2 = mWC[1];

            con1 = initializeVJoy(1, dev1);
            //con2 = initializeVJoy(2, dev2);
            
            wInfo[0].setTarget(con1,1);
            //wInfo[0].setTarget(con2,2);
        }

        private vJoy initializeVJoy(uint id, Wiimote wiiDevice)
        {
            vJoy joystick = new vJoy();
            // Get the driver attributes (Vendor ID, Product ID, Version Number)
            if (!joystick.vJoyEnabled())
            {
                Console.WriteLine("vJoy driver not enabled: Failed Getting vJoy attributes.\n");
                return null;
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
                    return null;
                case VjdStat.VJD_STAT_MISS:
                    Console.WriteLine("vJoy Device {0} is not installed or disabled\nCannot continue\n", id);
                    return null;
                default:
                    Console.WriteLine("vJoy Device {0} general error\nCannot continue\n", id);
                    return null;
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
            Console.WriteLine("Axis Y\t\t{0}\n", AxisY ? "Yes" : "No");
            Console.WriteLine("Axis Z\t\t{0}\n", AxisZ ? "Yes" : "No");
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
                return null;
            }
            else
                Console.WriteLine("Acquired: vJoy device number {0}.\n", id);

            long maxval = 0;

            joystick.GetVJDAxisMax(id, HID_USAGES.HID_USAGE_X, ref maxval);
            joystick.ResetVJD(id);
            wiiDevice = mWC[(int)(id-1)];
            Debug.WriteLine("Device type " + wiiDevice.WiimoteState.ExtensionType);
            Debug.WriteLine("Device id " + id + " loaded");

            return joystick;
        }
        
    }
}
