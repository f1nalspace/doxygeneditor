using System.Collections.Generic;
using TSP.DoxygenEditor.Lexers;

namespace TSP.DoxygenEditor.Parsers
{
    public abstract class BaseSymbolResolver<TEntity, TToken> where TEntity : BaseEntity where TToken : BaseToken
    {
        protected abstract void ResolveNode(BaseNode<TEntity> node);
        protected abstract void ResolveToken(TToken token);

        private void ResolveChildrenNodes(BaseNode<TEntity> rootNode)
        {
            foreach (IBaseNode child in rootNode.Children)
            {
                if (typeof(BaseNode<TEntity>).Equals(child.GetType()))
                {
                    BaseNode<TEntity> cppChild = (BaseNode<TEntity>)child;
                    ResolveNode(cppChild);
                    ResolveChildrenNodes(cppChild);
                }
            }
        }

        public void ResolveNodes(BaseNode<TEntity> rootNode)
        {
            ResolveChildrenNodes(rootNode);
        }

        public void ResolveTokens(IEnumerable<TToken> tokens)
        {
            foreach (var token in tokens)
                ResolveToken(token);
        }
    }
}
