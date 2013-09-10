using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Leap;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private static Controller _controller;
        private long _currentTime;
        private long _previousTime;
        private long _timeChange;
        private bool _cursourEnabled;
        private WinFormsListener _listener;
        private object sync = new object();

        public Form1()
        {
            InitializeComponent();
            _listener = new WinFormsListener();
            _controller = new Controller();
            _controller.AddListener(_listener);
            _listener.Frame += ListenerOnFrame;
            _cursourEnabled = false;
            FormClosed +=OnFormClosed;
        }

        private void OnFormClosed(object sender, FormClosedEventArgs formClosedEventArgs)
        {
            _controller.RemoveListener(_listener);
            _controller.Dispose();
        }

        private void ListenerOnFrame(Controller controller)
        {
            lock(sync)
            {
            var fingers = new FingerList();
            var frame = controller.Frame();
            _currentTime = frame.Timestamp;
            _timeChange = _currentTime - _previousTime;

                #region Read frama data
                if (!frame.Hands.Empty)
                {
                    Hand hand = frame.Hands[0];
                    fingers = hand.Fingers;
                    if (_timeChange > 1000)
                    {
                        if (!fingers.Empty)
                        {
                            if (textBox1.InvokeRequired)
                            {
                                BeginInvoke(new Action<object>(Update), new object[] {fingers});
                            }
                        }
                    }
                }
                #endregion
                #region Gesture

                GestureList gestures = frame.Gestures();
                foreach (Gesture gesture in gestures)
                {
                    switch (gesture.Type)
                    {
                        case Gesture.GestureType.TYPECIRCLE:
                            PrintData("Circle gesture");
                            break;
                        case Gesture.GestureType.TYPESWIPE:
                            PrintData("Swipe gesture");
                            break;
                        case Gesture.GestureType.TYPEKEYTAP:
                            PrintData("Tap gesture");
                            break;
                        case Gesture.GestureType.TYPESCREENTAP:
                            PrintData("Tap ScreenTapGesture");
                            break;
                        default:
                            PrintData("Unknown gesture type.");
                            break;
                    }
                }
                #endregion
                #region Mouse

                if (_cursourEnabled)
                {
                    var screen = controller.CalibratedScreens.ClosestScreenHit(fingers[0]);

                    if (fingers.Count == 2)
                    {
                        if (_timeChange > 4000)
                        {
                            var x = (int) screen.Intersect(fingers[0], true, 1.0F).x;
                            var y = (int) screen.Intersect(fingers[0], true, 1.0F).y;
                            MouseCursor.mouse_event(0x0002 | 0x0004, 0, x, y, 0);
                        }
                    }

                    if (screen != null && screen.IsValid)
                    {
                        if ((int) fingers[0].TipVelocity.Magnitude > 25)
                        {
                            var xScreenIntersect = screen.Intersect(fingers[0], true).x;
                            var yScreenIntersect = screen.Intersect(fingers[0], true).y;

                            if (xScreenIntersect.ToString() != "NaN")
                            {
                                var x = (int) (xScreenIntersect*screen.WidthPixels);
                                var y = (int) (screen.HeightPixels - (yScreenIntersect*screen.HeightPixels));
                                MouseCursor.MoveCursor(x, y);
                            }

                        }
                    }
                }

                #endregion
                _previousTime = _currentTime;
                frame.Dispose();
            }
        }

        void PrintData(string str)
        {
            if (textBox2.InvokeRequired)
            {
                BeginInvoke(new Action<string>(UpdateGestureBox), str);
            }
        }

        private void Update(object obj)
        {
            var fngr = (FingerList) obj;
            Debug.WriteLine(fngr.Count.ToString());
            var xRaw = (decimal)fngr[0].TipPosition.x;
            textBox1.Text = fngr.Count.ToString();
            textBox1.Location = new Point(
                CoordinateConverter.TranslateToLeft(xRaw, 1500),
                700 - (int)fngr[0].TipPosition.y);
        }

        private void UpdateGestureBox(string str)
        {
            Debug.WriteLine(str);
            textBox2.Text = str;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _cursourEnabled = true;
        }
    }

    class MouseCursor
    {
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        public static void MoveCursor(int x, int y)
        {
            SetCursorPos(x, y);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
    }
}
