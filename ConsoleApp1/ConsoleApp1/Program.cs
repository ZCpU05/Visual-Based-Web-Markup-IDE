using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace AutoClicker
{
    class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        static void Main(string[] args)
        {
            bool isClicking = false;
            int clickDelay = 200;
            int toggleKey = 0x76; // F7 key
            int exitKey = 0x11;   // Control (Ctrl) key

            Console.WriteLine("C# Auto Clicker started.");
            Console.WriteLine("Press F7 to toggle clicking on/off.");
            Console.WriteLine("Press CONTROL to exit the program entirely.");

            while (true)
            {
                // Check if CONTROL is pressed to kill the application
                if ((GetAsyncKeyState(exitKey) & 0x8000) != 0)
                {
                    Console.WriteLine("Control pressed. Exiting program...");
                    break;
                }

                // Check if F7 is pressed to toggle
                if ((GetAsyncKeyState(toggleKey) & 0x8000) != 0)
                {
                    isClicking = !isClicking;
                    string status = isClicking ? "ACTIVATED" : "DEACTIVATED";
                    Console.WriteLine($"Auto Clicker {status}");

                    Thread.Sleep(300);
                }

                if (isClicking)
                {
                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                }

                Thread.Sleep(clickDelay);
            }
        }
    }
}
