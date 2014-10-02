using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace Win10TaskbarCleaner
{

    public class WorkScheduler
    {
        private bool _registeredForSessionChange = false;

        private bool _runWhenInactive;
        private System.Timers.Timer _timer;

        public delegate void ScheduledWorkCallback(object data);

        ~WorkScheduler()
        {
            if (_registeredForSessionChange)
            {
                Microsoft.Win32.SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
            }
        }

        private struct ScheduleCallbackInformation
        {
            public ScheduledWorkCallback callback;
            public object state;
        }

        private ScheduleCallbackInformation _callbackInfo;
        
        /// <summary>
        /// Schedule work (the callback) to be triggered at interval.
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <param name="runWhenInactive">Should this work run while the user is inactive?</param>
        public void Schedule(int interval, ScheduledWorkCallback callback, object state = null, bool runWhenInactive = false)
        {
            _runWhenInactive = runWhenInactive;

            if (!runWhenInactive)
            {
                _registeredForSessionChange = true;
                Microsoft.Win32.SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            }

            _callbackInfo = new ScheduleCallbackInformation { callback = callback, state = state };

            _timer = new System.Timers.Timer(interval);
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = true;
            _timer.Start();

            // trigger an immediate run
            _timer_Elapsed(null, null);
        }

        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Triggering Timer");
            _callbackInfo.callback(_callbackInfo.state);
        }

        void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            if (e.Reason == Microsoft.Win32.SessionSwitchReason.ConsoleDisconnect ||
                e.Reason == Microsoft.Win32.SessionSwitchReason.RemoteDisconnect ||
                e.Reason == Microsoft.Win32.SessionSwitchReason.SessionLogoff ||
                e.Reason == Microsoft.Win32.SessionSwitchReason.SessionLock)
            {
                System.Diagnostics.Debug.WriteLine("Timer Paused (session switch)");

                _timer.Stop();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Timer Resumed (session switch)");

                _timer.Start();
            }
        }
    }
}
