﻿using System;
using Gwen.Controls;

namespace Gwen.Input
{
    public class KeyData
    {
        public bool[] KeyState;
        public double[] NextRepeat;
        public Base Target;
        public bool LeftMouseDown;
        public bool RightMouseDown;

        public KeyData()
        {
            KeyState = new bool[(int)Key.Count];
            NextRepeat = new double[(int)Key.Count];
            // everything is initialized to 0 by default
        }
    }
}
