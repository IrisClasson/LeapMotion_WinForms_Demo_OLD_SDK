﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Leap;

namespace WindowsFormsApplication1
{
    class WinFormsListener : Listener
    {
        public event Action<Controller> Frame;

        public override void OnInit(Controller controller)
        {
        }

        public override void OnConnect(Controller controller)
        {
            controller.EnableGesture(Gesture.GestureType.TYPECIRCLE);
            controller.EnableGesture(Gesture.GestureType.TYPEKEYTAP);
            controller.EnableGesture(Gesture.GestureType.TYPESCREENTAP);
            controller.EnableGesture(Gesture.GestureType.TYPESWIPE);
        }

        public override void OnDisconnect(Controller controller)
        {
        }

        public override void OnExit(Controller controller)
        {
        }

        public override void OnFrame(Controller controller)
        {
             if (Frame != null) Frame(controller);
        }
    }
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());       
        }
    }
}
