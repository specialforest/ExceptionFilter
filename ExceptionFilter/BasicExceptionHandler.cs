using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ExceptionFilter
{
    public abstract class BasicExceptionHandler : ExceptionHandler
    {
        public BasicExceptionHandler(ExpressionRuleData data)
        {
            _action = data.Action;
            if (data.UseRegex)
            {
                _regexExpression = new Regex(data.Expression);
            }
            else
            {
                _simpleExpression = data.Expression;
            }
        }

        public override ExceptionAction? Handle(EnvDTE.StackFrame frame)
        {
            string testExpression = GetExpression(frame);
            if (testExpression == null)
            {
                return null;
            }

            if (_simpleExpression != null && _simpleExpression == testExpression)
            {
                return _action;
            }

            if (_regexExpression != null && _regexExpression.IsMatch(testExpression))
            {
                return _action;
            }

            return null;
        }

        protected abstract String GetExpression(EnvDTE.StackFrame frame);

        private ExceptionAction _action;
        private string _simpleExpression;
        private Regex _regexExpression;
    }
}
