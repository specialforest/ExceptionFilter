using System;
using System.Collections.Generic;
using System.Text;

namespace ExceptionFilter
{
    /// <summary>
    /// Debugger action on exception
    /// </summary>
    public enum ExceptionAction { Default, Ignore, Break, Continue };

    /// <summary>
    /// Exception handler interface
    /// </summary>
    public abstract class ExceptionHandler
    {
        public abstract ExceptionAction? Handle(EnvDTE.StackFrame frame);
    }
}
