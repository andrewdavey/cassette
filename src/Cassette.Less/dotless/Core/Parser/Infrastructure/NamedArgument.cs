namespace dotless.Core.Parser.Infrastructure
{
    using Tree;

    class NamedArgument
    {
        public string Name { get; set; }
        public Expression Value { get; set; }
    }
}