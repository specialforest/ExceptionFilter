namespace ExceptionFilter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    using System.Xml.Serialization;

    public class ModuleRuleData : RuleData
    {
        [DefaultValue("")]
        [XmlElementAttribute("Module")]
        public string Module { get; set; }
    }

    [Description("Module Filter")]
    [RuleType("ModuleFilter")]
    public class ByModuleRule : Rule
    {
        private ModuleRuleData data;

        public ByModuleRule()
        {
            this.data = new ModuleRuleData();
        }

        public ByModuleRule(RuleData data)
        {
            this.data = (ModuleRuleData)data;
        }

        public override RuleData Data
        {
            get { return this.data; }
        }

        public override ExceptionHandler CreateHandler()
        {
            return new ByModuleExceptionHandler(data);
        }
    }

    public class ByModuleExceptionHandler : ExceptionHandler
    {
        private ModuleRuleData data;

        public ByModuleExceptionHandler(ModuleRuleData data)
        {
            this.data = data;
        }

        public override ExceptionAction? Handle(EnvDTE.StackFrame frame)
        {
            if (frame.Module == this.data.Module)
            {
                return this.data.Action;
            }

            return null;
        }
    }
}
