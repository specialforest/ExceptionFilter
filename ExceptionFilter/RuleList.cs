﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ExceptionFilter
{
    public class RuleList : List<Rule>
    {
        private const string _xmlRootElement = "Rules";
        private const string _xmlArrayItem = "Rule";
        private const string _xmlArrayItemType = "type";
        private const string _xmlVersion = "version";
        private const int _version = 1;

        public void Deserialize(XmlReader reader, List<RuleFactory> factories)
        {
            Clear();
            if (reader.IsStartElement(_xmlRootElement) && reader.IsEmptyElement)
            {
                return;
            }

            reader.ReadStartElement(_xmlRootElement);
            List<Rule> rules = new List<Rule>();
            while (reader.IsStartElement(_xmlArrayItem))
            {
                string ruleTypeID = reader.GetAttribute(_xmlArrayItemType);
                reader.ReadStartElement(_xmlArrayItem);
                Rule rule = DeserializeRule(reader, factories, ruleTypeID);
                rules.Add(rule);
                reader.ReadEndElement();
            }

            reader.ReadEndElement();
            AddRange(rules);
        }

        public void Serialize(XmlWriter writer)
        {
            writer.WriteStartElement(_xmlRootElement);
            writer.WriteAttributeString(_xmlVersion, _version.ToString());
            foreach (Rule rule in this)
            {
                writer.WriteStartElement(_xmlArrayItem);
                writer.WriteAttributeString(_xmlArrayItemType, rule.RuleTypeID);
                new XmlSerializer(rule.Data.GetType()).Serialize(writer, rule.Data);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private Rule DeserializeRule(System.Xml.XmlReader reader, List<RuleFactory> factories, string ruleType)
        {
            foreach (RuleFactory factory in factories)
            {
                if (factory.CanDeserialize(ruleType))
                {
                    return factory.Deserialize(reader);
                }
            }

            throw new SerializationException(String.Format("Unknown rule type: '{0}'", ruleType));
        }
    }
}
