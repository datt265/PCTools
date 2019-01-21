using System;
using System.Management;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.Win32;


namespace PCTools
{
    public partial class PCTools : Form
    {
        static string str;
        public PCTools()
        {
            InitializeComponent();
            this.MaximumSize = this.MinimumSize = this.Size;

        }

        [StructLayout(LayoutKind.Sequential)]
        private struct OSVERSIONINFOEX
        {
            public int dwOSVersionInfoSize;
            public int dwMajorVersion;
            public int dwMinorVersion;
            public int dwBuildNumber;
            public int dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szCSDVersion;
            public short wServicePackMajor;
            public short wServicePackMinor;
            public short wSuiteMask;
            public byte wProductType;
            public byte wReserved;
        }

        [DllImport("kernel32.dll")]
        private static extern bool GetVersionEx(ref OSVERSIONINFOEX osVersionInfo);

        public string DoGetHostAddresses(string hostname)
        {
            IPAddress[] ips;
            string strIP = "";
            ips = Dns.GetHostAddresses(hostname);

            foreach (IPAddress ip in ips)
            {
                strIP  = Convert.ToString(ip);
            }
            return strIP;
        }

        public string GetOSServicePack()
        {
            OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX();

            osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));

            if (!GetVersionEx(ref osVersionInfo))
            {
                return "";
            }
            else
            {
                return " " + osVersionInfo.szCSDVersion;
            }
        }


        public static string GetOSName()
        {
            OperatingSystem osInfo = Environment.OSVersion;
            string osName = "UNKNOWN";

            switch (osInfo.Platform)
            {
                case PlatformID.Win32Windows:
                    {
                        switch (osInfo.Version.Minor)
                        {
                            case 0:
                                {
                                    osName = "Windows 95";
                                    break;
                                }

                            case 10:
                                {
                                    if (osInfo.Version.Revision.ToString() == "2222A")
                                    {
                                        osName = "Windows 98 Second Edition";
                                    }
                                    else
                                    {
                                        osName = "Windows 98";
                                    }
                                    break;
                                }

                            case 90:
                                {
                                    osName = "Windows Me";
                                    break;
                                }
                        }
                        break;
                    }

                case PlatformID.Win32NT:
                    {
                        switch (osInfo.Version.Major)
                        {
                            case 3:
                                {
                                    osName = "Windows NT 3.51";
                                    break;
                                }

                            case 4:
                                {
                                    osName = "Windows NT 4.0";
                                    break;
                                }

                            case 5:
                                {
                                    if (osInfo.Version.Minor == 0)
                                    {
                                        osName = "Windows 2000";
                                    }
                                    else if (osInfo.Version.Minor == 1)
                                    {
                                        osName = "Windows XP";
                                    }
                                    else if (osInfo.Version.Minor == 2)
                                    {
                                        osName = "Windows Server 2003";
                                    }
                                    break;
                                }

                            case 6:
                                {
                                    if (osInfo.Version.Minor == 0)
                                    {
                                        osName = "Windows Vista";
                                        break;
                                    }
                                    else if (osInfo.Version.Minor == 1)
                                    {
                                        osName = "Windows 7 / 2008r2";
                                        break;
                                    }
                                    else if (osInfo.Version.Minor == 2)
                                    {
                                        osName = "Windows 8";
                                        break;
                                    }
                                    else if (osInfo.Version.Minor == 3)
                                    {
                                        osName = "Windows 8.1 / 2012r2";
                                        break;
                                    }
                                    break;
                                }

                            case 10:
                                if (osInfo.Version.Minor == 0)
                                {
                                    osName = "Windows 10";
                                    break;
                                }
                                break;
                        }
                        break;
                    }

                    case PlatformID.WinCE:
                    {

                    osName = "Windows CE";

                    }
                    break;
            }

            return osName;
        }


        public double getRam()
        {
            double Ram_MegaBytes = 0;

            ManagementObjectSearcher Search = new ManagementObjectSearcher("Select * From Win32_ComputerSystem");


            foreach (ManagementObject Mobject in Search.Get())
            {
                double Ram_Bytes = (Convert.ToDouble(Mobject["TotalPhysicalMemory"]));

                Ram_MegaBytes = Ram_Bytes / 1048576;
            }

            return Ram_MegaBytes;

        }



        private void PCTools_Load(object sender, EventArgs e)
        {
            string copyright = "\u00a9D. Attard";
            this.toolStripStatusLabel2.Text = copyright;
            this.toolStripStatusLabel3.Text = DateTime.Now.ToLongDateString();



            LabelLoggedUser.Text = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

            labelComputerName.Text = Dns.GetHostName();
            string hn = Dns.GetHostName();

            labelIPAddress.Text = DoGetHostAddresses(hn);


            labelOSVersion.Text = GetOSName() + " " + GetOSServicePack();
            labelIEVer.Text = (new WebBrowser()).Version.Major.ToString();

            labelRAM.Text = getRam().ToString("#")+ " " + "Mb";
            labelMACAddress.Text = (GetMACAddress());

            labelSerialNumber.Text = getSerialNumber();
        }

        protected string getSerialNumber()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BIOS");

            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    return wmi.GetPropertyValue("SerialNumber").ToString();

                }

                catch { }

            }

            return "Unknown";
        }



        protected string GetMACAddress()
        {

            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");

            ManagementObjectCollection moc = mc.GetInstances();

            string MACAddress = "";

            foreach (ManagementObject mo in moc)
            {

                if (mo["MacAddress"] != null)
                {

                    if ((bool)mo["IPEnabled"] == true)
                    {

                        MACAddress = mo["MacAddress"].ToString();

                    }

                }

            }
            return MACAddress;
        }


        private static void GetVersionFromRegistry()
        {

            // Opens the registry key for the .NET Framework entry.
            using (RegistryKey ndpKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "").
                OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
            {
                // As an alternative, if you know the computers you will query are running .NET Framework 4.5
                // or later, you can use:
                // using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                // RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
                foreach (string versionKeyName in ndpKey.GetSubKeyNames())
                {
                    if (versionKeyName.StartsWith("v"))
                    {

                        RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                        string name = (string)versionKey.GetValue("Version", "");
                        string sp = versionKey.GetValue("SP", "").ToString();
                        string install = versionKey.GetValue("Install", "").ToString();
                        if (install == "") //no install info, must be later.
                            //Console.WriteLine(versionKeyName + "  " + name);
                            str += (versionKeyName + "  " + name) + Environment.NewLine;
                        else
                        {
                            if (sp != "" && install == "1")
                            {
                               // Console.WriteLine(versionKeyName + "  " + name + "  SP" + sp);
                                str += (versionKeyName + "  " + name + "  SP" + sp) + Environment.NewLine;
                            }

                        }
                        if (name != "")
                        {
                            continue;
                        }
                        foreach (string subKeyName in versionKey.GetSubKeyNames())
                        {
                            RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                            name = (string)subKey.GetValue("Version", "");
                            if (name != "")
                                sp = subKey.GetValue("SP", "").ToString();
                            install = subKey.GetValue("Install", "").ToString();
                            if (install == "") //no install info, must be later.
                                //Console.WriteLine(versionKeyName + "  " + name);
                                str += (versionKeyName + "  " + name) + Environment.NewLine;
                            else
                            {
                                if (sp != "" && install == "1")
                                {
                                   // Console.WriteLine("  " + subKeyName + "  " + name + "  SP" + sp);
                                    str += ("  " + subKeyName + "  " + name + "  SP" + sp) + Environment.NewLine;
                                }
                                else if (install == "1")
                                {
                                   // Console.WriteLine("  " + subKeyName + "  " + name);
                                    str += ("  " + subKeyName + "  " + name) + Environment.NewLine;
                                }

                            }

                        }

                    }
                }
            }
            MessageBox.Show(str,".Net Versions Installed");


        }

        private void callnetversion_Click(object sender, EventArgs e)
        {
            str = "";
            GetVersionFromRegistry();
        }
    }
}
