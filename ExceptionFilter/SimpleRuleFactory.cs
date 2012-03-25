using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace ExceptionFilter
{
    /// <summary>
    /// Simple rule factory implementation
    /// </summary>
    public class SimpleRuleFactory : RuleFactory
    {
        private Type _ruleType;
        private Type _ruleDataType;

        public SimpleRuleFactory(Type ruleType, Type ruleDataType)
        {
            if (!ruleType.IsSubclassOf(typeof(Rule)))
            {
                throw new ArgumentException(String.Format("Class '{0}' is not a sub-class of type '{1}'", ruleType.Name, typeof(Rule).Name));
            }

            if (Attribute.GetCustomAttributes(ruleType, typeof(DescriptionAttribute)).Length == 0)
            {
                throw new ArgumentException(String.Format("Attribute '{0}' is not defined by class '{1}'", typeof(DescriptionAttribute).Name, ruleType.Name));
            }

            if (Attribute.GetCustomAttributes(ruleType, typeof(RuleType)).Length == 0)
            {
                throw new ArgumentException(String.Format("Attribute '{0}' is not defined by class '{1}'", typeof(RuleType).Name, ruleType.Name));
            }

            _ruleType = ruleType;
            _ruleDataType = ruleDataType;
        }

        public override string Description
        {
            get { return ((DescriptionAttribute)Attribute.GetCustomAttribute(_ruleType, typeof(DescriptionAttribute))).Description; }
        }

        public override Rule Create()
        {
            return (Rule)Activator.CreateInstance(_ruleType);
        }

        public override bool CanDeserialize(string ruleTypeID)
        {
            string myRuleTypeID = ((RuleType)Attribute.GetCustomAttribute(_ruleType, typeof(RuleType))).RuleTypeID;
            return myRuleTypeID == ruleTypeID;
        }

        public override Rule Deserialize(XmlReader reader)
        {
            RuleData data = (RuleData)new XmlSerializer(_ruleDataType).Deserialize(reader);
            return (Rule)Activator.CreateInstance(_ruleType, data);
        }
    }
}
