namespace dotless.Core.Parser.Infrastructure
{
    using Nodes;
    using Tree;

    interface IOperable
    {
        Node Operate(Operation op, Node other);
        Color ToColor();
    }
}