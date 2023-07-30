using System;
using System.Collections.Generic;
using System.Data;

public class RuleProcessor
{
    private readonly List<IRule> _rules;
    public RuleProcessor(List<IRule> rules)
    {
        _rules = rules;
    }
    // Constructor to inject the IEmailSender implementation
    public void ProcessRules()
    {
        foreach (var rule in _rules)
        {
            rule.Execute();
        }
    }
}
