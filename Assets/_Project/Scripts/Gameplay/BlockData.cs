using GemGrid.Configuration;

namespace GemGrid.Gameplay
{
    /// <summary>One spawned instance of a shape sitting in a tray slot.</summary>
    public readonly struct BlockData
    {
        public readonly string InstanceId;
        public readonly BlockShapeDefinition Shape;

        public BlockData(string instanceId, BlockShapeDefinition shape)
        {
            InstanceId = instanceId;
            Shape = shape;
        }

        public bool IsEmpty => Shape == null;

        public static BlockData Empty { get; } = new BlockData(null, null);
    }
}
