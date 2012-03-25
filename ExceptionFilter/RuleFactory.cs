using System.Xml;

namespace ExceptionFilter
{
    public abstract class RuleFactory
    {
        public abstract string Description { get; }

        public abstract Rule Create();
        public abstract bool CanDeserialize(string ruleTypeID);
        public abstract Rule Deserialize(XmlReader reader);
    }
}
