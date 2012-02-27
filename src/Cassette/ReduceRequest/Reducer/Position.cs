namespace Cassette.ReduceRequest.Reducer
{
    public struct Position
    {
        public PositionMode PositionMode { get; set; }
        public int Offset { get; set; }
        public Direction Direction { get; set; }
    }
}
