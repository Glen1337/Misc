using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using Autodesk.AutoCAD.Interop;
using System.Runtime.InteropServices;

namespace Misc
{
    namespace Projects
    {
        public enum Status { Good = 0, Bad = 1 }

        class Program
        {
            // From stackoverflow.com
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool SetWindowPos(
                IntPtr hWnd,
                IntPtr hWndInsertAfter,
                int x,
                int y,
                int cx,
                int cy,
                int uFlags);

            // From stackoverflow.com
            private const int HWND_TOPMOST = -1;
            private const int SWP_NOMOVE = 0x0002;
            private const int SWP_NOSIZE = 0x0001;

            // no version number so it will run with any version
            //const string strProgId = "AutoCAD.Application";

            public const String progID = "AutoCAD.Application.19";
            //public enum Status { Good = 0, Bad = 1 }
            public static Autodesk.AutoCAD.Interop.AcadApplication acadComApp = null;

            static void Main(string[] args)
            {
                System.Diagnostics.Stopwatch timer = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    Console.WriteLine("Attempting to get license and run AutoCAD..." + Environment.NewLine);

                    //System.Threading.Thread.Sleep(1000);
                    IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;

                    // From stackoverflow.com
                    SetWindowPos(hWnd,
                        new IntPtr(HWND_TOPMOST),
                        0, 0, 0, 0,
                        SWP_NOMOVE | SWP_NOSIZE);

                    Status status;
                    Console.Title = "Trying to get Acad...";
                    AcadProcChanger aproc = new AcadProcChanger();

                    do
                    {
                        GC.Collect();
                        GC.Collect();
                        aproc.preTryClean();

                        status = aproc.StartAcad();

                        if (status == Status.Good)
                        {
                            try
                            {
                                AcadApplication AA = Marshal.GetActiveObject(Program.progID) as AcadApplication;
                                AA.Visible = true;
                                timer.Stop();
                                //AA.ActiveDocument.SendCommand( "(print " + "\"It took " + Math.Round(timer.Elapsed.TotalMinutes, 1, MidpointRounding.AwayFromZero).ToString() + " minutes to open this\")(princ) ");
                                Console.WriteLine("Running acad instance detected after execution");
                            }
                            catch
                            {
                                System.Console.WriteLine("Error: Could not detect running acad instance after execution");
                            }

                            Console.Title = "Got Acad";
                            Console.WriteLine("Success: " + acadComApp.Name + "(" + acadComApp.Version + ") " + " [" + DateTime.Now.ToString("hh:mm:ss tt") + "]");
                            Program.acadComApp.Visible = true;

                            break;
                        }
                        else
                        {
                            Console.WriteLine("Acad license in use. Retrying...\t[" + DateTime.Now.ToString("hh:mm:ss tt") + "]" + Environment.NewLine);
                        }
                        // Check for lic every 1 second
                        System.Threading.Thread.Sleep(1000);
                    }
                    while (status == Status.Bad);
                    timer.Stop();

                    // from stackoverflow.com
                    SetWindowPos(hWnd, new IntPtr(HWND_TOPMOST), 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);

                    Console.WriteLine("It took " + Math.Round(timer.Elapsed.TotalMinutes, 1, MidpointRounding.AwayFromZero) + " minutes to open AutoCAD");
                    Console.WriteLine("Closing...");
                    System.Threading.Thread.Sleep(10000);

                    return;
                }
                catch (System.Exception se)
                {
                    Console.WriteLine("Error: Unhandled exception caught at the top: " + se.Message);
                }
            }
        }
        internal sealed class AcadProcChanger
        {
            public void preTryClean()
            {
                UInt16 openedInstances = 0;
                foreach (Process acadproc in Process.GetProcessesByName("acad"))
                {
                    try
                    {
                        acadproc.Kill();
                        openedInstances++;
                    }
                    catch { Console.WriteLine("Error: Could not close an already opened instance of acad"); }
                }

                Console.WriteLine(openedInstances + " currently running AutoCAD instances closed");
                Program.acadComApp = null;
                return;
            }

            public Status StartAcad()
            {
                try
                {
                    Program.acadComApp = Marshal.GetActiveObject(Program.progID) as AcadApplication;

                    if (Program.acadComApp != null) { Console.WriteLine("Attempting to start Acad even though an instance is already running"); return Status.Bad; }

                }
                catch
                {
                    //Good, No acad instance is running
                    try
                    {
                        Program.acadComApp = Activator.CreateInstance(Type.GetTypeFromProgID(Program.progID), true) as AcadApplication;
                    }
                    catch
                    {
                        try
                        {
                            Program.acadComApp.Quit();
                            Marshal.ReleaseComObject(Program.acadComApp);
                        }
                        catch
                        {
                            Console.WriteLine("Activator could not create acad instance");
                            return Status.Bad;
                        }
                    }
                }
                return Status.Good;
            }
        }
    }
}
