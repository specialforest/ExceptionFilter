using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using EnvDTE90a;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace ExceptionFilter
{
    [Description("By File Filter")]
    [RuleType("ByFileFilter")]
    public class ByFileRule : Rule
    {
        private ExpressionRuleData _data;

        public ByFileRule()
        {
            _data = new ExpressionRuleData();
        }

        public ByFileRule(RuleData data)
        {
            _data = (ExpressionRuleData)data;
        }

        public override RuleData Data
        {
            get { return _data; }
        }

        public override ExceptionHandler CreateHandler()
        {
            return new ByFileExceptionHandler(_data);
        }
    }

    public class ByFileExceptionHandler : BasicExceptionHandler
    {
        public ByFileExceptionHandler(ExpressionRuleData data)
            : base(data)
        {
        }

        protected override String GetExpression(EnvDTE.StackFrame frame)
        {
            try
            {
                EnvDTE90a.StackFrame2 frameV2 = (EnvDTE90a.StackFrame2)frame;
                return frameV2.FileName;
            }
            catch (InvalidCastException)
            {
                return null;
            }
            catch (COMException)
            {
                return null;
            }
        }
    }
}
