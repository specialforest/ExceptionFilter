using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ExceptionFilter
{
    /// <summary>
    /// Rule type unique identifier
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class RuleType : Attribute
    {
        private string _ruleTypeID;

        public string RuleTypeID
        {
            get { return _ruleTypeID; }
        }

        public RuleType(string ruleTypeID)
        {
            _ruleTypeID = ruleTypeID;
        }
    }

    /// <summary>
    /// Base class for rule data
    /// </summary>
    public class RuleData
    {
        [Category("General")]
        public string Name { get; set; }

        [Category("General")]
        [DefaultValue(ExceptionAction.Default)]
        public ExceptionAction Action { get; set; }
    }

    /// <summary>
    /// Base class for rules
    /// Every inherited class must specify Description and RuleType attributes
    /// </summary>
    public abstract class Rule
    {
        public string Description
        {
            get { return ((DescriptionAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(DescriptionAttribute))).Description; }
        }

        public string RuleTypeID
        {
            get { return ((RuleType)Attribute.GetCustomAttribute(this.GetType(), typeof(RuleType))).RuleTypeID; }
        }

        public string Name
        {
            get { return Data.Name; }
        }

        public abstract RuleData Data { get; }

        public abstract ExceptionHandler CreateHandler();
    }
}
