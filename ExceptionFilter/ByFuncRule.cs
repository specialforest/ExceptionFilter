using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace ExceptionFilter
{
    [Description("Routine Filter")]
    [RuleType("FuncFilter")]
    public class ByFuncRule : Rule
    {
        private ExpressionRuleData _data;

        public ByFuncRule()
        {
            _data = new ExpressionRuleData();
        }

        public ByFuncRule(RuleData data)
        {
            _data = (ExpressionRuleData)data;
        }

        public override RuleData Data
        {
            get { return _data; }
        }

        public override ExceptionHandler CreateHandler()
        {
            return new ByFuncExceptionHandler(_data);
        }
    }

    public class ByFuncExceptionHandler : BasicExceptionHandler
    {
        public ByFuncExceptionHandler(ExpressionRuleData data)
            : base(data)
        {
        }

        protected override String GetExpression(EnvDTE.StackFrame frame)
        {
            try
            {
                return frame.FunctionName;
            }
            catch (COMException)
            {
                return null;
            }
        }
    }
}
