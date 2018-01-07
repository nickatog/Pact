using System.Collections.Generic;

namespace Pact
{
    public class BlockContext
    {
        public IDictionary<string, string> Attributes { get; private set; }
        public BlockContext ParentBlock { get; private set; }

        public BlockContext(
            IDictionary<string, string> attributes,
            BlockContext parentBlock)
        {
            Attributes = attributes;
            ParentBlock = parentBlock;
        }
    }
}
