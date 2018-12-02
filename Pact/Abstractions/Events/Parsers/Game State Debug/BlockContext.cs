using System.Collections.Generic;

namespace Pact
{
    public class BlockContext
    {
        public BlockContext(
            IDictionary<string, string> attributes,
            BlockContext parentBlock)
        {
            Attributes = attributes;
            ParentBlock = parentBlock;
        }

        public IDictionary<string, string> Attributes { get; }
        public BlockContext ParentBlock { get; }
    }
}
