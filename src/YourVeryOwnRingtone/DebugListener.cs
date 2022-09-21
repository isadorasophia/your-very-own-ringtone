using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

#nullable enable

namespace YourVeryOwnRingtone
{
    public sealed class DebugListener : IDebugEventCallback2
    {
        private SoundManager _manager;

        private readonly Dictionary<Guid, string> _events = new Dictionary<Guid, string>
        {
            { typeof(IDebugBreakpointEvent2).GUID, "breakpoint" },
            { typeof(IDebugExceptionEvent2).GUID, "exception" },
            { typeof(IDebugProcessDestroyEvent2).GUID, "stop" },
            { typeof(IDebugStepCompleteEvent2).GUID, "step" }
        };

        public DebugListener(SoundManager manager)
        {
            _manager = manager;
        }

        public async Task InitializeAsync(IAsyncServiceProvider serviceProvider)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // Register to debug events!
            if (await serviceProvider.GetServiceAsync(typeof(SVsShellDebugger)) is IVsDebugger debugService)
            {
                debugService.AdviseDebugEventCallback(this);
            }
        }

        public int Event(IDebugEngine2 pEngine, IDebugProcess2 pProcess, IDebugProgram2 pProgram, IDebugThread2 pThread, IDebugEvent2 pEvent, ref Guid riidEvent, uint dwAttrib)
        {
            try
            {
                if (_events.TryGetValue(riidEvent, out string name))
                {
                    ProcessDebugEvent(name);
                }
            }
            finally
            {
                // https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.shell.interop.ivsdebugger.advisedebugeventcallback?redirectedfrom=MSDN&view=visualstudiosdk-2022#Microsoft_VisualStudio_Shell_Interop_IVsDebugger_AdviseDebugEventCallback_System_Object_
                // It is strongly advised that if a package chooses to implement IDebugEventCallback in managed code, that ReleaseComObject be invoked on the various interfaces passed to Event.
                if (pEngine != null) Marshal.ReleaseComObject(pEngine);
                if (pProcess != null) Marshal.ReleaseComObject(pProcess);
                if (pProgram != null) Marshal.ReleaseComObject(pProgram);
                if (pThread != null) Marshal.ReleaseComObject(pThread);
                if (pEvent != null) Marshal.ReleaseComObject(pEvent);
            }

            return 0;
        }

        private void ProcessDebugEvent(string name)
        {
            _manager.PlaySound(name);
        }
    }
}
