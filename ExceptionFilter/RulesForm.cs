using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ExceptionFilter
{
    /// <summary>
    /// Rules editing form
    /// </summary>
    public partial class RulesForm : Form
    {
        private BindingList<Rule> _rulesBindings;
        private List<RuleFactory> _ruleFactories;
        private int _lastSelectedItem;

        public RulesForm(RuleList rules, List<RuleFactory> ruleFactories)
        {
            InitializeComponent();

            _rulesBindings = new BindingList<Rule>(rules);
            lbRules.DataSource = _rulesBindings;
            lbRules.DisplayMember = "Name";
            _lastSelectedItem = -1;

            _ruleFactories = ruleFactories;
            cbRuleType.DataSource = _ruleFactories;
            cbRuleType.DisplayMember = "Description";
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            RuleFactory factory = (RuleFactory)cbRuleType.SelectedItem;
            if (factory == null)
            {
                return;
            }

            Rule rule = factory.Create();
            rule.Data.Name = "New Rule";
            rule.Data.Action = ExceptionAction.Continue;
            _rulesBindings.Add(rule);

            lbRules.SelectedIndex = _rulesBindings.Count - 1;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Rule selectedRule = (Rule)lbRules.SelectedItem;
            if (lbRules.SelectedItem != null)
            {
                pgRuleProps.SelectedObject = selectedRule.Data;
                tbRuleDesc.Text = selectedRule.Description;
            }
            else
            {
                pgRuleProps.SelectedObject = null;
                tbRuleDesc.Clear();
            }

            int newSelectedItem = lbRules.SelectedIndex;
            if (_lastSelectedItem >= 0 && _lastSelectedItem != newSelectedItem)
            {
                _rulesBindings.ResetItem(_lastSelectedItem);
            }

            _lastSelectedItem = newSelectedItem;
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            int index = lbRules.SelectedIndex;
            if (index < 0)
            {
                return;
            }

            _rulesBindings.RemoveAt(index);
            if (_rulesBindings.Count > 0)
            {
                lbRules.SelectedIndex = (index > 0) ? (index - 1) : index;
            }
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            int index = lbRules.SelectedIndex;
            if (index > 0)
            {
                Rule previousItem = _rulesBindings[index - 1];
                _rulesBindings.RemoveAt(index - 1);
                _rulesBindings.Insert(index, previousItem);
            }
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            int index = lbRules.SelectedIndex;
            if (index >= 0 && index < _rulesBindings.Count - 1)
            {
                Rule nextItem = _rulesBindings[index + 1];
                _rulesBindings.RemoveAt(index + 1);
                _rulesBindings.Insert(index, nextItem);
            }
        }
    }
}
