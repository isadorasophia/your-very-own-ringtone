using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

#nullable enable

namespace YourVeryOwnRingtone
{
    public sealed class VSListener : IDebugEventCallback2, IVsUpdateSolutionEvents2
    {
        private SoundManager _manager;

        private CommandEvents? _commandEvents;
        private Commands2? _commands;

        private readonly Dictionary<Guid, string> _events = new Dictionary<Guid, string>
        {
            { typeof(IDebugSessionCreateEvent2).GUID, "start" },
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
                _commandEvents.BeforeExecute += OnBeforeExecute;

                _commands = dte.Commands as Commands2;
            }

            if (await serviceProvider.GetServiceAsync(typeof(SVsSolutionBuildManager)) is IVsSolutionBuildManager2 buildManager)
            {
                buildManager.AdviseUpdateSolutionEvents(this, out uint _);
            }
        }

        private void ProcessSoundEvent(string name)
        {
            _ = _manager.PlaySoundAsync(name);
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

        public int UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            ProcessSoundEvent("build.start");

            return 0;
        }

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            if (fSucceeded == 1)
            {
                ProcessSoundEvent("build.onsuccess");
            }
            else
            {
                ProcessSoundEvent("build.onfail");
            }

            return 0;
        }

        #region Unused IVsUpdateSolutionEvents2

        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate) => 1;

        public int UpdateSolution_Cancel() => 1;

        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy) => 1;

        public int UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel) => 1;

        public int UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel) => 1;

        #endregion

        private void OnBeforeExecute(string guid, int id, object _, object __, ref bool ___)
        {
            _ = ProcessCommandAsync(guid, id);
        }

        private async Task ProcessCommandAsync(string guid, int id)
        {
            // Ensures that the rest of the method runs on a background thread. Simply using ConfigureAwait(false) won't necessarily ensure that's the case,
            // see https://devblogs.microsoft.com/dotnet/configureawait-faq/#does-configureawaitfalse-guarantee-the-callback-wont-be-run-in-the-original-context
            await TaskScheduler.Default;

            string name = GetCommandName(guid, id);
            switch (name)
            {
                case "File.SaveSelectedItems":
                    ProcessSoundEvent("save");
                    break;

                case "Edit.Find":
                    ProcessSoundEvent("find");
                    break;

                case "Edit.Undo":
                    ProcessSoundEvent("undo");
                    break;

                case "Debug.Start":
                    ProcessSoundEvent("continue");
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

        private readonly Dictionary<(Guid, int), string> _hardcodedCommands = new Dictionary<(Guid, int), string>
        {
            { (new("5EFC7975-14BC-11CF-9B2B-00AA00573819"), 295), "Debug.Start" },
            { (new("5EFC7975-14BC-11CF-9B2B-00AA00573819"), 296), "Debug.Restart" },
            { (new("5EFC7975-14BC-11CF-9B2B-00AA00573819"), 250), "Debug.StepOut" },
            { (new("5EFC7975-14BC-11CF-9B2B-00AA00573819"), 248), "Debug.StepInto" },
            { (new("5EFC7975-14BC-11CF-9B2B-00AA00573819"), 249), "Debug.StepOver" },
            { (new("5EFC7975-14BC-11CF-9B2B-00AA00573819"), 43), "Edit.Undo" },
            { (new("5EFC7975-14BC-11CF-9B2B-00AA00573819"), 97), "Edit.Find" },
            { (new("5EFC7975-14BC-11CF-9B2B-00AA00573819"), 331), "File.SaveSelectedItems" },
            { (new("C9DD4A59-47FB-11D2-83E7-00C04F9902C1"), 61476), "Debug.ApplyCodeChanges" }
        };

        private string GetCommandName(string guid, int id)
        {
            if (guid is null)
            {
                return string.Empty;
            }

            if (_hardcodedCommands.TryGetValue((new(guid), id), out string result))
            {
                return result;
            }

            return string.Empty;
        }

        /// <summary>
        /// Used for finding out new commands. We should use _hardcodedCommands for faster results without relying on the main thread.
        /// </summary>
        private async Task<string> GetCommandNameAsync(string guid, int id)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (guid is null)
            {
                return string.Empty;
            }

            try
            {
                return _commands?.Item(guid, id).Name ?? string.Empty;
            }
            catch { }

            return string.Empty;
        }

    }
}
