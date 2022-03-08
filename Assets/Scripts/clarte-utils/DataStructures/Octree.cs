using System;
using System.Collections.Generic;
using UnityEngine;

namespace CLARTE.DataStructures
{
    public class Octree<T>
    {
        public class CellEvents
        {
            #region Events
            public event EventHandler<Cell> OnEnabled;
            public event EventHandler<Cell> OnDisabled;
            #endregion

            #region Public methods
            public void Enable(Cell cell)
            {
                TriggerEvent(OnEnabled, cell);
            }

            public void Disable(Cell cell)
            {
                TriggerEvent(OnDisabled, cell);
            }
            #endregion

            #region Internal methods
            public void TriggerEvent(EventHandler<Cell> ev, Cell cell)
            {
                // Avoid possible race conditions
                EventHandler<Cell> e = ev;

                e?.Invoke(this, cell);
            }
            #endregion
        }

        public struct Cell
        {
            public Bounds bounds;
            public T value;

            #region Constructors
            public Cell(Vector3 center, float resolution, T val)
            {
                bounds = new Bounds(center, resolution * Vector3.one);
                value = val;
            }
            #endregion
        }

        public struct UpdateResult
        {
            #region Members
            public bool keepValue;
            public T value;
            #endregion

            #region Constructors
            public UpdateResult(bool keep, T val)
            {
                keepValue = keep;
                value = val;
            }
            #endregion
        }

        protected abstract class NodeBase
        {
            #region Abstract methods
            public abstract void Reset(Action<NodeBase> release);
            #endregion
        }

        protected class Node : NodeBase
        {
            #region Members
            public const int dim = 8;

            public NodeBase[] children;
            #endregion

            #region Constructors
            public Node()
            {
                children = new NodeBase[dim];
            }
            #endregion

            #region Public methods
            public override void Reset(Action<NodeBase> release)
            {
                for(int i = 0; i < dim; i++)
                {
                    release(children[i]);

                    children[i] = null;
                }
            }
            #endregion
        }

        protected class Leaf : NodeBase
        {
            #region Members
            public CellEvents events;
            public Vector3 center;
            public T value;
            #endregion

            #region Constructors
            public Leaf(Func<CellEvents, T> factory)
            {
                events = new CellEvents();
                center = Vector3.zero;
                value = factory(events);
            }
            #endregion

            #region Public methods
            public override void Reset(Action<NodeBase> release)
            {

            }
            #endregion
        }

        #region Members
        protected readonly float resolution;
        protected readonly float half_resolution;
        protected readonly int maxDepth;
        protected readonly Bounds bounds;
        protected NodeBase root;
        protected Stack<Node> nodePool;
        protected Stack<Leaf> leafPool;
        protected Func<CellEvents, T> factory;
        protected Func<T, T> reset;
        #endregion

        #region Constructors
        public Octree(Bounds bounds, float resolution, Func<CellEvents, T> factory, Func<T, T> reset)
        {
            this.resolution = resolution;
            this.factory = factory;
            this.reset = reset;

            half_resolution = 0.5f * resolution;
            
            nodePool = new Stack<Node>();
            leafPool = new Stack<Leaf>();

            root = null;

            float size = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

            maxDepth = Mathf.CeilToInt((float) Math.Log(size / resolution, 2));

            size = Mathf.Pow(2, maxDepth) * resolution;

            this.bounds = new Bounds(bounds.center, size * Vector3.one);

            if (maxDepth >= 32)
            {
                maxDepth = 0;

                throw new ArgumentOutOfRangeException("The bounds is too large or the resolution too small. It would result in a tree with a depth >= 32, which is not supported.", "bounds + resolution");
            }
        }
        #endregion

        #region Getters / Setters
        #endregion

        #region Public methods
        public bool Update(Vector3 position, Func<T, UpdateResult> updater)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            if(position.x < min.x || position.y < min.y || position.z < min.z || position.x >= max.x || position.y >= max.y || position.z >= max.z)
            {
                return false;
            }

            root = Update(root, maxDepth, bounds.center, position, updater);

            return true;
        }
        #endregion

        #region Internal methods
        protected Cell GetCell(Leaf leaf)
        {
            return new Cell(leaf.center, resolution, leaf.value);
        }

        protected Node GetNewNode()
        {
            Node result;

            if (nodePool.Count > 0)
            {
                result = nodePool.Pop();
            }
            else
            {
                result = new Node();
            }

            return result;
        }

        protected Leaf GetNewLeaf(Vector3 center)
        {
            Leaf leaf;

            if (leafPool.Count > 0)
            {
                leaf = leafPool.Pop();
            }
            else
            {
                leaf = new Leaf(factory);
            }

            leaf.center = center;
            leaf.value = reset(leaf.value);

            leaf.events.Enable(GetCell(leaf));

            return leaf;
        }

        protected void Release<U>(U node) where U : NodeBase
        {
            if (node != null)
            {
                switch(node)
                {
                    case Node n:
                        nodePool.Push(n);

                        break;

                    case Leaf leaf:
                        leaf.events.Disable(GetCell(leaf));

                        leafPool.Push(leaf);

                        break;
                }

                node.Reset(Release);
            }
        }

        protected NodeBase Update(NodeBase node, int depth, Vector3 center, Vector3 position, Func<T, UpdateResult> updater)
        {
            bool is_leaf = depth == 0;

            if (node == null)
            {
                if(is_leaf)
                {
                    node = GetNewLeaf(center);
                }
                else
                {
                    node = GetNewNode();
                }
            }

            if (is_leaf)
            {
                switch(node)
                {
                    case Node n:
                        // This should only happens when resolution is decreased.
                        throw new NotSupportedException("Invalid octree node where a leaf was expected.");
                }

                Leaf leaf = node as Leaf;

                UpdateResult r = updater(leaf.value);

                leaf.value = r.value;

                if(!r.keepValue)
                {
                    Release(leaf);

                    node = null;
                }
            }
            else
            {
                switch (node)
                {
                    case Leaf leaf:
                        // This should only happens when resolution is increased.
                        throw new NotSupportedException("Invalid octree leaf where a node was expected.");
                }

                Node n = node as Node;

                int x = position.x >= center.x ? 1 : 0;
                int y = position.y >= center.y ? 2 : 0;
                int z = position.z >= center.z ? 4 : 0;

                int index = x | y | z;

                int d = depth - 1;

                Vector3 c = center + half_resolution * (1 << d) * new Vector3((x << 1) - 1, y - 1, (z >> 1) - 1);

                n.children[index] = Update(n.children[index], d, c, position, updater);

                bool prune = true;

                foreach(NodeBase child in n.children)
                {
                    if(child != null)
                    {
                        prune = false;

                        break;
                    }
                }

                if(prune)
                {
                    Release(n);

                    node = null;
                }
            }

            return node;
        }
        #endregion
    }
}
