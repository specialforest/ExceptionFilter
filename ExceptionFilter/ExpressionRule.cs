using System.ComponentModel;
using System.Xml.Serialization;
using System;

namespace ExceptionFilter
{
    public class ExpressionRuleData : RuleData
    {
        [DefaultValue("")]
        [XmlElementAttribute("Expression")]
        public string Expression { get; set; }

        [DefaultValue(false)]
        [XmlElementAttribute("UseRegex")]
        public bool UseRegex { get; set; }
    }
}
