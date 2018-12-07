using System;
using System.Runtime.InteropServices;
using System.Windows;
using static ScreenWake.Interop;
using Microsoft.Win32;

namespace ScreenWake
{
    class PowerScheme
    {
        internal const uint ERROR_SUCCESS = 0;
        internal const uint ERROR_INSUFFICIENT_BUFFER = 122;
        internal const uint ERROR_MORE_DATA = 234;

        internal static Guid DISPLAY_SUBGROUP_GUID = 
            new Guid("7516b95f-f776-4464-8c53-06167f40cc99");
        internal static Guid DISPLAY_TIMEOUT_SETTING_GUID = 
            new Guid("3c0bc021-c8a8-4e07-a973-6b14cbcb2b7e");

        private readonly int[] _startUpTimeouts = new int[2];

        private Guid _activeSchemeGuid = new Guid();

        public Binding UiBinding;

        public PowerScheme()
        {
            UiBinding = new Binding(SetActiveTimeout);
            GetActiveScheme();
            _startUpTimeouts[0] = GetACTimeout();
            _startUpTimeouts[1] = GetDCTimeout();
            SystemEvents.PowerModeChanged += RefreshUi;
        }

        #region Active power scheme
        public void GetActiveScheme()
        {
            // Get GUID pointer
            var guidPtr = IntPtr.Zero;
            if (PowerGetActiveScheme(
                IntPtr.Zero, out guidPtr) != ERROR_SUCCESS)
            {
                throw new InvalidOperationException(
                    "Unknown - Failed retrieving power plan.");
            }
            else
            {
                // Get GUID string
                _activeSchemeGuid = (Guid)
                    Marshal.PtrToStructure(
                        guidPtr, typeof(Guid));
                Marshal.FreeHGlobal(guidPtr);
            }
        }

        public bool SetActiveScheme()
        {
            if (_activeSchemeGuid == Guid.Empty)
            {
                throw new ArgumentException("Active scheme GUID is invalid.");
            }

            return PowerSetActiveScheme(
                IntPtr.Zero, ref _activeSchemeGuid)
                == ERROR_SUCCESS;
        }
        #endregion

        #region Power source
        public bool IsPowerLineConnected
        {
            get
            {
                SystemPowerStatus status = new SystemPowerStatus();
                GetSystemPowerStatus(ref status);
                return (PowerLineStatus)status.ACLineStatus
                    == PowerLineStatus.Online;
            }
        }
        #endregion

        #region Display timeout
        public int GetActiveTimeout()
        {
            if (_activeSchemeGuid == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Active scheme GUID is invalid.");
            }
            
            if (IsPowerLineConnected) return GetACTimeout();
            else return GetDCTimeout();
        }

        public void SetActiveTimeout(int timeout)
        {
            if (_activeSchemeGuid == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Active scheme GUID is invalid.");
            }
            
            if (IsPowerLineConnected) SetACTimeout(timeout);
            else SetDCTimeout(timeout);
        }

        public int GetACTimeout()
        {
            uint displayTimeout = 0;
            PowerReadACValueIndex(
                IntPtr.Zero, ref _activeSchemeGuid,
                ref DISPLAY_SUBGROUP_GUID, ref DISPLAY_TIMEOUT_SETTING_GUID,
                ref displayTimeout);
            return (int) displayTimeout;
        }

        public int GetDCTimeout()
        {
            uint displayTimeout = 0;
            PowerReadDCValueIndex(
                    IntPtr.Zero, ref _activeSchemeGuid,
                    ref DISPLAY_SUBGROUP_GUID, ref DISPLAY_TIMEOUT_SETTING_GUID,
                    ref displayTimeout);
            return (int)displayTimeout;
        }

        public void SetACTimeout(int timeout)
        {
            PowerWriteACValueIndex(
                    IntPtr.Zero, ref _activeSchemeGuid,
                    ref DISPLAY_SUBGROUP_GUID, ref DISPLAY_TIMEOUT_SETTING_GUID,
                    (uint)timeout);
            SetActiveScheme();
        }

        public void SetDCTimeout(int timeout)
        {
            PowerWriteDCValueIndex(
                    IntPtr.Zero, ref _activeSchemeGuid,
                    ref DISPLAY_SUBGROUP_GUID, ref DISPLAY_TIMEOUT_SETTING_GUID,
                    (uint)timeout);
            SetActiveScheme();
        }
        #endregion

        public void RefreshUi(object sender = null, 
            PowerModeChangedEventArgs e = null)
        {
            UiBinding.PowerLineState = IsPowerLineConnected ? "AC Connected" : "Battery Power";
            UiBinding.DisplayTimeout = GetActiveTimeout();
        }

        public void SaveSettings()
        {
            if (UiBinding.Persistance == false)
            {
                // Not persisting = revert to settings at startup
                SetACTimeout(_startUpTimeouts[0]);
                SetDCTimeout(_startUpTimeouts[1]);
            }
        }
    }
}
