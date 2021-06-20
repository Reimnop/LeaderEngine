using System.Collections.Generic;

namespace LeaderEngine
{
    public class FntNode
    {
        public readonly string Name;
        public readonly string Value;

        public List<FntNode> this[string name] => children[name];

        private Dictionary<string, List<FntNode>> children = new Dictionary<string, List<FntNode>>();

        public FntNode(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public void AddChildren(FntNode node)
        {
            if (children.ContainsKey(node.Name))
            {
                children[node.Name].Add(node);
                return;
            }
            children.Add(node.Name, new List<FntNode>() { node });
        }
    }
}
