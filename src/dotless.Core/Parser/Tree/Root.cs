namespace dotless.Core.Parser.Tree
{
    using System;
    using System.Text.RegularExpressions;
    using dotless.Core.Utils;
    using Exceptions;
    using Infrastructure;
    using Infrastructure.Nodes;

    public class Root : Ruleset
    {
        public Func<ParsingException, ParserException> Error { get; set; }

        public Root(NodeList rules, Func<ParsingException, ParserException> error) 
            : this(rules, error, null)
        {
        }

        protected Root(NodeList rules, Func<ParsingException, ParserException> error, Ruleset master) 
            : base(new NodeList<Selector>(), rules, master)
        {
            Error = error;
        }

        public override void AppendCSS(Env env)
        {
            try
            {
                base.AppendCSS(env);

                if (env.Compress)
                    env.Output.Reset(Regex.Replace(env.Output.ToString(), @"(\s)+", " "));
            }
            catch (ParsingException e)
            {
                throw Error(e);
            }
        }

        public override Node Evaluate(Env env)
        {
            env = env ?? new Env();

            NodeHelper.ExpandNodes<Import>(env, this.Rules);

            Root clone = new Root(new NodeList(Rules), Error, OriginalRuleset).ReducedFrom<Root>(this);
            clone.EvaluateRules(env);

            return clone;
        }
    }
}