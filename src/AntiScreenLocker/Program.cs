using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AntiScreenLocker
{
    internal class Program
    {
        [DllImport("user32.dll", EntryPoint = "mouse_event")]
        private static extern int MouseEvent(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll", EntryPoint = "SetCurrentPos")]
        private static extern bool SetCurrentPos(int x, int y);

        [DllImport("user32.dll", EntryPoint = "GetCurrentPos")]
        private static extern bool GetCurrentPos(POINT point);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        private static TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(1);

        // ReSharper disable once InconsistentNaming
        private static int MOUSEEVENTF_MOVE { get; } = 0x0001;

        [STAThread]
        private static void Main(string[] args)
        {
            var mutex = new Mutex(true, "AntiScreenLocker", out var createdNew);
            if (createdNew)
            {
                mutex.ReleaseMutex();
                if (args.Length > 0 && int.TryParse(args[0], out var timeout))
                {
                    Timeout = TimeSpan.FromMilliseconds(timeout);
                    Console.WriteLine($"Interval:{Timeout}");
                }

                new System.Threading.Timer(state =>
                {
                    var delta = new Random(DateTime.Now.Millisecond).Next(0, 10) >= 5 ? 1 : -1;

                    MouseEvent(MOUSEEVENTF_MOVE, delta, delta, 0, 0);
                }, null, TimeSpan.Zero, Timeout);
                Application.Run();
            }
        }
    }
}