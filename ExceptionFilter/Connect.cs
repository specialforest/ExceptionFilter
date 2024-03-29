﻿using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using EnvDTE90;
using EnvDTE90a;
using Microsoft.VisualStudio.CommandBars;
using System.IO;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Windows.Forms;

namespace ExceptionFilter
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public class Connect : IDTExtensibility2, IDTCommandTarget
	{
        private const string SettingsFile = "ExceptionFilter.Settings";

        private DTE2 _applicationObject;
        private AddIn _addInInstance;
        private OutputWindowPane _output;
        private DebuggerEvents _debuggerEvents;
        private SolutionEvents _solutionEvents;

        private RuleList _rules;
        private List<RuleFactory> _ruleFactories;
        private List<ExceptionHandler> _handlers;

		/// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
		public Connect()
		{
            _rules = new RuleList();
            _ruleFactories = new List<RuleFactory>();
            _ruleFactories.Add(new SimpleRuleFactory(typeof(ByFileRule), typeof(ExpressionRuleData)));
            _ruleFactories.Add(new SimpleRuleFactory(typeof(ByFuncRule), typeof(ExpressionRuleData)));
            _ruleFactories.Add(new SimpleRuleFactory(typeof(ByModuleRule), typeof(ModuleRuleData)));
            _handlers = new List<ExceptionHandler>();
		}

		/// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
			_applicationObject = (DTE2)application;
			_addInInstance = (AddIn)addInInst;

            if (connectMode == ext_ConnectMode.ext_cm_AfterStartup ||
                connectMode == ext_ConnectMode.ext_cm_Startup ||
                connectMode == ext_ConnectMode.ext_cm_UISetup)
            {
                Events events = _applicationObject.Events;
                _solutionEvents = events.SolutionEvents;
                _solutionEvents.Opened += new _dispSolutionEvents_OpenedEventHandler(OnSolutionOpened);
                _solutionEvents.BeforeClosing += new _dispSolutionEvents_BeforeClosingEventHandler(OnSolutionClosing);
                _debuggerEvents = events.DebuggerEvents;

                SetupUI();
            }

            if (_applicationObject.Solution.IsOpen)
            {
                OnSolutionOpened();
            }
		}

		/// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
            DetachFromDebugger();
		}

		/// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />		
		public void OnAddInsUpdate(ref Array custom)
		{
		}

		/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref Array custom)
		{
		}

		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref Array custom)
		{
		}
		
		/// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
		/// <param term='commandName'>The name of the command to determine state for.</param>
		/// <param term='neededText'>Text that is needed for the command.</param>
		/// <param term='status'>The state of the command in the user interface.</param>
		/// <param term='commandText'>Text requested by the neededText parameter.</param>
		/// <seealso class='Exec' />
		public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
		{
			if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
			{
                if (commandName == "ExceptionFilter.Connect.ExceptionFilterOptions")
				{
					status = vsCommandStatus.vsCommandStatusSupported;
                    if (_applicationObject.Solution.IsOpen)
                    {
                        status |= vsCommandStatus.vsCommandStatusEnabled;
                    }

					return;
				}
			}
		}

		/// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
		/// <param term='commandName'>The name of the command to execute.</param>
		/// <param term='executeOption'>Describes how the command should be run.</param>
		/// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
		/// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
		/// <param term='handled'>Informs the caller if the command was handled or not.</param>
		/// <seealso class='Exec' />
		public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
		{
			handled = false;
			if(executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
			{
                if (commandName == "ExceptionFilter.Connect.ExceptionFilterOptions")
				{
                    RulesForm form = new RulesForm(_rules, _ruleFactories);
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        BuildHandlers();
                    }
                    else
                    {
                        ///TODO: handle cancel
                    }

					handled = true;
					return;
				}
			}
		}

        void SetupUI()
        {
            OutputWindow outputWindow = (OutputWindow)_applicationObject.Windows.Item(Constants.vsWindowKindOutput).Object;
            _output = outputWindow.OutputWindowPanes.Add("Exception Filter");

            object[] contextGUIDS = new object[] { };
            Commands2 commands = (Commands2)_applicationObject.Commands;
            string toolsMenuName = "Tools";

            // Place the command on the tools menu.
            // Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
            Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar = ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars)["MenuBar"];

            // Find the Tools command bar on the MenuBar command bar:
            CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
            CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;

            // This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
            // just make sure you also update the QueryStatus/Exec method to include the new command names.
            try
            {
                // Add a command to the Commands collection:
                Command command = commands.AddNamedCommand2(_addInInstance, "ExceptionFilterOptions", "Exception Filter...", "Shows Exception Filter options dialog",
                    true, Type.Missing, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported, (int)vsCommandStyle.vsCommandStyleText, vsCommandControlType.vsCommandControlTypeButton);

                // Add a control for the command to the tools menu:
                if ((command != null) && (toolsPopup != null))
                {
                    command.AddControl(toolsPopup.CommandBar, 1);
                }
            }
            catch (System.ArgumentException e)
            {
                // If we are here, then the exception is probably because a command with that name
                // already exists. If so there is no need to recreate the command and we can 
                // safely ignore the exception.
                Trace.Write(e);
            }
        }

        void OnSolutionClosing()
        {
            DetachFromDebugger();
            SaveSettings();
        }

        void OnSolutionOpened()
        {
            AttachToDebugger();
            LoadSettings();
            BuildHandlers();
        }

        void AttachToDebugger()
        {
            if (_debuggerEvents != null)
            {
                _debuggerEvents.OnExceptionThrown += new EnvDTE._dispDebuggerEvents_OnExceptionThrownEventHandler(this.OnExceptionThrown);
                _debuggerEvents.OnExceptionNotHandled += new EnvDTE._dispDebuggerEvents_OnExceptionNotHandledEventHandler(this.OnNotHandledException);
            }
        }

        void DetachFromDebugger()
        {
            if (_debuggerEvents != null)
            {
                _debuggerEvents.OnExceptionThrown -= new EnvDTE._dispDebuggerEvents_OnExceptionThrownEventHandler(this.OnExceptionThrown);
                _debuggerEvents.OnExceptionNotHandled -= new EnvDTE._dispDebuggerEvents_OnExceptionNotHandledEventHandler(this.OnNotHandledException);
            }
        }

        string GetSettingsPath()
        {
            string settingsDir = Path.GetDirectoryName(_applicationObject.Solution.FullName);
            return Path.Combine(settingsDir, SettingsFile);
        }

        void LoadSettings()
        {
            string settingsPath = GetSettingsPath();
            if (File.Exists(settingsPath))
            {
                XmlTextReader stream = new XmlTextReader(settingsPath);
                _rules.Deserialize(stream, _ruleFactories);
            }
        }

        void SaveSettings()
        {
            string settingsPath = GetSettingsPath();
            if (_rules.Count == 0)
            {
                if (File.Exists(settingsPath))
                {
                    File.Delete(settingsPath);
                }

                return;
            }

            XmlTextWriter stream = new XmlTextWriter(settingsPath, Encoding.UTF8);
            stream.Formatting = Formatting.Indented;
            _rules.Serialize(stream);
            stream.Close();
        }

        void BuildHandlers()
        {
            _handlers.Clear();
            foreach (Rule rule in _rules)
            {
                _handlers.Add(rule.CreateHandler());
            }
        }

        dbgExceptionAction ConvertExceptionAction(ExceptionAction action)
        {
            switch (action)
            {
                case ExceptionAction.Default:
                    return dbgExceptionAction.dbgExceptionActionDefault;

                case ExceptionAction.Ignore:
                    return dbgExceptionAction.dbgExceptionActionContinue;

                case ExceptionAction.Break:
                    return dbgExceptionAction.dbgExceptionActionBreak;

                case ExceptionAction.Continue:
                    return dbgExceptionAction.dbgExceptionActionContinue;

                default:
                    throw new ArgumentException();
            }
        }

        void OnExceptionThrown(
            string ExceptionType,
            string Name,
            int Code,
            string Description,
            ref dbgExceptionAction Action)
        {
            EnvDTE.StackFrame topFrame = _applicationObject.Debugger.CurrentStackFrame;
            if (topFrame.Language == "C++")
            {
                var stackFrames = _applicationObject.Debugger.CurrentThread.StackFrames.GetEnumerator();
                while (stackFrames.MoveNext())
                {
                    if (((EnvDTE.StackFrame)stackFrames.Current).FunctionName == "_CxxThrowException")
                    {
                        break;
                    }
                }

                if (stackFrames.MoveNext())
                {
                    topFrame = (EnvDTE.StackFrame)stackFrames.Current;
                }
            }

            if (topFrame != null)
            {
                foreach (ExceptionHandler handler in _handlers)
                {
                    ExceptionAction? action = handler.Handle(topFrame);
                    if (action.HasValue)
                    {
                        Action = ConvertExceptionAction(action.Value);
                        _output.OutputString("\nFiltered: " + Name);
                        return;
                    }
                }

                PrintFrame(topFrame);
            }

            Action = dbgExceptionAction.dbgExceptionActionDefault;
        }

        void OnNotHandledException(
            string ExceptionType,
            string Name,
            int Code,
            string Description,
            ref dbgExceptionAction ExceptionAction)
        {
            ExceptionAction = dbgExceptionAction.dbgExceptionActionDefault;
        }

        void PrintFrame(EnvDTE.StackFrame frame)
        {
            _output.OutputString("\nStack frame:");
            _output.OutputString("\n\tModule: " + frame.Module);
            try
            {
                StackFrame2 frameV2 = (StackFrame2)frame;
                _output.OutputString("\n\tFilename: " + frameV2.FileName);
                _output.OutputString("\n\tLineNumber: " + frameV2.LineNumber);
            }
            catch (InvalidCastException)
            {
            }
            catch (COMException)
            {
            }

            _output.OutputString("\n\tFunctionName: " + frame.FunctionName);
            _output.OutputString("\n\tArguments:");
            foreach (Expression exp in frame.Arguments)
            {
                _output.OutputString("\n\t" + exp.Name + " = " + exp.Value);
            }

            _output.OutputString("\n\tLocals:");
            foreach (Expression exp in frame.Locals)
            {
                _output.OutputString("\n\t" + exp.Name + " = " + exp.Value);
            }
        }
	}
}
