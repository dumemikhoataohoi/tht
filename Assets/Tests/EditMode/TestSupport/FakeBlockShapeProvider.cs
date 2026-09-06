using GemGrid.Configuration;

namespace GemGrid.Tests.EditMode
{
    public class FakeBlockShapeProvider : IBlockShapeProvider
    {
        private readonly BlockShapeLibrary _library;

        public FakeBlockShapeProvider(params BlockShapeDefinition[] shapes)
        {
            _library = new BlockShapeLibrary(shapes);
        }

        public BlockShapeLibrary GetLibrary() => _library;
    }
}
