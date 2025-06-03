using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TigerUtilsLib;

partial class TigerUtils {
    public static PriorityGraph<TKey, TValue, TPriority>.Node NewPriorityGraphNode<TKey, TValue, TPriority>(TKey key, TValue value, TPriority priority, IEnumerable<TKey>? after = null, IEnumerable<TKey>? before = null, IEnumerable<TKey>? nodesAfter = null, IEnumerable<TKey>? nodesBefore = null) where TKey : notnull => new(key, value, priority, after, before, nodesAfter, nodesBefore);
    public static PriorityGraphK<TKey, TValue, TPriority>.Node NewPriorityGraphKNode<TKey, TValue, TPriority>(TValue value, TPriority priority, IEnumerable<TKey>? after = null, IEnumerable<TKey>? before = null, IEnumerable<TKey>? nodesAfter = null, IEnumerable<TKey>? nodesBefore = null) where TKey : notnull => new(value, priority, after, before, nodesAfter, nodesBefore);
    public static PriorityGraphPNode<TKey, TValue> NewPriorityGraphPNode<TKey, TValue>(TKey key, TValue value, IEnumerable<TKey>? after = null, IEnumerable<TKey>? before = null, IEnumerable<TKey>? nodesAfter = null, IEnumerable<TKey>? nodesBefore = null) where TKey : notnull => new(key, value, after, before, nodesAfter, nodesBefore);
    public static PriorityGraphSNode<TKey, TValue> NewPriorityGraphSNode<TKey, TValue>(TValue value, IEnumerable<TKey>? after = null, IEnumerable<TKey>? before = null, IEnumerable<TKey>? nodesAfter = null, IEnumerable<TKey>? nodesBefore = null) where TKey : notnull => new(value, after, before, nodesAfter, nodesBefore);
    public static PriorityGraph<TKey, TPriority>.Node NewPriorityGraphNode<TKey, TPriority>(TKey key, TPriority priority, IEnumerable<TKey>? after = null, IEnumerable<TKey>? before = null, IEnumerable<TKey>? nodesAfter = null, IEnumerable<TKey>? nodesBefore = null) where TKey : notnull => new(key, priority, after, before, nodesAfter, nodesBefore);
    public static PriorityGraphPNode<TKey> NewPriorityGraphPNode<TKey>(TKey key, IEnumerable<TKey>? after = null, IEnumerable<TKey>? before = null, IEnumerable<TKey>? nodesAfter = null, IEnumerable<TKey>? nodesBefore = null) where TKey : notnull => new(key, after, before, nodesAfter, nodesBefore);
}

partial class TigerClasses {
    public class PriorityGraph<TKey, TValue, TPriority> where TKey : notnull {
        #region Node
        /// <summary>
        /// 节点
        /// </summary>
        /// <param name="Key">节点的键</param>
        /// <param name="Value">节点的值</param>
        /// <param name="Priority">节点的优先度</param>
        /// <param name="After">表示将此节点放在哪些节点之后</param>
        /// <param name="Before">表示将此节点放在哪些节点之前</param>
        /// <param name="NodesAfter">表示将哪些节点放在此节点之后</param>
        /// <param name="NodesBefore">表示将哪些节点放在此节点之前</param>
        public record Node(TKey Key, TValue Value, TPriority Priority,
            IEnumerable<TKey>? After = null,
            IEnumerable<TKey>? Before = null,
            IEnumerable<TKey>? NodesAfter = null,
            IEnumerable<TKey>? NodesBefore = null) : INode;
        public interface INode {
            /// <summary>节点的键</summary>
            public TKey Key { get; }
            /// <summary>节点的值</summary>
            public TValue Value { get; }
            /// <summary>节点的优先度</summary>
            public TPriority Priority { get; }
            /// <summary>表示将此节点放在哪些节点之后</summary>
            public IEnumerable<TKey>? After { get; }
            /// <summary>表示将此节点放在哪些节点之前</summary>
            public IEnumerable<TKey>? Before { get; }
            /// <summary>表示将哪些节点放在此节点之后</summary>
            public IEnumerable<TKey>? NodesAfter { get; }
            /// <summary>表示将哪些节点放在此节点之前</summary>
            public IEnumerable<TKey>? NodesBefore { get; }
        }
        #endregion

        public static IEnumerable<TNode> Sort<TNode>(IEnumerable<TNode> nodes) where TNode : INode => Default.SortInner(nodes);
        public static PriorityGraph<TKey, TValue, TPriority> Default { get; } = new();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IEnumerable<TNode> SortInner<TNode>(IEnumerable<TNode> nodes) where TNode : INode => AllowMultipleKey ? Sort_AllowMultipleKey(nodes) : Sort_DisallowMutipleKey(nodes);

        #region 设置
        public IEqualityComparer<TKey>? KeyComparer { get; set; }
        public IComparer<TPriority> PriorityComparer { get; set; } = Comparer<TPriority>.Default;
        /// <summary>
        /// 是否允许节点中的键重复
        /// 若允许, 那么重复键的节点将被合并
        /// </summary>
        public bool AllowMultipleKey { get; set; }
        /// <summary>
        /// <br/>当出现环时是否不报错
        /// <br/>若为 <see langword="true"/>, 则会以切掉环的一边的方式破坏环,
        /// <br/>且出现环时会设置 <see cref="AnyCircularReference"/> 为 true
        /// <br/>使用 <see cref="ClearAnyCircularReference"/> 以清除它
        /// </summary>
        public bool AllowCircle { get; set; }
        public bool AnyCircularReference { get; private set; }
        public void ClearAnyCircularReference() => AnyCircularReference = false;
        private void ThrowCircularReferenceException() {
            if (AllowCircle) {
                AnyCircularReference = true;
                return;
            }
            throw new CircularReferenceException();
        }
        #endregion
        #region DisallowMultiplyKey
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TNode> Sort_DisallowMutipleKey<TNode>(IEnumerable<TNode> nodes) where TNode : INode {
            List<NodeHolder<TNode>> result = [];
            Dictionary<TKey, NodeHolder<TNode>> dictionary = new(KeyComparer);
            int len = 0;
            // 录入
            foreach (var node in nodes) {
                var holder = NewNodeHolder<TNode>(len++, node);
                result.Add(holder);
                dictionary.Add(node.Key, holder);
            }
            // 注册前后继
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void Register(Dictionary<TKey, NodeHolder<TNode>> dictionary, IEnumerable<TKey>? keys, NodeHolder<TNode> self, List<NodeHolder<TNode>> toFeed) {
                if (keys == null) {
                    return;
                }
                foreach (var key in keys) {
                    if (!dictionary.TryGetValue(key, out var other) || other.Index == self.Index) {
                        continue;
                    }
                    toFeed.Add(other);
                }
            }
            foreach (var r in result) {
                var node = r.Node;
                Register(dictionary, node.After, r, r.After);
                Register(dictionary, node.Before, r, r.Before);
                Register(dictionary, node.NodesAfter, r, r.NodesAfter);
                Register(dictionary, node.NodesBefore, r, r.NodesBefore);
            }
            return SortSupperInner(result);
        }
        #endregion
        #region AllowMultiplyKey
        private class NodeHolderGroup<TNode> where TNode : INode {
            public List<NodeHolder<TNode>> Nodes { get; } = [];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TNode> Sort_AllowMultipleKey<TNode>(IEnumerable<TNode> nodes) where TNode : INode {
            List<NodeHolder<TNode>> result = [];
            Dictionary<TKey, NodeHolderGroup<TNode>> groups = new(KeyComparer);
            int len = 0;
            // 录入
            foreach (var node in nodes) {
                var holder = NewNodeHolder<TNode>(len++, node);
                var group = groups.GetOrAdd(node.Key, static () => new());
                group.Nodes.Add(holder);
                result.Add(holder);
            }
            // 注册前后继
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void Register(Dictionary<TKey, NodeHolderGroup<TNode>> groups, IEnumerable<TKey>? keys, NodeHolderGroup<TNode> self, List<NodeHolder<TNode>> toFeed) {
                if (keys == null) {
                    return;
                }
                foreach (var key in keys) {
                    if (!groups.TryGetValue(key, out var other) || other == self) {
                        continue;
                    }
                    foreach (var n in other.Nodes) {
                        toFeed.Add(n);
                    }
                }
            }
            foreach (var r in result) {
                var node = r.Node;
                var group = groups[node.Key];
                Register(groups, node.After, group, r.After);
                Register(groups, node.Before, group, r.Before);
                Register(groups, node.NodesAfter, group, r.NodesAfter);
                Register(groups, node.NodesBefore, group, r.NodesBefore);
            }
            return SortSupperInner(result);
        }
        #endregion
        /// <param name="After">它要放在哪些节点之后</param>
        /// <param name="Before">它要放在哪些节点之前</param>
        /// <param name="NodesAfter">放在它之后的节点</param>
        /// <param name="NodesBefore">放在它之前的节点</param>
        /// <param name="AllNodesAfter">经排序后它之后的节点</param>
        /// <param name="AllNodesBefore">经排序后它之前的节点</param>
        private record NodeHolder<TNode>(int Index, TNode Node,
            List<NodeHolder<TNode>> After, List<NodeHolder<TNode>> Before,
            List<NodeHolder<TNode>> NodesAfter, List<NodeHolder<TNode>> NodesBefore,
            HashSet<NodeHolder<TNode>> AllNodesAfter, HashSet<NodeHolder<TNode>> AllNodesBefore) where TNode : INode {
            public override int GetHashCode() => Index;
        }
        private int CompareHolder<TNode>(NodeHolder<TNode> x, NodeHolder<TNode> y) where TNode : INode {
            var p = PriorityComparer.Compare(x.Node.Priority, y.Node.Priority);
            return p != 0 ? p : x.Index.CompareTo(y.Index);
        }
        private static NodeHolder<TNode> NewNodeHolder<TNode>(int index, TNode node) where TNode : INode => new(index, node, [], [], [], [], [], []);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TNode> SortSupperInner<TNode>(List<NodeHolder<TNode>> nodes) where TNode : INode {
            // 先粗略按照 priority 排序, 以 index 为第二键确保稳定性
            nodes.Sort(CompareHolder);
            NodeHolder<TNode>[] array = [.. nodes];
            int len = nodes.Count;
            for (int i = 0; i < len; ++i) {
                var node = array[i];
                // 按照 After, NodesBefore, Before, NodesAfter 的顺序
                if (node.After.Count != 0) {
                    foreach (var a in node.After) {
                        AddNodesBefore(node, a);
                    }
                    MoveAfter(node, nodes);
                }
                if (node.NodesBefore.Count != 0) {
                    foreach (var nb in node.NodesBefore) {
                        AddNodesBefore(node, nb);
                    }
                    MoveBefore(nodes, node);
                }
                if (node.Before.Count != 0) {
                    foreach (var b in node.Before) {
                        AddNodesAfter(node, b);
                    }
                    MoveBefore(node, nodes);
                }
                if (node.NodesAfter.Count != 0) {
                    foreach (var na in node.NodesAfter) {
                        AddNodesAfter(node, na);
                    }
                    MoveAfter(nodes, node);
                }
            }
            return nodes.Select(static n => n.Node);
        }

        private void AddNodesBefore<TNode>(NodeHolder<TNode> self, NodeHolder<TNode> nodeBefore) where TNode : INode {
            if (self.AllNodesAfter.Contains(nodeBefore)) {
                ThrowCircularReferenceException();
                return;
            }
            if (!AddEdge(nodeBefore, self)) {
                return;
            }
            foreach (var b in nodeBefore.AllNodesBefore) {
                if (self.Index == b.Index) {
                    ThrowCircularReferenceException();
                    continue;
                }
                AddNodesBefore(self, b);
            }
        }
        private void AddNodesAfter<TNode>(NodeHolder<TNode> self, NodeHolder<TNode> nodeAfter) where TNode : INode {
            if (self.AllNodesBefore.Contains(nodeAfter)) {
                ThrowCircularReferenceException();
                return;
            }
            if (!AddEdge(self, nodeAfter)) {
                return;
            }
            foreach (var a in nodeAfter.AllNodesAfter) {
                AddNodesAfter(self, a);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AddEdge<TNode>(NodeHolder<TNode> from, NodeHolder<TNode> to) where TNode : INode {
            if (!AddEdgeSimple(from, to)) {
                return false;
            }
            foreach (var beforeFrom in from.AllNodesBefore) {
                if (AddEdgeSimple(beforeFrom, to)) {
                    foreach (var afterTo in to.AllNodesAfter) {
                        AddEdgeSimple(beforeFrom, afterTo);
                    }
                }
            }
            foreach (var afterTo in to.AllNodesAfter) {
                if (AddEdgeSimple(from, afterTo)) {
                    foreach (var beforeFrom in from.AllNodesBefore) {
                        AddEdgeSimple(beforeFrom, afterTo);
                    }
                }
            }
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AddEdgeSimple<TNode>(NodeHolder<TNode> from, NodeHolder<TNode> to) where TNode : INode {
            if (!from.AllNodesAfter.Add(to)) {
                return false;
            }
            to.AllNodesBefore.Add(from);
            return true;
        }

        /// <summary>
        /// 将 <paramref name="self"/> 的 AllNodesBefore 放在自己的前面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveBefore<TNode>(List<NodeHolder<TNode>> nodes, NodeHolder<TNode> self) where TNode : INode {
            #region 找到自己的 Index
            int index = 0;
            int len = nodes.Count;
            for (; index < len; ++index) {
                if (nodes[index].Index == self.Index) {
                    break;
                }
            }
            if (index == len) {
                throw new Exception("self not in nodes!");
            }
            #endregion
            List<NodeHolder<TNode>> toMove = [];
            for (int i = len - 1; i >= index; --i) {
                var node = nodes[i];
                if (self.AllNodesBefore.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                if (toMove.Count != 0) {
                    nodes[i + toMove.Count] = node;
                }
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index + toMove.Count - 1 - i] = toMove[i];
            }
        }
        /// <summary>
        /// 将 <paramref name="self"/> 的 AllNodesAfter 放在自己的后面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveAfter<TNode>(List<NodeHolder<TNode>> nodes, NodeHolder<TNode> self) where TNode : INode {
            #region 找到自己的 Index
            int index = 0;
            int len = nodes.Count;
            for (; index < len; ++index) {
                if (nodes[index].Index == self.Index) {
                    break;
                }
            }
            if (index == len) {
                throw new Exception("self not in nodes!");
            }
            #endregion
            List<NodeHolder<TNode>> toMove = [];
            for (int i = 0; i <= index; ++i) {
                var node = nodes[i];
                if (self.AllNodesAfter.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                if (toMove.Count != 0) {
                    nodes[i - toMove.Count] = node;
                }
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index - toMove.Count + 1 + i] = toMove[i];
            }
        }
        /// <summary>
        /// 将 <paramref name="self"/> 以及 AllNodesBefore 放在 AllNodesAfter 的前面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveBefore<TNode>(NodeHolder<TNode> self, List<NodeHolder<TNode>> nodes) where TNode : INode {
            int index = 0, firstIndex = -1;
            int len = nodes.Count;
            for (; index < len; ++index) {
                var node = nodes[index];
                if (node.Index == self.Index) {
                    break;
                }
                if (firstIndex == -1 && self.AllNodesAfter.Contains(node)) {
                    firstIndex = index;
                }
            }
            if (index == len) {
                throw new Exception("self not in nodes!");
            }
            if (firstIndex == -1) {
                return;
            }
            List<NodeHolder<TNode>> toMove = [self];
            for (--index; index >= firstIndex; --index) {
                var node = nodes[index];
                if (self.AllNodesBefore.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                nodes[index + toMove.Count] = node;
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index + toMove.Count - i] = toMove[i];
            }
        }
        /// <summary>
        /// 将 <paramref name="self"/> 放在 AllNodesBefore 的后面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveAfter<TNode>(NodeHolder<TNode> self, List<NodeHolder<TNode>> nodes) where TNode : INode {
            int len = nodes.Count;
            int index = len - 1, lastIndex = -1;
            for (; index >= 0; --index) {
                var node = nodes[index];
                if (node.Index == self.Index) {
                    break;
                }
                if (lastIndex == -1 && self.AllNodesBefore.Contains(node)) {
                    lastIndex = index;
                }
            }
            if (index == -1) {
                throw new Exception("self not in nodes!");
            }
            if (lastIndex == -1) {
                return;
            }
            List<NodeHolder<TNode>> toMove = [self];
            for (++index; index <= lastIndex; ++index) {
                var node = nodes[index];
                if (self.AllNodesAfter.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                nodes[index - toMove.Count] = node;
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index - toMove.Count + i] = toMove[i];
            }
        }
    }
    public class PriorityGraphK<TKey, TValue, TPriority>(Func<TValue, TKey> keySelector) where TKey : notnull {
        #region Node
        /// <summary>
        /// 节点
        /// </summary>
        /// <param name="Value">节点的值</param>
        /// <param name="Priority">节点的优先度</param>
        /// <param name="After">表示将此节点放在哪些节点之后</param>
        /// <param name="Before">表示将此节点放在哪些节点之前</param>
        /// <param name="NodesAfter">表示将哪些节点放在此节点之后</param>
        /// <param name="NodesBefore">表示将哪些节点放在此节点之前</param>
        public record Node(TValue Value, TPriority Priority,
            IEnumerable<TKey>? After = null,
            IEnumerable<TKey>? Before = null,
            IEnumerable<TKey>? NodesAfter = null,
            IEnumerable<TKey>? NodesBefore = null) : INode;
        public interface INode {
            /// <summary>节点的值</summary>
            public TValue Value { get; }
            /// <summary>节点的优先度</summary>
            public TPriority Priority { get; }
            /// <summary>表示将此节点放在哪些节点之后</summary>
            public IEnumerable<TKey>? After { get; }
            /// <summary>表示将此节点放在哪些节点之前</summary>
            public IEnumerable<TKey>? Before { get; }
            /// <summary>表示将哪些节点放在此节点之后</summary>
            public IEnumerable<TKey>? NodesAfter { get; }
            /// <summary>表示将哪些节点放在此节点之前</summary>
            public IEnumerable<TKey>? NodesBefore { get; }
        }
        #endregion

        public static IEnumerable<TNode> Sort<TNode>(IEnumerable<TNode> nodes, Func<TValue, TKey> keySelector) where TNode : INode => new PriorityGraphK<TKey, TValue, TPriority>(keySelector).SortInner(nodes);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IEnumerable<TNode> SortInner<TNode>(IEnumerable<TNode> nodes) where TNode : INode => AllowMultipleKey ? Sort_AllowMultipleKey(nodes) : Sort_DisallowMutipleKey(nodes);

        #region 设置
        public IEqualityComparer<TKey>? KeyComparer { get; set; }
        public Func<TValue, TKey> KeySelector { get; set; } = keySelector;
        public IComparer<TPriority> PriorityComparer { get; set; } = Comparer<TPriority>.Default;
        /// <summary>
        /// 是否允许节点中的键重复
        /// 若允许, 那么重复键的节点将被合并
        /// </summary>
        public bool AllowMultipleKey { get; set; }
        /// <summary>
        /// <br/>当出现环时是否不报错
        /// <br/>若为 <see langword="true"/>, 则会以切掉环的一边的方式破坏环,
        /// <br/>且出现环时会设置 <see cref="AnyCircularReference"/> 为 true
        /// <br/>使用 <see cref="ClearAnyCircularReference"/> 以清除它
        /// </summary>
        public bool AllowCircle { get; set; }
        public bool AnyCircularReference { get; private set; }
        public void ClearAnyCircularReference() => AnyCircularReference = false;
        private void ThrowCircularReferenceException() {
            if (AllowCircle) {
                AnyCircularReference = true;
                return;
            }
            throw new CircularReferenceException();
        }

        #endregion
        #region DisallowMultiplyKey
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TNode> Sort_DisallowMutipleKey<TNode>(IEnumerable<TNode> nodes) where TNode : INode {
            List<NodeHolder<TNode>> result = [];
            Dictionary<TKey, NodeHolder<TNode>> dictionary = new(KeyComparer);
            int len = 0;
            // 录入
            foreach (var node in nodes) {
                var holder = NewNodeHolder<TNode>(len++, node);
                result.Add(holder);
                dictionary.Add(holder.Key, holder);
            }
            // 注册前后继
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void Register(Dictionary<TKey, NodeHolder<TNode>> dictionary, IEnumerable<TKey>? keys, NodeHolder<TNode> self, List<NodeHolder<TNode>> toFeed) {
                if (keys == null) {
                    return;
                }
                foreach (var key in keys) {
                    if (!dictionary.TryGetValue(key, out var other) || other.Index == self.Index) {
                        continue;
                    }
                    toFeed.Add(other);
                }
            }
            foreach (var r in result) {
                var node = r.Node;
                Register(dictionary, node.After, r, r.After);
                Register(dictionary, node.Before, r, r.Before);
                Register(dictionary, node.NodesAfter, r, r.NodesAfter);
                Register(dictionary, node.NodesBefore, r, r.NodesBefore);
            }
            return SortSupperInner(result);
        }
        #endregion
        #region AllowMultiplyKey
        private class NodeHolderGroup<TNode> where TNode : INode {
            public List<NodeHolder<TNode>> Nodes { get; } = [];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TNode> Sort_AllowMultipleKey<TNode>(IEnumerable<TNode> nodes) where TNode : INode {
            List<NodeHolder<TNode>> result = [];
            Dictionary<TKey, NodeHolderGroup<TNode>> groups = new(KeyComparer);
            int len = 0;
            // 录入
            foreach (var node in nodes) {
                var holder = NewNodeHolder<TNode>(len++, node);
                var group = groups.GetOrAdd(holder.Key, static () => new());
                group.Nodes.Add(holder);
                result.Add(holder);
            }
            // 注册前后继
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void Register(Dictionary<TKey, NodeHolderGroup<TNode>> groups, IEnumerable<TKey>? keys, NodeHolderGroup<TNode> self, List<NodeHolder<TNode>> toFeed) {
                if (keys == null) {
                    return;
                }
                foreach (var key in keys) {
                    if (!groups.TryGetValue(key, out var other) || other == self) {
                        continue;
                    }
                    foreach (var n in other.Nodes) {
                        toFeed.Add(n);
                    }
                }
            }
            foreach (var r in result) {
                var node = r.Node;
                var group = groups[r.Key];
                Register(groups, node.After, group, r.After);
                Register(groups, node.Before, group, r.Before);
                Register(groups, node.NodesAfter, group, r.NodesAfter);
                Register(groups, node.NodesBefore, group, r.NodesBefore);
            }
            return SortSupperInner(result);
        }
        #endregion
        /// <param name="After">它要放在哪些节点之后</param>
        /// <param name="Before">它要放在哪些节点之前</param>
        /// <param name="NodesAfter">放在它之后的节点</param>
        /// <param name="NodesBefore">放在它之前的节点</param>
        /// <param name="AllNodesAfter">经排序后它之后的节点</param>
        /// <param name="AllNodesBefore">经排序后它之前的节点</param>
        private record NodeHolder<TNode>(int Index, TNode Node, TKey Key,
            List<NodeHolder<TNode>> After, List<NodeHolder<TNode>> Before,
            List<NodeHolder<TNode>> NodesAfter, List<NodeHolder<TNode>> NodesBefore,
            HashSet<NodeHolder<TNode>> AllNodesAfter, HashSet<NodeHolder<TNode>> AllNodesBefore) where TNode : INode {
            public override int GetHashCode() => Index;
        }
        private int CompareHolder<TNode>(NodeHolder<TNode> x, NodeHolder<TNode> y) where TNode : INode {
            var p = PriorityComparer.Compare(x.Node.Priority, y.Node.Priority);
            return p != 0 ? p : (x.Index).CompareTo(y.Index);
        }
        private NodeHolder<TNode> NewNodeHolder<TNode>(int index, TNode node) where TNode : INode => new(index, node, KeySelector(node.Value), [], [], [], [], [], []);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TNode> SortSupperInner<TNode>(List<NodeHolder<TNode>> nodes) where TNode : INode {
            // 先粗略按照 priority 排序, 以 index 为第二键确保稳定性
            nodes.Sort(CompareHolder);
            NodeHolder<TNode>[] array = [.. nodes];
            int len = nodes.Count;
            for (int i = 0; i < len; ++i) {
                var node = array[i];
                // 按照 After, NodesBefore, Before, NodesAfter 的顺序
                if (node.After.Count != 0) {
                    foreach (var a in node.After) {
                        AddNodesBefore(node, a);
                    }
                    MoveAfter(node, nodes);
                }
                if (node.NodesBefore.Count != 0) {
                    foreach (var nb in node.NodesBefore) {
                        AddNodesBefore(node, nb);
                    }
                    MoveBefore(nodes, node);
                }
                if (node.Before.Count != 0) {
                    foreach (var b in node.Before) {
                        AddNodesAfter(node, b);
                    }
                    MoveBefore(node, nodes);
                }
                if (node.NodesAfter.Count != 0) {
                    foreach (var na in node.NodesAfter) {
                        AddNodesAfter(node, na);
                    }
                    MoveAfter(nodes, node);
                }
            }
            return nodes.Select(static n => n.Node);
        }

        private void AddNodesBefore<TNode>(NodeHolder<TNode> self, NodeHolder<TNode> nodeBefore) where TNode : INode {
            if (self.AllNodesAfter.Contains(nodeBefore)) {
                ThrowCircularReferenceException();
                return;
            }
            if (!AddEdge(nodeBefore, self)) {
                return;
            }
            foreach (var b in nodeBefore.AllNodesBefore) {
                if (self.Index == b.Index) {
                    ThrowCircularReferenceException();
                    continue;
                }
                AddNodesBefore(self, b);
            }
        }
        private void AddNodesAfter<TNode>(NodeHolder<TNode> self, NodeHolder<TNode> nodeAfter) where TNode : INode {
            if (self.AllNodesBefore.Contains(nodeAfter)) {
                ThrowCircularReferenceException();
                return;
            }
            if (!AddEdge(self, nodeAfter)) {
                return;
            }
            foreach (var a in nodeAfter.AllNodesAfter) {
                AddNodesAfter(self, a);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AddEdge<TNode>(NodeHolder<TNode> from, NodeHolder<TNode> to) where TNode : INode {
            if (!AddEdgeSimple(from, to)) {
                return false;
            }
            foreach (var beforeFrom in from.AllNodesBefore) {
                if (AddEdgeSimple(beforeFrom, to)) {
                    foreach (var afterTo in to.AllNodesAfter) {
                        AddEdgeSimple(beforeFrom, afterTo);
                    }
                }
            }
            foreach (var afterTo in to.AllNodesAfter) {
                if (AddEdgeSimple(from, afterTo)) {
                    foreach (var beforeFrom in from.AllNodesBefore) {
                        AddEdgeSimple(beforeFrom, afterTo);
                    }
                }
            }
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AddEdgeSimple<TNode>(NodeHolder<TNode> from, NodeHolder<TNode> to) where TNode : INode {
            if (!from.AllNodesAfter.Add(to)) {
                return false;
            }
            to.AllNodesBefore.Add(from);
            return true;
        }

        /// <summary>
        /// 将 <paramref name="self"/> 的 AllNodesBefore 放在自己的前面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveBefore<TNode>(List<NodeHolder<TNode>> nodes, NodeHolder<TNode> self) where TNode : INode {
            #region 找到自己的 Index
            int index = 0;
            int len = nodes.Count;
            for (; index < len; ++index) {
                if (nodes[index].Index == self.Index) {
                    break;
                }
            }
            if (index == len) {
                throw new Exception("self not in nodes!");
            }
            #endregion
            List<NodeHolder<TNode>> toMove = [];
            for (int i = len - 1; i >= index; --i) {
                var node = nodes[i];
                if (self.AllNodesBefore.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                if (toMove.Count != 0) {
                    nodes[i + toMove.Count] = node;
                }
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index + toMove.Count - 1 - i] = toMove[i];
            }
        }
        /// <summary>
        /// 将 <paramref name="self"/> 的 AllNodesAfter 放在自己的后面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveAfter<TNode>(List<NodeHolder<TNode>> nodes, NodeHolder<TNode> self) where TNode : INode {
            #region 找到自己的 Index
            int index = 0;
            int len = nodes.Count;
            for (; index < len; ++index) {
                if (nodes[index].Index == self.Index) {
                    break;
                }
            }
            if (index == len) {
                throw new Exception("self not in nodes!");
            }
            #endregion
            List<NodeHolder<TNode>> toMove = [];
            for (int i = 0; i <= index; ++i) {
                var node = nodes[i];
                if (self.AllNodesAfter.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                if (toMove.Count != 0) {
                    nodes[i - toMove.Count] = node;
                }
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index - toMove.Count + 1 + i] = toMove[i];
            }
        }
        /// <summary>
        /// 将 <paramref name="self"/> 以及 AllNodesBefore 放在 AllNodesAfter 的前面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveBefore<TNode>(NodeHolder<TNode> self, List<NodeHolder<TNode>> nodes) where TNode : INode {
            int index = 0, firstIndex = -1;
            int len = nodes.Count;
            for (; index < len; ++index) {
                var node = nodes[index];
                if (node.Index == self.Index) {
                    break;
                }
                if (firstIndex == -1 && self.AllNodesAfter.Contains(node)) {
                    firstIndex = index;
                }
            }
            if (index == len) {
                throw new Exception("self not in nodes!");
            }
            if (firstIndex == -1) {
                return;
            }
            List<NodeHolder<TNode>> toMove = [self];
            for (--index; index >= firstIndex; --index) {
                var node = nodes[index];
                if (self.AllNodesBefore.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                nodes[index + toMove.Count] = node;
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index + toMove.Count - i] = toMove[i];
            }
        }
        /// <summary>
        /// 将 <paramref name="self"/> 放在 AllNodesBefore 的后面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveAfter<TNode>(NodeHolder<TNode> self, List<NodeHolder<TNode>> nodes) where TNode : INode {
            int len = nodes.Count;
            int index = len - 1, lastIndex = -1;
            for (; index >= 0; --index) {
                var node = nodes[index];
                if (node.Index == self.Index) {
                    break;
                }
                if (lastIndex == -1 && self.AllNodesBefore.Contains(node)) {
                    lastIndex = index;
                }
            }
            if (index == -1) {
                throw new Exception("self not in nodes!");
            }
            if (lastIndex == -1) {
                return;
            }
            List<NodeHolder<TNode>> toMove = [self];
            for (++index; index <= lastIndex; ++index) {
                var node = nodes[index];
                if (self.AllNodesAfter.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                nodes[index - toMove.Count] = node;
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index - toMove.Count + i] = toMove[i];
            }
        }
    }
    #region Node
    /// <summary>
    /// 节点
    /// </summary>
    /// <param name="Key">节点的键</param>
    /// <param name="Value">节点的值</param>
    /// <param name="After">表示将此节点放在哪些节点之后</param>
    /// <param name="Before">表示将此节点放在哪些节点之前</param>
    /// <param name="NodesAfter">表示将哪些节点放在此节点之后</param>
    /// <param name="NodesBefore">表示将哪些节点放在此节点之前</param>
    public record PriorityGraphPNode<TKey, TValue>(TKey Key, TValue Value,
        IEnumerable<TKey>? After = null,
        IEnumerable<TKey>? Before = null,
        IEnumerable<TKey>? NodesAfter = null,
        IEnumerable<TKey>? NodesBefore = null) : IPriorityGraphPNode<TKey, TValue> where TKey : notnull;
    public interface IPriorityGraphPNode<TKey, TValue> where TKey : notnull {
        /// <summary>节点的键</summary>
        public TKey Key { get; }
        /// <summary>节点的值</summary>
        public TValue Value { get; }
        /// <summary>表示将此节点放在哪些节点之后</summary>
        public IEnumerable<TKey>? After { get; }
        /// <summary>表示将此节点放在哪些节点之前</summary>
        public IEnumerable<TKey>? Before { get; }
        /// <summary>表示将哪些节点放在此节点之后</summary>
        public IEnumerable<TKey>? NodesAfter { get; }
        /// <summary>表示将哪些节点放在此节点之前</summary>
        public IEnumerable<TKey>? NodesBefore { get; }
    }
    #endregion
    public class PriorityGraphP<TKey, TValue, TPriority>(Func<TValue, TPriority> prioritySelector) where TKey : notnull {
        public static IEnumerable<TNode> Sort<TNode>(IEnumerable<TNode> nodes, Func<TValue, TPriority> prioritySelector) where TNode : IPriorityGraphPNode<TKey, TValue> => new PriorityGraphP<TKey, TValue, TPriority>(prioritySelector).SortInner(nodes);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IEnumerable<TNode> SortInner<TNode>(IEnumerable<TNode> nodes) where TNode : IPriorityGraphPNode<TKey, TValue> => AllowMultipleKey ? Sort_AllowMultipleKey(nodes) : Sort_DisallowMutipleKey(nodes);

        #region 设置
        public IEqualityComparer<TKey>? KeyComparer { get; set; }
        public Func<TValue, TPriority> PrioritySelector { get; set; } = prioritySelector;
        public IComparer<TPriority> PriorityComparer { get; set; } = Comparer<TPriority>.Default;
        /// <summary>
        /// 是否允许节点中的键重复
        /// 若允许, 那么重复键的节点将被合并
        /// </summary>
        public bool AllowMultipleKey { get; set; }
        /// <summary>
        /// <br/>当出现环时是否不报错
        /// <br/>若为 <see langword="true"/>, 则会以切掉环的一边的方式破坏环,
        /// <br/>且出现环时会设置 <see cref="AnyCircularReference"/> 为 true
        /// <br/>使用 <see cref="ClearAnyCircularReference"/> 以清除它
        /// </summary>
        public bool AllowCircle { get; set; }
        public bool AnyCircularReference { get; private set; }
        public void ClearAnyCircularReference() => AnyCircularReference = false;
        private void ThrowCircularReferenceException() {
            if (AllowCircle) {
                AnyCircularReference = true;
                return;
            }
            throw new CircularReferenceException();
        }
        #endregion
        #region DisallowMultiplyKey
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TNode> Sort_DisallowMutipleKey<TNode>(IEnumerable<TNode> nodes) where TNode : IPriorityGraphPNode<TKey, TValue> {
            List<NodeHolder<TNode>> result = [];
            Dictionary<TKey, NodeHolder<TNode>> dictionary = new(KeyComparer);
            int len = 0;
            // 录入
            foreach (var node in nodes) {
                var holder = NewNodeHolder<TNode>(len++, node);
                result.Add(holder);
                dictionary.Add(node.Key, holder);
            }
            // 注册前后继
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void Register(Dictionary<TKey, NodeHolder<TNode>> dictionary, IEnumerable<TKey>? keys, NodeHolder<TNode> self, List<NodeHolder<TNode>> toFeed) {
                if (keys == null) {
                    return;
                }
                foreach (var key in keys) {
                    if (!dictionary.TryGetValue(key, out var other) || other.Index == self.Index) {
                        continue;
                    }
                    toFeed.Add(other);
                }
            }
            foreach (var r in result) {
                var node = r.Node;
                Register(dictionary, node.After, r, r.After);
                Register(dictionary, node.Before, r, r.Before);
                Register(dictionary, node.NodesAfter, r, r.NodesAfter);
                Register(dictionary, node.NodesBefore, r, r.NodesBefore);
            }
            return SortSupperInner(result);
        }
        #endregion
        #region AllowMultiplyKey
        private class NodeHolderGroup<TNode> where TNode : IPriorityGraphPNode<TKey, TValue> {
            public List<NodeHolder<TNode>> Nodes { get; } = [];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TNode> Sort_AllowMultipleKey<TNode>(IEnumerable<TNode> nodes) where TNode : IPriorityGraphPNode<TKey, TValue> {
            List<NodeHolder<TNode>> result = [];
            Dictionary<TKey, NodeHolderGroup<TNode>> groups = new(KeyComparer);
            int len = 0;
            // 录入
            foreach (var node in nodes) {
                var holder = NewNodeHolder<TNode>(len++, node);
                var group = groups.GetOrAdd(node.Key, static () => new());
                group.Nodes.Add(holder);
                result.Add(holder);
            }
            // 注册前后继
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void Register(Dictionary<TKey, NodeHolderGroup<TNode>> groups, IEnumerable<TKey>? keys, NodeHolderGroup<TNode> self, List<NodeHolder<TNode>> toFeed) {
                if (keys == null) {
                    return;
                }
                foreach (var key in keys) {
                    if (!groups.TryGetValue(key, out var other) || other == self) {
                        continue;
                    }
                    foreach (var n in other.Nodes) {
                        toFeed.Add(n);
                    }
                }
            }
            foreach (var r in result) {
                var node = r.Node;
                var group = groups[node.Key];
                Register(groups, node.After, group, r.After);
                Register(groups, node.Before, group, r.Before);
                Register(groups, node.NodesAfter, group, r.NodesAfter);
                Register(groups, node.NodesBefore, group, r.NodesBefore);
            }
            return SortSupperInner(result);
        }
        #endregion
        /// <param name="After">它要放在哪些节点之后</param>
        /// <param name="Before">它要放在哪些节点之前</param>
        /// <param name="NodesAfter">放在它之后的节点</param>
        /// <param name="NodesBefore">放在它之前的节点</param>
        /// <param name="AllNodesAfter">经排序后它之后的节点</param>
        /// <param name="AllNodesBefore">经排序后它之前的节点</param>
        private record NodeHolder<TNode>(int Index, TNode Node, TPriority Priority,
            List<NodeHolder<TNode>> After, List<NodeHolder<TNode>> Before,
            List<NodeHolder<TNode>> NodesAfter, List<NodeHolder<TNode>> NodesBefore,
            HashSet<NodeHolder<TNode>> AllNodesAfter, HashSet<NodeHolder<TNode>> AllNodesBefore) where TNode : IPriorityGraphPNode<TKey, TValue> {
            public override int GetHashCode() => Index;
        }
        private int CompareHolder<TNode>(NodeHolder<TNode> x, NodeHolder<TNode> y) where TNode : IPriorityGraphPNode<TKey, TValue> {
            var p = PriorityComparer.Compare(x.Priority, y.Priority);
            return p != 0 ? p : x.Index.CompareTo(y.Index);
        }
        private NodeHolder<TNode> NewNodeHolder<TNode>(int index, TNode node) where TNode : IPriorityGraphPNode<TKey, TValue> => new(index, node, PrioritySelector(node.Value), [], [], [], [], [], []);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TNode> SortSupperInner<TNode>(List<NodeHolder<TNode>> nodes) where TNode : IPriorityGraphPNode<TKey, TValue> {
            // 先粗略按照 priority 排序, 以 index 为第二键确保稳定性
            nodes.Sort(CompareHolder);
            NodeHolder<TNode>[] array = [.. nodes];
            int len = nodes.Count;
            for (int i = 0; i < len; ++i) {
                var node = array[i];
                // 按照 After, NodesBefore, Before, NodesAfter 的顺序
                if (node.After.Count != 0) {
                    foreach (var a in node.After) {
                        AddNodesBefore(node, a);
                    }
                    MoveAfter(node, nodes);
                }
                if (node.NodesBefore.Count != 0) {
                    foreach (var nb in node.NodesBefore) {
                        AddNodesBefore(node, nb);
                    }
                    MoveBefore(nodes, node);
                }
                if (node.Before.Count != 0) {
                    foreach (var b in node.Before) {
                        AddNodesAfter(node, b);
                    }
                    MoveBefore(node, nodes);
                }
                if (node.NodesAfter.Count != 0) {
                    foreach (var na in node.NodesAfter) {
                        AddNodesAfter(node, na);
                    }
                    MoveAfter(nodes, node);
                }
            }
            return nodes.Select(static n => n.Node);
        }

        private void AddNodesBefore<TNode>(NodeHolder<TNode> self, NodeHolder<TNode> nodeBefore) where TNode : IPriorityGraphPNode<TKey, TValue> {
            if (self.AllNodesAfter.Contains(nodeBefore)) {
                ThrowCircularReferenceException();
                return;
            }
            if (!AddEdge(nodeBefore, self)) {
                return;
            }
            foreach (var b in nodeBefore.AllNodesBefore) {
                if (self.Index == b.Index) {
                    ThrowCircularReferenceException();
                    continue;
                }
                AddNodesBefore(self, b);
            }
        }
        private void AddNodesAfter<TNode>(NodeHolder<TNode> self, NodeHolder<TNode> nodeAfter) where TNode : IPriorityGraphPNode<TKey, TValue> {
            if (self.AllNodesBefore.Contains(nodeAfter)) {
                ThrowCircularReferenceException();
                return;
            }
            if (!AddEdge(self, nodeAfter)) {
                return;
            }
            foreach (var a in nodeAfter.AllNodesAfter) {
                AddNodesAfter(self, a);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AddEdge<TNode>(NodeHolder<TNode> from, NodeHolder<TNode> to) where TNode : IPriorityGraphPNode<TKey, TValue> {
            if (!AddEdgeSimple(from, to)) {
                return false;
            }
            foreach (var beforeFrom in from.AllNodesBefore) {
                if (AddEdgeSimple(beforeFrom, to)) {
                    foreach (var afterTo in to.AllNodesAfter) {
                        AddEdgeSimple(beforeFrom, afterTo);
                    }
                }
            }
            foreach (var afterTo in to.AllNodesAfter) {
                if (AddEdgeSimple(from, afterTo)) {
                    foreach (var beforeFrom in from.AllNodesBefore) {
                        AddEdgeSimple(beforeFrom, afterTo);
                    }
                }
            }
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AddEdgeSimple<TNode>(NodeHolder<TNode> from, NodeHolder<TNode> to) where TNode : IPriorityGraphPNode<TKey, TValue> {
            if (!from.AllNodesAfter.Add(to)) {
                return false;
            }
            to.AllNodesBefore.Add(from);
            return true;
        }

        /// <summary>
        /// 将 <paramref name="self"/> 的 AllNodesBefore 放在自己的前面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveBefore<TNode>(List<NodeHolder<TNode>> nodes, NodeHolder<TNode> self) where TNode : IPriorityGraphPNode<TKey, TValue> {
            #region 找到自己的 Index
            int index = 0;
            int len = nodes.Count;
            for (; index < len; ++index) {
                if (nodes[index].Index == self.Index) {
                    break;
                }
            }
            if (index == len) {
                throw new Exception("self not in nodes!");
            }
            #endregion
            List<NodeHolder<TNode>> toMove = [];
            for (int i = len - 1; i >= index; --i) {
                var node = nodes[i];
                if (self.AllNodesBefore.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                if (toMove.Count != 0) {
                    nodes[i + toMove.Count] = node;
                }
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index + toMove.Count - 1 - i] = toMove[i];
            }
        }
        /// <summary>
        /// 将 <paramref name="self"/> 的 AllNodesAfter 放在自己的后面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveAfter<TNode>(List<NodeHolder<TNode>> nodes, NodeHolder<TNode> self) where TNode : IPriorityGraphPNode<TKey, TValue> {
            #region 找到自己的 Index
            int index = 0;
            int len = nodes.Count;
            for (; index < len; ++index) {
                if (nodes[index].Index == self.Index) {
                    break;
                }
            }
            if (index == len) {
                throw new Exception("self not in nodes!");
            }
            #endregion
            List<NodeHolder<TNode>> toMove = [];
            for (int i = 0; i <= index; ++i) {
                var node = nodes[i];
                if (self.AllNodesAfter.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                if (toMove.Count != 0) {
                    nodes[i - toMove.Count] = node;
                }
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index - toMove.Count + 1 + i] = toMove[i];
            }
        }
        /// <summary>
        /// 将 <paramref name="self"/> 以及 AllNodesBefore 放在 AllNodesAfter 的前面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveBefore<TNode>(NodeHolder<TNode> self, List<NodeHolder<TNode>> nodes) where TNode : IPriorityGraphPNode<TKey, TValue> {
            int index = 0, firstIndex = -1;
            int len = nodes.Count;
            for (; index < len; ++index) {
                var node = nodes[index];
                if (node.Index == self.Index) {
                    break;
                }
                if (firstIndex == -1 && self.AllNodesAfter.Contains(node)) {
                    firstIndex = index;
                }
            }
            if (index == len) {
                throw new Exception("self not in nodes!");
            }
            if (firstIndex == -1) {
                return;
            }
            List<NodeHolder<TNode>> toMove = [self];
            for (--index; index >= firstIndex; --index) {
                var node = nodes[index];
                if (self.AllNodesBefore.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                nodes[index + toMove.Count] = node;
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index + toMove.Count - i] = toMove[i];
            }
        }
        /// <summary>
        /// 将 <paramref name="self"/> 放在 AllNodesBefore 的后面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveAfter<TNode>(NodeHolder<TNode> self, List<NodeHolder<TNode>> nodes) where TNode : IPriorityGraphPNode<TKey, TValue> {
            int len = nodes.Count;
            int index = len - 1, lastIndex = -1;
            for (; index >= 0; --index) {
                var node = nodes[index];
                if (node.Index == self.Index) {
                    break;
                }
                if (lastIndex == -1 && self.AllNodesBefore.Contains(node)) {
                    lastIndex = index;
                }
            }
            if (index == -1) {
                throw new Exception("self not in nodes!");
            }
            if (lastIndex == -1) {
                return;
            }
            List<NodeHolder<TNode>> toMove = [self];
            for (++index; index <= lastIndex; ++index) {
                var node = nodes[index];
                if (self.AllNodesAfter.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                nodes[index - toMove.Count] = node;
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index - toMove.Count + i] = toMove[i];
            }
        }
    }
    #region Node
    /// <summary>
    /// 节点
    /// </summary>
    /// <param name="Value">节点的值</param>
    /// <param name="After">表示将此节点放在哪些节点之后</param>
    /// <param name="Before">表示将此节点放在哪些节点之前</param>
    /// <param name="NodesAfter">表示将哪些节点放在此节点之后</param>
    /// <param name="NodesBefore">表示将哪些节点放在此节点之前</param>
    public record PriorityGraphSNode<TKey, TValue>(TValue Value,
        IEnumerable<TKey>? After = null,
        IEnumerable<TKey>? Before = null,
        IEnumerable<TKey>? NodesAfter = null,
        IEnumerable<TKey>? NodesBefore = null) : IPriorityGraphSNode<TKey, TValue> where TKey : notnull;
    public interface IPriorityGraphSNode<TKey, TValue> where TKey : notnull {
        /// <summary>节点的值</summary>
        public TValue Value { get; }
        /// <summary>表示将此节点放在哪些节点之后</summary>
        public IEnumerable<TKey>? After { get; }
        /// <summary>表示将此节点放在哪些节点之前</summary>
        public IEnumerable<TKey>? Before { get; }
        /// <summary>表示将哪些节点放在此节点之后</summary>
        public IEnumerable<TKey>? NodesAfter { get; }
        /// <summary>表示将哪些节点放在此节点之前</summary>
        public IEnumerable<TKey>? NodesBefore { get; }
    }
    #endregion
    public class PriorityGraphS<TKey, TValue, TPriority>(Func<TValue, TKey> keySelector, Func<TValue, TPriority> prioritySelector) where TKey : notnull {

        public static IEnumerable<TNode> Sort<TNode>(IEnumerable<TNode> nodes, Func<TValue, TKey> keySelector, Func<TValue, TPriority> prioritySelector) where TNode : IPriorityGraphSNode<TKey, TValue> => new PriorityGraphS<TKey, TValue, TPriority>(keySelector, prioritySelector).SortInner(nodes);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IEnumerable<TNode> SortInner<TNode>(IEnumerable<TNode> nodes) where TNode : IPriorityGraphSNode<TKey, TValue> => AllowMultipleKey ? Sort_AllowMultipleKey(nodes) : Sort_DisallowMutipleKey(nodes);

        #region 设置
        public IEqualityComparer<TKey>? KeyComparer { get; set; }
        public Func<TValue, TKey> KeySelector { get; set; } = keySelector;
        public Func<TValue, TPriority> PrioritySelector { get; set; } = prioritySelector;
        public IComparer<TPriority> PriorityComparer { get; set; } = Comparer<TPriority>.Default;
        /// <summary>
        /// 是否允许节点中的键重复
        /// 若允许, 那么重复键的节点将被合并
        /// </summary>
        public bool AllowMultipleKey { get; set; }
        /// <summary>
        /// <br/>当出现环时是否不报错
        /// <br/>若为 <see langword="true"/>, 则会以切掉环的一边的方式破坏环,
        /// <br/>且出现环时会设置 <see cref="AnyCircularReference"/> 为 true
        /// <br/>使用 <see cref="ClearAnyCircularReference"/> 以清除它
        /// </summary>
        public bool AllowCircle { get; set; }
        public bool AnyCircularReference { get; private set; }
        public void ClearAnyCircularReference() => AnyCircularReference = false;
        private void ThrowCircularReferenceException() {
            if (AllowCircle) {
                AnyCircularReference = true;
                return;
            }
            throw new CircularReferenceException();
        }
        #endregion
        #region DisallowMultiplyKey
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TNode> Sort_DisallowMutipleKey<TNode>(IEnumerable<TNode> nodes) where TNode : IPriorityGraphSNode<TKey, TValue> {
            List<NodeHolder<TNode>> result = [];
            Dictionary<TKey, NodeHolder<TNode>> dictionary = new(KeyComparer);
            int len = 0;
            // 录入
            foreach (var node in nodes) {
                var holder = NewNodeHolder<TNode>(len++, node);
                result.Add(holder);
                dictionary.Add(holder.Key, holder);
            }
            // 注册前后继
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void Register(Dictionary<TKey, NodeHolder<TNode>> dictionary, IEnumerable<TKey>? keys, NodeHolder<TNode> self, List<NodeHolder<TNode>> toFeed) {
                if (keys == null) {
                    return;
                }
                foreach (var key in keys) {
                    if (!dictionary.TryGetValue(key, out var other) || other.Index == self.Index) {
                        continue;
                    }
                    toFeed.Add(other);
                }
            }
            foreach (var r in result) {
                var node = r.Node;
                Register(dictionary, node.After, r, r.After);
                Register(dictionary, node.Before, r, r.Before);
                Register(dictionary, node.NodesAfter, r, r.NodesAfter);
                Register(dictionary, node.NodesBefore, r, r.NodesBefore);
            }
            return SortSupperInner(result);
        }
        #endregion
        #region AllowMultiplyKey
        private class NodeHolderGroup<TNode> where TNode : IPriorityGraphSNode<TKey, TValue> {
            public List<NodeHolder<TNode>> Nodes { get; } = [];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TNode> Sort_AllowMultipleKey<TNode>(IEnumerable<TNode> nodes) where TNode : IPriorityGraphSNode<TKey, TValue> {
            List<NodeHolder<TNode>> result = [];
            Dictionary<TKey, NodeHolderGroup<TNode>> groups = new(KeyComparer);
            int len = 0;
            // 录入
            foreach (var node in nodes) {
                var holder = NewNodeHolder<TNode>(len++, node);
                var group = groups.GetOrAdd(holder.Key, static () => new());
                group.Nodes.Add(holder);
                result.Add(holder);
            }
            // 注册前后继
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void Register(Dictionary<TKey, NodeHolderGroup<TNode>> groups, IEnumerable<TKey>? keys, NodeHolderGroup<TNode> self, List<NodeHolder<TNode>> toFeed) {
                if (keys == null) {
                    return;
                }
                foreach (var key in keys) {
                    if (!groups.TryGetValue(key, out var other) || other == self) {
                        continue;
                    }
                    foreach (var n in other.Nodes) {
                        toFeed.Add(n);
                    }
                }
            }
            foreach (var r in result) {
                var node = r.Node;
                var group = groups[r.Key];
                Register(groups, node.After, group, r.After);
                Register(groups, node.Before, group, r.Before);
                Register(groups, node.NodesAfter, group, r.NodesAfter);
                Register(groups, node.NodesBefore, group, r.NodesBefore);
            }
            return SortSupperInner(result);
        }
        #endregion
        /// <param name="After">它要放在哪些节点之后</param>
        /// <param name="Before">它要放在哪些节点之前</param>
        /// <param name="NodesAfter">放在它之后的节点</param>
        /// <param name="NodesBefore">放在它之前的节点</param>
        /// <param name="AllNodesAfter">经排序后它之后的节点</param>
        /// <param name="AllNodesBefore">经排序后它之前的节点</param>
        private record NodeHolder<TNode>(int Index, TNode Node, TKey Key, TPriority Priority,
            List<NodeHolder<TNode>> After, List<NodeHolder<TNode>> Before,
            List<NodeHolder<TNode>> NodesAfter, List<NodeHolder<TNode>> NodesBefore,
            HashSet<NodeHolder<TNode>> AllNodesAfter, HashSet<NodeHolder<TNode>> AllNodesBefore) where TNode : IPriorityGraphSNode<TKey, TValue> {
            public override int GetHashCode() => Index;
        }
        private int CompareHolder<TNode>(NodeHolder<TNode> x, NodeHolder<TNode> y) where TNode : IPriorityGraphSNode<TKey, TValue> {
            var p = PriorityComparer.Compare(x.Priority, y.Priority);
            return p != 0 ? p : x.Index.CompareTo(y.Index);
        }
        private NodeHolder<TNode> NewNodeHolder<TNode>(int index, TNode node) where TNode : IPriorityGraphSNode<TKey, TValue> => new(index, node, KeySelector(node.Value), PrioritySelector(node.Value), [], [], [], [], [], []);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TNode> SortSupperInner<TNode>(List<NodeHolder<TNode>> nodes) where TNode : IPriorityGraphSNode<TKey, TValue> {
            // 先粗略按照 priority 排序, 以 index 为第二键确保稳定性
            nodes.Sort(CompareHolder);
            NodeHolder<TNode>[] array = [.. nodes];
            int len = nodes.Count;
            for (int i = 0; i < len; ++i) {
                var node = array[i];
                // 按照 After, NodesBefore, Before, NodesAfter 的顺序
                if (node.After.Count != 0) {
                    foreach (var a in node.After) {
                        AddNodesBefore(node, a);
                    }
                    MoveAfter(node, nodes);
                }
                if (node.NodesBefore.Count != 0) {
                    foreach (var nb in node.NodesBefore) {
                        AddNodesBefore(node, nb);
                    }
                    MoveBefore(nodes, node);
                }
                if (node.Before.Count != 0) {
                    foreach (var b in node.Before) {
                        AddNodesAfter(node, b);
                    }
                    MoveBefore(node, nodes);
                }
                if (node.NodesAfter.Count != 0) {
                    foreach (var na in node.NodesAfter) {
                        AddNodesAfter(node, na);
                    }
                    MoveAfter(nodes, node);
                }
            }
            return nodes.Select(static n => n.Node);
        }

        private void AddNodesBefore<TNode>(NodeHolder<TNode> self, NodeHolder<TNode> nodeBefore) where TNode : IPriorityGraphSNode<TKey, TValue> {
            if (self.AllNodesAfter.Contains(nodeBefore)) {
                ThrowCircularReferenceException();
                return;
            }
            if (!AddEdge(nodeBefore, self)) {
                return;
            }
            foreach (var b in nodeBefore.AllNodesBefore) {
                if (self.Index == b.Index) {
                    ThrowCircularReferenceException();
                    continue;
                }
                AddNodesBefore(self, b);
            }
        }
        private void AddNodesAfter<TNode>(NodeHolder<TNode> self, NodeHolder<TNode> nodeAfter) where TNode : IPriorityGraphSNode<TKey, TValue> {
            if (self.AllNodesBefore.Contains(nodeAfter)) {
                ThrowCircularReferenceException();
                return;
            }
            if (!AddEdge(self, nodeAfter)) {
                return;
            }
            foreach (var a in nodeAfter.AllNodesAfter) {
                AddNodesAfter(self, a);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AddEdge<TNode>(NodeHolder<TNode> from, NodeHolder<TNode> to) where TNode : IPriorityGraphSNode<TKey, TValue> {
            if (!AddEdgeSimple(from, to)) {
                return false;
            }
            foreach (var beforeFrom in from.AllNodesBefore) {
                if (AddEdgeSimple(beforeFrom, to)) {
                    foreach (var afterTo in to.AllNodesAfter) {
                        AddEdgeSimple(beforeFrom, afterTo);
                    }
                }
            }
            foreach (var afterTo in to.AllNodesAfter) {
                if (AddEdgeSimple(from, afterTo)) {
                    foreach (var beforeFrom in from.AllNodesBefore) {
                        AddEdgeSimple(beforeFrom, afterTo);
                    }
                }
            }
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AddEdgeSimple<TNode>(NodeHolder<TNode> from, NodeHolder<TNode> to) where TNode : IPriorityGraphSNode<TKey, TValue> {
            if (!from.AllNodesAfter.Add(to)) {
                return false;
            }
            to.AllNodesBefore.Add(from);
            return true;
        }

        /// <summary>
        /// 将 <paramref name="self"/> 的 AllNodesBefore 放在自己的前面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveBefore<TNode>(List<NodeHolder<TNode>> nodes, NodeHolder<TNode> self) where TNode : IPriorityGraphSNode<TKey, TValue> {
            #region 找到自己的 Index
            int index = 0;
            int len = nodes.Count;
            for (; index < len; ++index) {
                if (nodes[index].Index == self.Index) {
                    break;
                }
            }
            if (index == len) {
                throw new Exception("self not in nodes!");
            }
            #endregion
            List<NodeHolder<TNode>> toMove = [];
            for (int i = len - 1; i >= index; --i) {
                var node = nodes[i];
                if (self.AllNodesBefore.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                if (toMove.Count != 0) {
                    nodes[i + toMove.Count] = node;
                }
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index + toMove.Count - 1 - i] = toMove[i];
            }
        }
        /// <summary>
        /// 将 <paramref name="self"/> 的 AllNodesAfter 放在自己的后面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveAfter<TNode>(List<NodeHolder<TNode>> nodes, NodeHolder<TNode> self) where TNode : IPriorityGraphSNode<TKey, TValue> {
            #region 找到自己的 Index
            int index = 0;
            int len = nodes.Count;
            for (; index < len; ++index) {
                if (nodes[index].Index == self.Index) {
                    break;
                }
            }
            if (index == len) {
                throw new Exception("self not in nodes!");
            }
            #endregion
            List<NodeHolder<TNode>> toMove = [];
            for (int i = 0; i <= index; ++i) {
                var node = nodes[i];
                if (self.AllNodesAfter.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                if (toMove.Count != 0) {
                    nodes[i - toMove.Count] = node;
                }
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index - toMove.Count + 1 + i] = toMove[i];
            }
        }
        /// <summary>
        /// 将 <paramref name="self"/> 以及 AllNodesBefore 放在 AllNodesAfter 的前面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveBefore<TNode>(NodeHolder<TNode> self, List<NodeHolder<TNode>> nodes) where TNode : IPriorityGraphSNode<TKey, TValue> {
            int index = 0, firstIndex = -1;
            int len = nodes.Count;
            for (; index < len; ++index) {
                var node = nodes[index];
                if (node.Index == self.Index) {
                    break;
                }
                if (firstIndex == -1 && self.AllNodesAfter.Contains(node)) {
                    firstIndex = index;
                }
            }
            if (index == len) {
                throw new Exception("self not in nodes!");
            }
            if (firstIndex == -1) {
                return;
            }
            List<NodeHolder<TNode>> toMove = [self];
            for (--index; index >= firstIndex; --index) {
                var node = nodes[index];
                if (self.AllNodesBefore.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                nodes[index + toMove.Count] = node;
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index + toMove.Count - i] = toMove[i];
            }
        }
        /// <summary>
        /// 将 <paramref name="self"/> 放在 AllNodesBefore 的后面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveAfter<TNode>(NodeHolder<TNode> self, List<NodeHolder<TNode>> nodes) where TNode : IPriorityGraphSNode<TKey, TValue> {
            int len = nodes.Count;
            int index = len - 1, lastIndex = -1;
            for (; index >= 0; --index) {
                var node = nodes[index];
                if (node.Index == self.Index) {
                    break;
                }
                if (lastIndex == -1 && self.AllNodesBefore.Contains(node)) {
                    lastIndex = index;
                }
            }
            if (index == -1) {
                throw new Exception("self not in nodes!");
            }
            if (lastIndex == -1) {
                return;
            }
            List<NodeHolder<TNode>> toMove = [self];
            for (++index; index <= lastIndex; ++index) {
                var node = nodes[index];
                if (self.AllNodesAfter.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                nodes[index - toMove.Count] = node;
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index - toMove.Count + i] = toMove[i];
            }
        }
    }
    public class PriorityGraph<TKey, TPriority> where TKey : notnull {
        #region Node
        /// <summary>
        /// 节点
        /// </summary>
        /// <param name="Key">节点的键</param>
        /// <param name="Priority">节点的优先度</param>
        /// <param name="After">表示将此节点放在哪些节点之后</param>
        /// <param name="Before">表示将此节点放在哪些节点之前</param>
        /// <param name="NodesAfter">表示将哪些节点放在此节点之后</param>
        /// <param name="NodesBefore">表示将哪些节点放在此节点之前</param>
        public record Node(TKey Key, TPriority Priority,
            IEnumerable<TKey>? After = null,
            IEnumerable<TKey>? Before = null,
            IEnumerable<TKey>? NodesAfter = null,
            IEnumerable<TKey>? NodesBefore = null) : INode;
        public interface INode {
            /// <summary>节点的键</summary>
            public TKey Key { get; }
            /// <summary>节点的优先度</summary>
            public TPriority Priority { get; }
            /// <summary>表示将此节点放在哪些节点之后</summary>
            public IEnumerable<TKey>? After { get; }
            /// <summary>表示将此节点放在哪些节点之前</summary>
            public IEnumerable<TKey>? Before { get; }
            /// <summary>表示将哪些节点放在此节点之后</summary>
            public IEnumerable<TKey>? NodesAfter { get; }
            /// <summary>表示将哪些节点放在此节点之前</summary>
            public IEnumerable<TKey>? NodesBefore { get; }
        }
        #endregion

        public static IEnumerable<TNode> Sort<TNode>(IEnumerable<TNode> nodes) where TNode : INode => Default.SortInner(nodes);
        public static PriorityGraph<TKey, TPriority> Default { get; } = new();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IEnumerable<TNode> SortInner<TNode>(IEnumerable<TNode> nodes) where TNode : INode => AllowMultipleKey ? Sort_AllowMultipleKey(nodes) : Sort_DisallowMutipleKey(nodes);

        #region 设置
        public IEqualityComparer<TKey>? KeyComparer { get; set; }
        public IComparer<TPriority> PriorityComparer { get; set; } = Comparer<TPriority>.Default;
        /// <summary>
        /// 是否允许节点中的键重复
        /// 若允许, 那么重复键的节点将被合并
        /// </summary>
        public bool AllowMultipleKey { get; set; }
        /// <summary>
        /// <br/>当出现环时是否不报错
        /// <br/>若为 <see langword="true"/>, 则会以切掉环的一边的方式破坏环,
        /// <br/>且出现环时会设置 <see cref="AnyCircularReference"/> 为 true
        /// <br/>使用 <see cref="ClearAnyCircularReference"/> 以清除它
        /// </summary>
        public bool AllowCircle { get; set; }
        public bool AnyCircularReference { get; private set; }
        public void ClearAnyCircularReference() => AnyCircularReference = false;
        private void ThrowCircularReferenceException() {
            if (AllowCircle) {
                AnyCircularReference = true;
                return;
            }
            throw new CircularReferenceException();
        }
        #endregion
        #region DisallowMultiplyKey
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TNode> Sort_DisallowMutipleKey<TNode>(IEnumerable<TNode> nodes) where TNode : INode {
            List<NodeHolder<TNode>> result = [];
            Dictionary<TKey, NodeHolder<TNode>> dictionary = new(KeyComparer);
            int len = 0;
            // 录入
            foreach (var node in nodes) {
                var holder = NewNodeHolder<TNode>(len++, node);
                result.Add(holder);
                dictionary.Add(node.Key, holder);
            }
            // 注册前后继
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void Register(Dictionary<TKey, NodeHolder<TNode>> dictionary, IEnumerable<TKey>? keys, NodeHolder<TNode> self, List<NodeHolder<TNode>> toFeed) {
                if (keys == null) {
                    return;
                }
                foreach (var key in keys) {
                    if (!dictionary.TryGetValue(key, out var other) || other.Index == self.Index) {
                        continue;
                    }
                    toFeed.Add(other);
                }
            }
            foreach (var r in result) {
                var node = r.Node;
                Register(dictionary, node.After, r, r.After);
                Register(dictionary, node.Before, r, r.Before);
                Register(dictionary, node.NodesAfter, r, r.NodesAfter);
                Register(dictionary, node.NodesBefore, r, r.NodesBefore);
            }
            return SortSupperInner(result);
        }
        #endregion
        #region AllowMultiplyKey
        private class NodeHolderGroup<TNode> where TNode : INode {
            public List<NodeHolder<TNode>> Nodes { get; } = [];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TNode> Sort_AllowMultipleKey<TNode>(IEnumerable<TNode> nodes) where TNode : INode {
            List<NodeHolder<TNode>> result = [];
            Dictionary<TKey, NodeHolderGroup<TNode>> groups = new(KeyComparer);
            int len = 0;
            // 录入
            foreach (var node in nodes) {
                var holder = NewNodeHolder<TNode>(len++, node);
                var group = groups.GetOrAdd(node.Key, static () => new());
                group.Nodes.Add(holder);
                result.Add(holder);
            }
            // 注册前后继
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void Register(Dictionary<TKey, NodeHolderGroup<TNode>> groups, IEnumerable<TKey>? keys, NodeHolderGroup<TNode> self, List<NodeHolder<TNode>> toFeed) {
                if (keys == null) {
                    return;
                }
                foreach (var key in keys) {
                    if (!groups.TryGetValue(key, out var other) || other == self) {
                        continue;
                    }
                    foreach (var n in other.Nodes) {
                        toFeed.Add(n);
                    }
                }
            }
            foreach (var r in result) {
                var node = r.Node;
                var group = groups[node.Key];
                Register(groups, node.After, group, r.After);
                Register(groups, node.Before, group, r.Before);
                Register(groups, node.NodesAfter, group, r.NodesAfter);
                Register(groups, node.NodesBefore, group, r.NodesBefore);
            }
            return SortSupperInner(result);
        }
        #endregion
        /// <param name="After">它要放在哪些节点之后</param>
        /// <param name="Before">它要放在哪些节点之前</param>
        /// <param name="NodesAfter">放在它之后的节点</param>
        /// <param name="NodesBefore">放在它之前的节点</param>
        /// <param name="AllNodesAfter">经排序后它之后的节点</param>
        /// <param name="AllNodesBefore">经排序后它之前的节点</param>
        private record NodeHolder<TNode>(int Index, TNode Node,
            List<NodeHolder<TNode>> After, List<NodeHolder<TNode>> Before,
            List<NodeHolder<TNode>> NodesAfter, List<NodeHolder<TNode>> NodesBefore,
            HashSet<NodeHolder<TNode>> AllNodesAfter, HashSet<NodeHolder<TNode>> AllNodesBefore) where TNode : INode {
            public override int GetHashCode() => Index;
        }
        private int CompareHolder<TNode>(NodeHolder<TNode> x, NodeHolder<TNode> y) where TNode : INode {
            var p = PriorityComparer.Compare(x.Node.Priority, y.Node.Priority);
            return p != 0 ? p : x.Index.CompareTo(y.Index);
        }
        private static NodeHolder<TNode> NewNodeHolder<TNode>(int index, TNode node) where TNode : INode => new(index, node, [], [], [], [], [], []);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TNode> SortSupperInner<TNode>(List<NodeHolder<TNode>> nodes) where TNode : INode {
            // 先粗略按照 priority 排序, 以 index 为第二键确保稳定性
            nodes.Sort(CompareHolder);
            NodeHolder<TNode>[] array = [.. nodes];
            int len = nodes.Count;
            for (int i = 0; i < len; ++i) {
                var node = array[i];
                // 按照 After, NodesBefore, Before, NodesAfter 的顺序
                if (node.After.Count != 0) {
                    foreach (var a in node.After) {
                        AddNodesBefore(node, a);
                    }
                    MoveAfter(node, nodes);
                }
                if (node.NodesBefore.Count != 0) {
                    foreach (var nb in node.NodesBefore) {
                        AddNodesBefore(node, nb);
                    }
                    MoveBefore(nodes, node);
                }
                if (node.Before.Count != 0) {
                    foreach (var b in node.Before) {
                        AddNodesAfter(node, b);
                    }
                    MoveBefore(node, nodes);
                }
                if (node.NodesAfter.Count != 0) {
                    foreach (var na in node.NodesAfter) {
                        AddNodesAfter(node, na);
                    }
                    MoveAfter(nodes, node);
                }
            }
            return nodes.Select(static n => n.Node);
        }

        private void AddNodesBefore<TNode>(NodeHolder<TNode> self, NodeHolder<TNode> nodeBefore) where TNode : INode {
            if (self.AllNodesAfter.Contains(nodeBefore)) {
                ThrowCircularReferenceException();
                return;
            }
            if (!AddEdge(nodeBefore, self)) {
                return;
            }
            foreach (var b in nodeBefore.AllNodesBefore) {
                if (self.Index == b.Index) {
                    ThrowCircularReferenceException();
                    continue;
                }
                AddNodesBefore(self, b);
            }
        }
        private void AddNodesAfter<TNode>(NodeHolder<TNode> self, NodeHolder<TNode> nodeAfter) where TNode : INode {
            if (self.AllNodesBefore.Contains(nodeAfter)) {
                ThrowCircularReferenceException();
                return;
            }
            if (!AddEdge(self, nodeAfter)) {
                return;
            }
            foreach (var a in nodeAfter.AllNodesAfter) {
                AddNodesAfter(self, a);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AddEdge<TNode>(NodeHolder<TNode> from, NodeHolder<TNode> to) where TNode : INode {
            if (!AddEdgeSimple(from, to)) {
                return false;
            }
            foreach (var beforeFrom in from.AllNodesBefore) {
                if (AddEdgeSimple(beforeFrom, to)) {
                    foreach (var afterTo in to.AllNodesAfter) {
                        AddEdgeSimple(beforeFrom, afterTo);
                    }
                }
            }
            foreach (var afterTo in to.AllNodesAfter) {
                if (AddEdgeSimple(from, afterTo)) {
                    foreach (var beforeFrom in from.AllNodesBefore) {
                        AddEdgeSimple(beforeFrom, afterTo);
                    }
                }
            }
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AddEdgeSimple<TNode>(NodeHolder<TNode> from, NodeHolder<TNode> to) where TNode : INode {
            if (!from.AllNodesAfter.Add(to)) {
                return false;
            }
            to.AllNodesBefore.Add(from);
            return true;
        }

        /// <summary>
        /// 将 <paramref name="self"/> 的 AllNodesBefore 放在自己的前面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveBefore<TNode>(List<NodeHolder<TNode>> nodes, NodeHolder<TNode> self) where TNode : INode {
            #region 找到自己的 Index
            int index = 0;
            int len = nodes.Count;
            for (; index < len; ++index) {
                if (nodes[index].Index == self.Index) {
                    break;
                }
            }
            if (index == len) {
                throw new Exception("self not in nodes!");
            }
            #endregion
            List<NodeHolder<TNode>> toMove = [];
            for (int i = len - 1; i >= index; --i) {
                var node = nodes[i];
                if (self.AllNodesBefore.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                if (toMove.Count != 0) {
                    nodes[i + toMove.Count] = node;
                }
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index + toMove.Count - 1 - i] = toMove[i];
            }
        }
        /// <summary>
        /// 将 <paramref name="self"/> 的 AllNodesAfter 放在自己的后面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveAfter<TNode>(List<NodeHolder<TNode>> nodes, NodeHolder<TNode> self) where TNode : INode {
            #region 找到自己的 Index
            int index = 0;
            int len = nodes.Count;
            for (; index < len; ++index) {
                if (nodes[index].Index == self.Index) {
                    break;
                }
            }
            if (index == len) {
                throw new Exception("self not in nodes!");
            }
            #endregion
            List<NodeHolder<TNode>> toMove = [];
            for (int i = 0; i <= index; ++i) {
                var node = nodes[i];
                if (self.AllNodesAfter.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                if (toMove.Count != 0) {
                    nodes[i - toMove.Count] = node;
                }
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index - toMove.Count + 1 + i] = toMove[i];
            }
        }
        /// <summary>
        /// 将 <paramref name="self"/> 以及 AllNodesBefore 放在 AllNodesAfter 的前面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveBefore<TNode>(NodeHolder<TNode> self, List<NodeHolder<TNode>> nodes) where TNode : INode {
            int index = 0, firstIndex = -1;
            int len = nodes.Count;
            for (; index < len; ++index) {
                var node = nodes[index];
                if (node.Index == self.Index) {
                    break;
                }
                if (firstIndex == -1 && self.AllNodesAfter.Contains(node)) {
                    firstIndex = index;
                }
            }
            if (index == len) {
                throw new Exception("self not in nodes!");
            }
            if (firstIndex == -1) {
                return;
            }
            List<NodeHolder<TNode>> toMove = [self];
            for (--index; index >= firstIndex; --index) {
                var node = nodes[index];
                if (self.AllNodesBefore.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                nodes[index + toMove.Count] = node;
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index + toMove.Count - i] = toMove[i];
            }
        }
        /// <summary>
        /// 将 <paramref name="self"/> 放在 AllNodesBefore 的后面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveAfter<TNode>(NodeHolder<TNode> self, List<NodeHolder<TNode>> nodes) where TNode : INode {
            int len = nodes.Count;
            int index = len - 1, lastIndex = -1;
            for (; index >= 0; --index) {
                var node = nodes[index];
                if (node.Index == self.Index) {
                    break;
                }
                if (lastIndex == -1 && self.AllNodesBefore.Contains(node)) {
                    lastIndex = index;
                }
            }
            if (index == -1) {
                throw new Exception("self not in nodes!");
            }
            if (lastIndex == -1) {
                return;
            }
            List<NodeHolder<TNode>> toMove = [self];
            for (++index; index <= lastIndex; ++index) {
                var node = nodes[index];
                if (self.AllNodesAfter.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                nodes[index - toMove.Count] = node;
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index - toMove.Count + i] = toMove[i];
            }
        }
    }
    #region Node
    /// <summary>
    /// 节点
    /// </summary>
    /// <param name="Key">节点的键</param>
    /// <param name="After">表示将此节点放在哪些节点之后</param>
    /// <param name="Before">表示将此节点放在哪些节点之前</param>
    /// <param name="NodesAfter">表示将哪些节点放在此节点之后</param>
    /// <param name="NodesBefore">表示将哪些节点放在此节点之前</param>
    public record PriorityGraphPNode<TKey>(TKey Key,
        IEnumerable<TKey>? After = null,
        IEnumerable<TKey>? Before = null,
        IEnumerable<TKey>? NodesAfter = null,
        IEnumerable<TKey>? NodesBefore = null) : IPriorityGraphPNode<TKey> where TKey : notnull;
    public interface IPriorityGraphPNode<TKey> where TKey : notnull {
        /// <summary>节点的键</summary>
        public TKey Key { get; }
        /// <summary>表示将此节点放在哪些节点之后</summary>
        public IEnumerable<TKey>? After { get; }
        /// <summary>表示将此节点放在哪些节点之前</summary>
        public IEnumerable<TKey>? Before { get; }
        /// <summary>表示将哪些节点放在此节点之后</summary>
        public IEnumerable<TKey>? NodesAfter { get; }
        /// <summary>表示将哪些节点放在此节点之前</summary>
        public IEnumerable<TKey>? NodesBefore { get; }
    }
    #endregion
    public class PriorityGraphP<TKey, TPriority>(Func<TKey, TPriority> prioritySelector) where TKey : notnull {
        public static IEnumerable<TNode> Sort<TNode>(IEnumerable<TNode> nodes, Func<TKey, TPriority> prioritySelector) where TNode : IPriorityGraphPNode<TKey> => new PriorityGraphP<TKey, TPriority>(prioritySelector).SortInner(nodes);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IEnumerable<TNode> SortInner<TNode>(IEnumerable<TNode> nodes) where TNode : IPriorityGraphPNode<TKey> => AllowMultipleKey ? Sort_AllowMultipleKey(nodes) : Sort_DisallowMutipleKey(nodes);

        #region 设置
        public IEqualityComparer<TKey>? KeyComparer { get; set; }
        public Func<TKey, TPriority> PrioritySelector { get; set; } = prioritySelector;
        public IComparer<TPriority> PriorityComparer { get; set; } = Comparer<TPriority>.Default;
        /// <summary>
        /// 是否允许节点中的键重复
        /// 若允许, 那么重复键的节点将被合并
        /// </summary>
        public bool AllowMultipleKey { get; set; }
        /// <summary>
        /// <br/>当出现环时是否不报错
        /// <br/>若为 <see langword="true"/>, 则会以切掉环的一边的方式破坏环,
        /// <br/>且出现环时会设置 <see cref="AnyCircularReference"/> 为 true
        /// <br/>使用 <see cref="ClearAnyCircularReference"/> 以清除它
        /// </summary>
        public bool AllowCircle { get; set; }
        public bool AnyCircularReference { get; private set; }
        public void ClearAnyCircularReference() => AnyCircularReference = false;
        private void ThrowCircularReferenceException() {
            if (AllowCircle) {
                AnyCircularReference = true;
                return;
            }
            throw new CircularReferenceException();
        }
        #endregion
        #region DisallowMultiplyKey
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TNode> Sort_DisallowMutipleKey<TNode>(IEnumerable<TNode> nodes) where TNode : IPriorityGraphPNode<TKey> {
            List<NodeHolder<TNode>> result = [];
            Dictionary<TKey, NodeHolder<TNode>> dictionary = new(KeyComparer);
            int len = 0;
            // 录入
            foreach (var node in nodes) {
                var holder = NewNodeHolder<TNode>(len++, node);
                result.Add(holder);
                dictionary.Add(node.Key, holder);
            }
            // 注册前后继
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void Register(Dictionary<TKey, NodeHolder<TNode>> dictionary, IEnumerable<TKey>? keys, NodeHolder<TNode> self, List<NodeHolder<TNode>> toFeed) {
                if (keys == null) {
                    return;
                }
                foreach (var key in keys) {
                    if (!dictionary.TryGetValue(key, out var other) || other.Index == self.Index) {
                        continue;
                    }
                    toFeed.Add(other);
                }
            }
            foreach (var r in result) {
                var node = r.Node;
                Register(dictionary, node.After, r, r.After);
                Register(dictionary, node.Before, r, r.Before);
                Register(dictionary, node.NodesAfter, r, r.NodesAfter);
                Register(dictionary, node.NodesBefore, r, r.NodesBefore);
            }
            return SortSupperInner(result);
        }
        #endregion
        #region AllowMultiplyKey
        private class NodeHolderGroup<TNode> where TNode : IPriorityGraphPNode<TKey> {
            public List<NodeHolder<TNode>> Nodes { get; } = [];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TNode> Sort_AllowMultipleKey<TNode>(IEnumerable<TNode> nodes) where TNode : IPriorityGraphPNode<TKey> {
            List<NodeHolder<TNode>> result = [];
            Dictionary<TKey, NodeHolderGroup<TNode>> groups = new(KeyComparer);
            int len = 0;
            // 录入
            foreach (var node in nodes) {
                var holder = NewNodeHolder<TNode>(len++, node);
                var group = groups.GetOrAdd(node.Key, static () => new());
                group.Nodes.Add(holder);
                result.Add(holder);
            }
            // 注册前后继
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void Register(Dictionary<TKey, NodeHolderGroup<TNode>> groups, IEnumerable<TKey>? keys, NodeHolderGroup<TNode> self, List<NodeHolder<TNode>> toFeed) {
                if (keys == null) {
                    return;
                }
                foreach (var key in keys) {
                    if (!groups.TryGetValue(key, out var other) || other == self) {
                        continue;
                    }
                    foreach (var n in other.Nodes) {
                        toFeed.Add(n);
                    }
                }
            }
            foreach (var r in result) {
                var node = r.Node;
                var group = groups[node.Key];
                Register(groups, node.After, group, r.After);
                Register(groups, node.Before, group, r.Before);
                Register(groups, node.NodesAfter, group, r.NodesAfter);
                Register(groups, node.NodesBefore, group, r.NodesBefore);
            }
            return SortSupperInner(result);
        }
        #endregion
        /// <param name="After">它要放在哪些节点之后</param>
        /// <param name="Before">它要放在哪些节点之前</param>
        /// <param name="NodesAfter">放在它之后的节点</param>
        /// <param name="NodesBefore">放在它之前的节点</param>
        /// <param name="AllNodesAfter">经排序后它之后的节点</param>
        /// <param name="AllNodesBefore">经排序后它之前的节点</param>
        private record NodeHolder<TNode>(int Index, TNode Node, TPriority Priority,
            List<NodeHolder<TNode>> After, List<NodeHolder<TNode>> Before,
            List<NodeHolder<TNode>> NodesAfter, List<NodeHolder<TNode>> NodesBefore,
            HashSet<NodeHolder<TNode>> AllNodesAfter, HashSet<NodeHolder<TNode>> AllNodesBefore) where TNode : IPriorityGraphPNode<TKey> {
            public override int GetHashCode() => Index;
        }
        private int CompareHolder<TNode>(NodeHolder<TNode> x, NodeHolder<TNode> y) where TNode : IPriorityGraphPNode<TKey> {
            var p = PriorityComparer.Compare(x.Priority, y.Priority);
            return p != 0 ? p : x.Index.CompareTo(y.Index);
        }
        private NodeHolder<TNode> NewNodeHolder<TNode>(int index, TNode node) where TNode : IPriorityGraphPNode<TKey> => new(index, node, PrioritySelector(node.Key), [], [], [], [], [], []);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TNode> SortSupperInner<TNode>(List<NodeHolder<TNode>> nodes) where TNode : IPriorityGraphPNode<TKey> {
            // 先粗略按照 priority 排序, 以 index 为第二键确保稳定性
            nodes.Sort(CompareHolder);
            NodeHolder<TNode>[] array = [.. nodes];
            int len = nodes.Count;
            for (int i = 0; i < len; ++i) {
                var node = array[i];
                // 按照 After, NodesBefore, Before, NodesAfter 的顺序
                if (node.After.Count != 0) {
                    foreach (var a in node.After) {
                        AddNodesBefore(node, a);
                    }
                    MoveAfter(node, nodes);
                }
                if (node.NodesBefore.Count != 0) {
                    foreach (var nb in node.NodesBefore) {
                        AddNodesBefore(node, nb);
                    }
                    MoveBefore(nodes, node);
                }
                if (node.Before.Count != 0) {
                    foreach (var b in node.Before) {
                        AddNodesAfter(node, b);
                    }
                    MoveBefore(node, nodes);
                }
                if (node.NodesAfter.Count != 0) {
                    foreach (var na in node.NodesAfter) {
                        AddNodesAfter(node, na);
                    }
                    MoveAfter(nodes, node);
                }
            }
            return nodes.Select(static n => n.Node);
        }

        private void AddNodesBefore<TNode>(NodeHolder<TNode> self, NodeHolder<TNode> nodeBefore) where TNode : IPriorityGraphPNode<TKey> {
            if (self.AllNodesAfter.Contains(nodeBefore)) {
                ThrowCircularReferenceException();
                return;
            }
            if (!AddEdge(nodeBefore, self)) {
                return;
            }
            foreach (var b in nodeBefore.AllNodesBefore) {
                if (self.Index == b.Index) {
                    ThrowCircularReferenceException();
                    continue;
                }
                AddNodesBefore(self, b);
            }
        }
        private void AddNodesAfter<TNode>(NodeHolder<TNode> self, NodeHolder<TNode> nodeAfter) where TNode : IPriorityGraphPNode<TKey> {
            if (self.AllNodesBefore.Contains(nodeAfter)) {
                ThrowCircularReferenceException();
                return;
            }
            if (!AddEdge(self, nodeAfter)) {
                return;
            }
            foreach (var a in nodeAfter.AllNodesAfter) {
                AddNodesAfter(self, a);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AddEdge<TNode>(NodeHolder<TNode> from, NodeHolder<TNode> to) where TNode : IPriorityGraphPNode<TKey> {
            if (!AddEdgeSimple(from, to)) {
                return false;
            }
            foreach (var beforeFrom in from.AllNodesBefore) {
                if (AddEdgeSimple(beforeFrom, to)) {
                    foreach (var afterTo in to.AllNodesAfter) {
                        AddEdgeSimple(beforeFrom, afterTo);
                    }
                }
            }
            foreach (var afterTo in to.AllNodesAfter) {
                if (AddEdgeSimple(from, afterTo)) {
                    foreach (var beforeFrom in from.AllNodesBefore) {
                        AddEdgeSimple(beforeFrom, afterTo);
                    }
                }
            }
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AddEdgeSimple<TNode>(NodeHolder<TNode> from, NodeHolder<TNode> to) where TNode : IPriorityGraphPNode<TKey> {
            if (!from.AllNodesAfter.Add(to)) {
                return false;
            }
            to.AllNodesBefore.Add(from);
            return true;
        }

        /// <summary>
        /// 将 <paramref name="self"/> 的 AllNodesBefore 放在自己的前面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveBefore<TNode>(List<NodeHolder<TNode>> nodes, NodeHolder<TNode> self) where TNode : IPriorityGraphPNode<TKey> {
            #region 找到自己的 Index
            int index = 0;
            int len = nodes.Count;
            for (; index < len; ++index) {
                if (nodes[index].Index == self.Index) {
                    break;
                }
            }
            if (index == len) {
                throw new Exception("self not in nodes!");
            }
            #endregion
            List<NodeHolder<TNode>> toMove = [];
            for (int i = len - 1; i >= index; --i) {
                var node = nodes[i];
                if (self.AllNodesBefore.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                if (toMove.Count != 0) {
                    nodes[i + toMove.Count] = node;
                }
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index + toMove.Count - 1 - i] = toMove[i];
            }
        }
        /// <summary>
        /// 将 <paramref name="self"/> 的 AllNodesAfter 放在自己的后面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveAfter<TNode>(List<NodeHolder<TNode>> nodes, NodeHolder<TNode> self) where TNode : IPriorityGraphPNode<TKey> {
            #region 找到自己的 Index
            int index = 0;
            int len = nodes.Count;
            for (; index < len; ++index) {
                if (nodes[index].Index == self.Index) {
                    break;
                }
            }
            if (index == len) {
                throw new Exception("self not in nodes!");
            }
            #endregion
            List<NodeHolder<TNode>> toMove = [];
            for (int i = 0; i <= index; ++i) {
                var node = nodes[i];
                if (self.AllNodesAfter.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                if (toMove.Count != 0) {
                    nodes[i - toMove.Count] = node;
                }
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index - toMove.Count + 1 + i] = toMove[i];
            }
        }
        /// <summary>
        /// 将 <paramref name="self"/> 以及 AllNodesBefore 放在 AllNodesAfter 的前面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveBefore<TNode>(NodeHolder<TNode> self, List<NodeHolder<TNode>> nodes) where TNode : IPriorityGraphPNode<TKey> {
            int index = 0, firstIndex = -1;
            int len = nodes.Count;
            for (; index < len; ++index) {
                var node = nodes[index];
                if (node.Index == self.Index) {
                    break;
                }
                if (firstIndex == -1 && self.AllNodesAfter.Contains(node)) {
                    firstIndex = index;
                }
            }
            if (index == len) {
                throw new Exception("self not in nodes!");
            }
            if (firstIndex == -1) {
                return;
            }
            List<NodeHolder<TNode>> toMove = [self];
            for (--index; index >= firstIndex; --index) {
                var node = nodes[index];
                if (self.AllNodesBefore.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                nodes[index + toMove.Count] = node;
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index + toMove.Count - i] = toMove[i];
            }
        }
        /// <summary>
        /// 将 <paramref name="self"/> 放在 AllNodesBefore 的后面
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveAfter<TNode>(NodeHolder<TNode> self, List<NodeHolder<TNode>> nodes) where TNode : IPriorityGraphPNode<TKey> {
            int len = nodes.Count;
            int index = len - 1, lastIndex = -1;
            for (; index >= 0; --index) {
                var node = nodes[index];
                if (node.Index == self.Index) {
                    break;
                }
                if (lastIndex == -1 && self.AllNodesBefore.Contains(node)) {
                    lastIndex = index;
                }
            }
            if (index == -1) {
                throw new Exception("self not in nodes!");
            }
            if (lastIndex == -1) {
                return;
            }
            List<NodeHolder<TNode>> toMove = [self];
            for (++index; index <= lastIndex; ++index) {
                var node = nodes[index];
                if (self.AllNodesAfter.Contains(node)) {
                    toMove.Add(node);
                    continue;
                }
                nodes[index - toMove.Count] = node;
            }
            for (int i = 0; i < toMove.Count; ++i) {
                nodes[index - toMove.Count + i] = toMove[i];
            }
        }
    }

    public class CircularReferenceException : Exception {
        public CircularReferenceException() { }
        public CircularReferenceException(string message) : base(message) { }
        public CircularReferenceException(string message, Exception innerException) : base(message, innerException) { }
    }
}

partial class TigerExtensions {
    #region priorityGraph.Sort
    public static IEnumerable<TNode> Sort<TNode, TKey, TValue, TPriority>(
        this PriorityGraph<TKey, TValue, TPriority> graph, IEnumerable<TNode> nodes) where TNode : PriorityGraph<TKey, TValue, TPriority>.INode where TKey : notnull
        => graph.SortInner(nodes);
    public static IEnumerable<TNode> Sort<TNode, TKey, TValue, TPriority>(
        this PriorityGraphK<TKey, TValue, TPriority> graph, IEnumerable<TNode> nodes) where TNode : PriorityGraphK<TKey, TValue, TPriority>.INode where TKey : notnull
        => graph.SortInner(nodes);
    public static IEnumerable<TNode> Sort<TNode, TKey, TValue, TPriority>(
        this PriorityGraphP<TKey, TValue, TPriority> graph, IEnumerable<TNode> nodes) where TNode : IPriorityGraphPNode<TKey, TValue> where TKey : notnull
        => graph.SortInner(nodes);
    public static IEnumerable<TNode> Sort<TNode, TKey, TValue, TPriority>(
        this PriorityGraphS<TKey, TValue, TPriority> graph, IEnumerable<TNode> nodes) where TNode : IPriorityGraphSNode<TKey, TValue> where TKey : notnull
        => graph.SortInner(nodes);
    public static IEnumerable<TNode> Sort<TNode, TKey, TPriority>(
        this PriorityGraph<TKey, TPriority> graph, IEnumerable<TNode> nodes) where TNode : PriorityGraph<TKey, TPriority>.INode where TKey : notnull
        => graph.SortInner(nodes);
    public static IEnumerable<TNode> Sort<TNode, TKey, TPriority>(
        this PriorityGraphP<TKey, TPriority> graph, IEnumerable<TNode> nodes) where TNode : IPriorityGraphPNode<TKey> where TKey : notnull
        => graph.SortInner(nodes);
    #endregion
    #region IEnumerable<Node>.SortGraph
    public static IEnumerable<TNode> SortGraph<TNode, TKey, TValue, TPriority>(
        this IEnumerable<TNode> nodes, PriorityGraph<TKey, TValue, TPriority>? graph = null) where TNode : PriorityGraph<TKey, TValue, TPriority>.INode where TKey : notnull
        => (graph ?? PriorityGraph<TKey, TValue, TPriority>.Default).Sort(nodes);

    public static IEnumerable<TNode> SortGraph<TNode, TKey, TValue, TPriority>(
        this IEnumerable<TNode> nodes, PriorityGraphK<TKey, TValue, TPriority> graph) where TNode : PriorityGraphK<TKey, TValue, TPriority>.INode where TKey : notnull
        => graph.Sort(nodes);
    public static IEnumerable<TNode> SortGraph<TNode, TKey, TValue, TPriority>(
        this IEnumerable<TNode> nodes, Func<TValue, TKey> keySelector) where TNode : PriorityGraphK<TKey, TValue, TPriority>.INode where TKey : notnull
        => PriorityGraphK<TKey, TValue, TPriority>.Sort(nodes, keySelector);

    public static IEnumerable<TNode> SortGraph<TNode, TKey, TValue, TPriority>(
        this IEnumerable<TNode> nodes, PriorityGraphP<TKey, TValue, TPriority> graph) where TNode : IPriorityGraphPNode<TKey, TValue> where TKey : notnull
        => graph.Sort(nodes);
    public static IEnumerable<TNode> SortGraph<TNode, TKey, TValue, TPriority>(
        this IEnumerable<TNode> nodes, Func<TValue, TPriority> prioritySelector) where TNode : IPriorityGraphPNode<TKey, TValue> where TKey : notnull
        => PriorityGraphP<TKey, TValue, TPriority>.Sort(nodes, prioritySelector);

    public static IEnumerable<TNode> SortGraph<TNode, TKey, TValue, TPriority>(
        this IEnumerable<TNode> nodes, PriorityGraphS<TKey, TValue, TPriority> graph) where TNode : IPriorityGraphSNode<TKey, TValue> where TKey : notnull
        => graph.Sort(nodes);
    public static IEnumerable<TNode> SortGraph<TNode, TKey, TValue, TPriority>(
        this IEnumerable<TNode> nodes, Func<TValue, TKey> keySelector, Func<TValue, TPriority> prioritySelector) where TNode : IPriorityGraphSNode<TKey, TValue> where TKey : notnull
        => PriorityGraphS<TKey, TValue, TPriority>.Sort(nodes, keySelector, prioritySelector);

    public static IEnumerable<TNode> SortGraph<TNode, TKey, TPriority>(
        this IEnumerable<TNode> nodes, PriorityGraph<TKey, TPriority>? graph = null) where TNode : PriorityGraph<TKey, TPriority>.INode where TKey : notnull
        => (graph ?? PriorityGraph<TKey, TPriority>.Default).Sort(nodes);

    public static IEnumerable<TNode> SortGraph<TNode, TKey, TPriority>(
        this IEnumerable<TNode> nodes, PriorityGraphP<TKey, TPriority> graph) where TNode : IPriorityGraphPNode<TKey> where TKey : notnull
        => graph.Sort(nodes);
    public static IEnumerable<TNode> SortGraph<TNode, TKey, TPriority>(
        this IEnumerable<TNode> nodes, Func<TKey, TPriority> prioritySelector) where TNode : IPriorityGraphPNode<TKey> where TKey : notnull
        => PriorityGraphP<TKey, TPriority>.Sort(nodes, prioritySelector);
    #endregion
}
