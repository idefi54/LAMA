using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace LAMA.Services
{
    //public enum Key
    //{
    //    LCtrl, RCtrl, LShift, RShift,
    //    A, B, C, D, E, F, G, H, I, J, K, L,
    //    M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
    //    Space,
    //    F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
    //    AN1, AN2, AN3, AN4, AN5, AN6, AN7, AN8, AN9, AN0 // Alpha-Numerical
    //}

    public delegate void KeyDown(string key);
    public delegate void KeyUp(string key);

    public interface IKeyboardService
    {
        bool IsKeyPressed(string key);

        event KeyDown OnKeyDown;
        event KeyUp OnKeyUp;
    }
}
