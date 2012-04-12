using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ExceptionFilter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            RuleList rules = new RuleList();
            List<RuleFactory> ruleFactories = new List<RuleFactory>();
            ruleFactories.Add(new SimpleRuleFactory(typeof(ByFileRule), typeof(ExpressionRuleData)));
            ruleFactories.Add(new SimpleRuleFactory(typeof(ByFuncRule), typeof(ExpressionRuleData)));
            ruleFactories.Add(new SimpleRuleFactory(typeof(ByModuleRule), typeof(ModuleRuleData)));
            Application.Run(new RulesForm(rules, ruleFactories));
        }
    }
}
