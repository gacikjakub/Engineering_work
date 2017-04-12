using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SlimDX.DirectInput;

namespace ServoMonitoring_with_Control
{
     delegate void onChange();   // necessary ??

    public class JoystickController
    {
        DirectInput Input;
        SlimDX.DirectInput.Joystick stick;
        Joystick[] Sticks;
        JoystickState state;
        Joystick choosenStick;
        int xValue;
        int yValue;
        int zValue;
        bool[] buttons;
        Thread StateRefresh;
        bool RefreshingStatus;
        public delegate void onChange();
        private onChange OnChange;


        public void JoystickListener(onChange oCh)
        {
            OnChange = oCh;
            
        }

        public JoystickController()
        {
            Input = new DirectInput();
            xValue = 0;
            yValue = 0;
            zValue = 0;
            Sticks = LoadSticks();
            if (Sticks.Length > 0) choosenStick = Sticks[0];
            state = new JoystickState();
            StateRefresh = new Thread(new ThreadStart(this.RefreshingThread));
            StartRefreshing();
        }


        public Joystick[] LoadSticks()
        {
            List<SlimDX.DirectInput.Joystick> sticks = new List<SlimDX.DirectInput.Joystick>();
            foreach (DeviceInstance device in Input.GetDevices(DeviceClass.GameController,DeviceEnumerationFlags.AttachedOnly))
            {
                try
                {
                    stick = new SlimDX.DirectInput.Joystick(Input, device.InstanceGuid);
                    stick.Acquire();

                    foreach(DeviceObjectInstance deviceObject in stick.GetObjects())
                    {
                        if ((deviceObject.ObjectType & ObjectDeviceType.Axis) !=0)
                        {
                            stick.GetObjectPropertiesById((int)deviceObject.ObjectType).SetRange(-100, 100);
                        }
                    }
                    sticks.Add(stick);

                } catch(DirectInputException)
                {

                }
            }
            return sticks.ToArray();
        }

        public int GetX()
        {
            return xValue;
        }

        public int GetY()
        {
            return yValue;
        }

        public int GetZ()
        {
            return zValue;
        }

        public bool[] GetButtons()
        {
            return buttons;
        }

        private void RefreshingThread()
        {
            if (Sticks.Length > 0)
            {
                bool[] oldButtons;      // necessary ??
                state = choosenStick.GetCurrentState();
                buttons = state.GetButtons();
                oldButtons = buttons;
                while (true)
                {
                    while (RefreshingStatus)
                    {
                        state = choosenStick.GetCurrentState();
                        oldButtons = buttons;
                        xValue = state.X;
                        yValue = state.Y;
                        zValue = state.Z;
                        buttons = state.GetButtons();

                        if (Math.Abs(xValue) > 10 || Math.Abs(yValue) > 10 || Math.Abs(zValue) > 10)   //|| !(oldButtons.Equals(buttons))
                        {
                            OnChange();
                        }
                    }
                }
            }
        }

        public void StartRefreshing()
        {
            if (!StateRefresh.IsAlive) StateRefresh.Start();
            this.RefreshingStatus = true;
        }

        public void StopRefreshing()
        {
            this.RefreshingStatus = false;
        }

    }
}
