using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

#nullable enable

namespace YourVeryOwnRingtone
{
    public sealed class VSListener : IDebugEventCallback2
    {
        private SoundManager _manager;

        private CommandEvents? _commandEvents;
        private Commands2? _commands;

        private readonly Dictionary<Guid, string> _events = new Dictionary<Guid, string>
        {
            { typeof(IDebugProgramCreateEvent2).GUID, "start" },
            { typeof(IDebugSessionDestroyEvent2).GUID, "stop" },
            { typeof(IDebugBreakpointEvent2).GUID, "breakpoint" },
            { typeof(IDebugExceptionEvent2).GUID, "exception" },
            { typeof(IDebugStepCompleteEvent2).GUID, "step" }
        };

        public VSListener(SoundManager manager)
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

            if (await serviceProvider.GetServiceAsync(typeof(DTE)) is DTE2 dte)
            {
                _commandEvents = dte.Events.get_CommandEvents(null, 0);
                _commandEvents.AfterExecute += OnAfterExecute;

                _commands = dte.Commands as Commands2;
            }
        }

        private void ProcessSoundEvent(string name)
        {
            _manager.PlaySound(name);
        }

        public int Event(IDebugEngine2 pEngine, IDebugProcess2 pProcess, IDebugProgram2 pProgram, IDebugThread2 pThread, IDebugEvent2 pEvent, ref Guid riidEvent, uint dwAttrib)
        {
            try
            {
                if (_events.TryGetValue(riidEvent, out string name))
                {
                    ProcessSoundEvent(name);
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

        private void OnAfterExecute(string guid, int id, object _, object __)
        {
            _ = ProcessCommandAsync(guid, id);
        }

        private async Task ProcessCommandAsync(string guid, int id)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            string name = GetCommandName(guid, id);
            switch (name)
            {
                case "Build.BuildSolution":
                case "Build.BuildOnlyProject":
                case "Build.Compile":
                    ProcessSoundEvent("build");
                    break;

                case "Edit.Find":
                    ProcessSoundEvent("find");
                    break;

                case "Edit.Undo":
                    ProcessSoundEvent("undo");
                    break;

                case "Debug.StepOver":
                    ProcessSoundEvent("stepover");
                    break;

                case "Debug.StepInto":
                    ProcessSoundEvent("stepinto");
                    break;

                case "Debug.StepOut":
                    ProcessSoundEvent("stepout");
                    break;

                case "Debug.ApplyCodeChanges":
                    ProcessSoundEvent("apply");
                    break;

                case "Debug.Restart":
                    ProcessSoundEvent("restart");
                    break;
            }
        }

        private string GetCommandName(string guid, int id)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string result = string.Empty;
            if (guid is null)
            {
                return result;
            }

            try
            {
                result = _commands?.Item(guid, id).Name ?? string.Empty;
            }
            catch { }

            return result;
        }
    }
}
