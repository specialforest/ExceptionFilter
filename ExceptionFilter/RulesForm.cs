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
        private RuleList rules;
        private List<RuleFactory> ruleFactories;
        private int lastSelectedItemIndex;

        public RulesForm(RuleList rules, List<RuleFactory> ruleFactories)
        {
            InitializeComponent();

            this.rules = rules;
            this.lastSelectedItemIndex = -1;
            PopulateList();

            this.ruleFactories = ruleFactories;
            cbRuleType.DataSource = this.ruleFactories;
            cbRuleType.DisplayMember = "Description";
            
        }

        private void UpdateRules()
        {
            this.rules.Clear();
            foreach (ListViewItem item in this.listView1.Items)
            {
                this.rules.Add((Rule)item.Tag);
            }
        }

        private void PopulateList()
        {
            this.listView1.BeginUpdate();
            this.listView1.Items.Clear();
            foreach (Rule rule in this.rules)
            {
                this.listView1.Items.Add(CreateItem(rule));
            }

            this.listView1.EndUpdate();
        }

        private ListViewItem CreateItem(Rule rule)
        {
            ListViewItem item = new ListViewItem();
            item.SubItems.Add(new ListViewItem.ListViewSubItem());
            item.SubItems.Add(new ListViewItem.ListViewSubItem());
            item.Tag = rule;
            UpdateItem(item);
            return item;
        }

        private void UpdateItem(ListViewItem item)
        {
            Rule rule = (Rule)item.Tag;
            item.SubItems[0].Text = rule.Name;
            item.SubItems[1].Text = rule.Description;
        }

        private void MoveItem(int from, int to)
        {
            ListViewItem item = this.listView1.Items[from];
            this.listView1.BeginUpdate();
            this.listView1.Items.RemoveAt(from);
            this.listView1.Items.Insert(to, item);
            this.listView1.EndUpdate();
        }

        private void OnNewRule(object sender, EventArgs e)
        {
            RuleFactory factory = (RuleFactory)cbRuleType.SelectedItem;
            if (factory == null)
            {
                return;
            }

            Rule rule = factory.Create();
            rule.Data.Name = "New Rule";
            rule.Data.Action = ExceptionAction.Continue;

            ListViewItem newItem = CreateItem(rule);
            this.listView1.Items.Insert(this.lastSelectedItemIndex + 1, newItem);
            newItem.Selected = true;
        }

        private void OnRuleSelected(object sender, EventArgs e)
        {
            int newSelectedItemIndex = this.listView1.SelectedIndices.Count > 0 ? this.listView1.SelectedIndices[0] : -1;
            if (this.lastSelectedItemIndex >= 0 && this.lastSelectedItemIndex != newSelectedItemIndex)
            {
                UpdateItem(this.listView1.Items[this.lastSelectedItemIndex]);
            }

            this.lastSelectedItemIndex = newSelectedItemIndex;
            if (newSelectedItemIndex >= 0)
            {
                Rule selectedRule = (Rule)this.listView1.SelectedItems[0].Tag;
                pgRuleProps.SelectedObject = selectedRule.Data;
                tbRuleDesc.Text = selectedRule.Description;
            }
            else
            {
                pgRuleProps.SelectedObject = null;
                tbRuleDesc.Clear();
            }
        }

        private void OnRuleRemove(object sender, EventArgs e)
        {
            if (this.listView1.SelectedIndices.Count > 0)
            {
                int selectedIndex = this.listView1.SelectedIndices[0];
                this.listView1.SelectedItems[0].Remove();
                if (this.listView1.Items.Count > 0)
                {
                    this.listView1.Items[selectedIndex > 0 ? selectedIndex - 1 : selectedIndex].Selected = true;
                }
            }
        }

        private void OnRuleMoveUp(object sender, EventArgs e)
        {
            if (this.listView1.SelectedIndices.Count == 0)
            {
                return;
            }

            int selectedIndex = this.listView1.SelectedIndices[0];
            if (selectedIndex > 0)
            {
                MoveItem(selectedIndex - 1, selectedIndex);
            }
        }

        private void OnRuleMoveDown(object sender, EventArgs e)
        {
            if (this.listView1.SelectedIndices.Count == 0)
            {
                return;
            }

            int selectedIndex = this.listView1.SelectedIndices[0];
            if (selectedIndex >= 0 && selectedIndex < this.listView1.Items.Count - 1)
            {
                MoveItem(selectedIndex + 1, selectedIndex);
            }
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
            {
                UpdateRules();
            }
        }
    }
}
