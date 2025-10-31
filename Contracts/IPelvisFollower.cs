using CarolCustomizer.Hooks.Watchdogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarolCustomizer.Contracts
{
    internal interface IPelvisFollower
    {
        void HandleNewPelvis(PelvisWatchdog watchdog);
    }
}
