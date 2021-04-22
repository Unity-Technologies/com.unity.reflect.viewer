using System;
using System.Collections.Generic;

namespace UnityEngine.Reflect.Viewer.Pipeline
{
	public sealed class RTree : ISpatialCollection<ISpatialObject>
	{
		internal sealed class SpatialNode : ISpatialObject
		{
			public Vector3 min { get; private set; }
			public Vector3 max { get; private set; }
			public Vector3 center { get; private set; }
			public float priority { get; set; }
			public bool isVisible { get; set; }
			public GameObject loadedObject { get; set; }

			public bool isLeaf => children.Count == 0 || !(children[0] is SpatialNode);

			public SpatialNode parent;
			public readonly List<ISpatialObject> children;

			public SpatialNode()
			{
				min = Vector3.positiveInfinity;
				max = Vector3.negativeInfinity;

				parent = null;
				children = new List<ISpatialObject>();
			}

			public void AdjustBounds()
			{
				if (children.Count == 0)
					return;

				CalculateMinMax(children, out var newMin, out var newMax);

				SetMinMax(newMin, newMax);
			}

			public bool Encapsulates(ISpatialObject obj)
			{
				return min.x <= obj.min.x && max.x >= obj.max.x &&
				       min.y <= obj.min.y && max.y >= obj.max.y &&
				       min.z <= obj.min.z && max.z >= obj.max.z;
			}

			public SpatialNode FindChildLeastEnlargement(ISpatialObject obj)
			{
				SpatialNode bestChild = null;
				var leastEnlargement = float.MaxValue;
				var leastVolume = float.MaxValue;
				foreach (var child in children)
				{
					var volume = CalculateVolume(child.min, child.max);
					var newVolume = CalculateVolume(Vector3.Min(child.min, obj.min),
						Vector3.Max(child.max, obj.max));
					var enlargement = newVolume - volume;
					if (enlargement > leastEnlargement ||
					    Mathf.Approximately(enlargement, leastEnlargement) && volume >= leastVolume)
						continue;

					bestChild = child as SpatialNode;
					leastEnlargement = enlargement;
					leastVolume = volume;
				}

				return bestChild;
			}

			void SetMinMax(Vector3 newMin, Vector3 newMax)
			{
				min = newMin;
				max = newMax;
				center = newMin + (newMax - newMin) / 2f;
			}

			public void Dispose()
			{
				for (var i = 0; i < children.Count; ++i)
					children[i].Dispose();

				children.Clear();
			}
		}

		const int k_X = 0b100;
		const int k_Y = 0b010;
		const int k_Z = 0b001;

		readonly int m_MaxPerNode;
		readonly int m_MinPerNode;

		SpatialNode m_RootNode;

		readonly PriorityHeap<ISpatialObject> m_PriorityHeap;
		readonly List<ISpatialObject>[] m_CornerSplitChildren;

		readonly Comparer<ISpatialObject> m_NodeComparerX;
		readonly Comparer<ISpatialObject> m_SplitNodeComparerX;
		readonly Comparer<ISpatialObject> m_NodeComparerY;
		readonly Comparer<ISpatialObject> m_SplitNodeComparerY;
		readonly Comparer<ISpatialObject> m_NodeComparerZ;
		readonly Comparer<ISpatialObject> m_SplitNodeComparerZ;

		readonly object m_Lock = new object();

		Bounds m_Bounds = new Bounds();

		public int objectCount { get; private set; }
		public int depth { get; private set; } = 1;

		public Bounds bounds
		{
			get
			{
				lock (m_Lock)
					m_Bounds.SetMinMax(m_RootNode.min, m_RootNode.max);
				return m_Bounds;
			}
		}

		public RTree(int minPerNode, int maxPerNode)
		{
			m_MinPerNode = minPerNode;
			m_MaxPerNode = maxPerNode;

			m_CornerSplitChildren = new List<ISpatialObject>[8];
			for (var i = 0; i < m_CornerSplitChildren.Length; ++i)
				m_CornerSplitChildren[i] = new List<ISpatialObject>();

			m_RootNode = new SpatialNode();
			m_PriorityHeap = new PriorityHeap<ISpatialObject>(m_MaxPerNode, Comparer<ISpatialObject>.Create((a, b) => a.priority.CompareTo(b.priority)));

			// split node comparison is reversed compared to existing node
			// that way children closest to the split axis are always at the end of the list
			m_NodeComparerX = Comparer<ISpatialObject>.Create((a, b) => a.center.x.CompareTo(b.center.x));
			m_SplitNodeComparerX = Comparer<ISpatialObject>.Create((a, b) => b.center.x.CompareTo(a.center.x));
			m_NodeComparerY = Comparer<ISpatialObject>.Create((a, b) => a.center.y.CompareTo(b.center.y));
			m_SplitNodeComparerY = Comparer<ISpatialObject>.Create((a, b) => b.center.y.CompareTo(a.center.y));
			m_NodeComparerZ = Comparer<ISpatialObject>.Create((a, b) => a.center.z.CompareTo(b.center.z));
			m_SplitNodeComparerZ = Comparer<ISpatialObject>.Create((a, b) => b.center.z.CompareTo(a.center.z));
		}

		public void Search<T>(Func<ISpatialObject, bool> predicate,
			Func<ISpatialObject, float> prioritizer,
			int maxResultsCount,
			ICollection<T> results) where T : ISpatialObject
		{
			lock (m_Lock)
			{
				var count = 0;
				m_PriorityHeap.Clear();
				results.Clear();
				m_PriorityHeap.Push(m_RootNode);

				while (count < maxResultsCount && !m_PriorityHeap.isEmpty)
				{
					if (!m_PriorityHeap.TryPop(out var obj))
						break;

					if (obj is SpatialNode node)
					{
						foreach (var child in node.children)
						{
							if (!predicate(child))
								continue;

							child.priority = prioritizer(child);
							m_PriorityHeap.Push(child);
						}

						continue;
					}

					results.Add((T)obj);
					++count;
				}
			}
		}

		public void Add(ISpatialObject obj)
		{
			lock (m_Lock)
			{
				Insert(obj);
				++objectCount;
			}
		}

		void Insert(ISpatialObject obj)
		{
			var leaf = ChooseLeaf(obj);
			if (leaf.children.Count < m_MaxPerNode)
			{
				leaf.children.Add(obj);
				AdjustTree(leaf);
			}
			else
			{
				SplitNode(leaf, obj, out var splitNode);
				AdjustTree(leaf, splitNode);
			}
		}

		SpatialNode ChooseLeaf(ISpatialObject obj)
		{
			var node = m_RootNode;
			while (node != null && !node.isLeaf)
			{
				node = node.FindChildLeastEnlargement(obj);
			}
			return node;
		}

		void SplitNode(SpatialNode node, ISpatialObject obj, out SpatialNode splitNode)
		{
			CornerBasedSplit(node, obj, out splitNode);
		}

		SpatialNode FindLeaf(ISpatialObject obj)
		{
			var node = m_RootNode;
			while (node != null && !node.isLeaf)
			{
				var validChildFound = false;
				foreach (var child in node.children)
				{
					if (!(child is SpatialNode childNode) || !childNode.Encapsulates(obj))
						continue;

					node = childNode;
					validChildFound = true;
					break;
				}

				if (!validChildFound)
					return null;
			}
			return node;
		}

		public void Remove(ISpatialObject obj)
		{
			lock (m_Lock)
			{
				if (!Delete(obj))
					return;

				--objectCount;
			}
		}

		bool Delete(ISpatialObject obj)
		{
			var node = FindLeaf(obj);

			if (node == null || !node.children.Remove(obj))
				return false;

			if (node.children.Count < m_MinPerNode)
				ReinsertAll(node);
			else
				AdjustTree(node);

			return true;
		}

		void AdjustTree(SpatialNode node, SpatialNode splitNode = null)
		{
			while (node != null)
			{
				node.AdjustBounds();

				if (node.parent == null)
				{
					if (splitNode == null)
						return;

					// we've reached the root node but it was split, create new root containing the split nodes
					m_RootNode = new SpatialNode();
					m_RootNode.children.Add(node);
					node.parent = m_RootNode;
					m_RootNode.children.Add(splitNode);
					splitNode.parent = m_RootNode;
					m_RootNode.AdjustBounds();
					++depth;
					return;
				}

				node = node.parent;

				// no split so keep propagating bounds adjustment upwards
				if (splitNode == null)
					continue;

				if (node.children.Count < m_MaxPerNode)
				{
					node.children.Add(splitNode);
					splitNode.parent = node;
					splitNode = null;
				}
				else
				{
					SplitNode(node, splitNode, out var newSplitNode);
					splitNode = newSplitNode;
				}
			}
		}

		void ReinsertAll(SpatialNode node)
		{
			if (!node.isLeaf)
				return;

			foreach (var obj in node.children)
				obj.priority = (node.center - obj.center).sqrMagnitude;

			node.children.Sort((a, b) => a.priority.CompareTo(b.priority));

			var objects = node.children;
			if (node.parent != null)
			{
				node.parent.children.Remove(node);
				AdjustTree(node.parent);
			}

			// Using a copy of the list to avoid modifications during the iteration
			var objectsCopy = new List<ISpatialObject>(objects);
			objects.Clear();

			foreach (var obj in objectsCopy)
				Insert(obj);
		}

		static void CalculateMinMax(List<ISpatialObject> objects, out Vector3 min, out Vector3 max)
		{
			min = Vector3.positiveInfinity;
			max = Vector3.negativeInfinity;

			if (objects.Count == 0)
				return;

			foreach (var obj in objects)
			{
				min = Vector3.Min(min, obj.min);
				max = Vector3.Max(max, obj.max);
			}
		}

		static float CalculateVolume(Vector3 min, Vector3 max)
		{
			return (max.x - min.x) * (max.y - min.y) * (max.z - min.z);
		}

		static float CalculateOverlap(Vector3 aMin, Vector3 aMax, Vector3 bMin, Vector3 bMax)
		{
			var dx = Mathf.Min(aMax.x, bMax.x) - Mathf.Max(aMin.x, bMin.x);
			if (dx <= 0f)
				return 0f;

			var dy = Mathf.Min(aMax.y, bMax.y) - Mathf.Max(aMin.y, bMin.y);
			if (dy <= 0f)
				return 0f;

			var dz = Mathf.Min(aMax.z, bMax.z) - Mathf.Max(aMin.z, bMin.z);
			if (dz <= 0f)
				return 0f;

			return dx * dy * dz;
		}

		void CornerBasedSplit(SpatialNode node, ISpatialObject obj, out SpatialNode splitNode)
		{
			foreach (var cornerChildren in m_CornerSplitChildren)
				cornerChildren.Clear();

			// separate children by their closest corners
			m_CornerSplitChildren[FindClosestCornerIndex(node, obj)].Add(obj);
			foreach (var child in node.children)
				m_CornerSplitChildren[FindClosestCornerIndex(node, child)].Add(child);

			// split children by closest corners along each axis
			var splitLowX = new List<ISpatialObject>();
			var splitHighX = new List<ISpatialObject>();
			var splitLowY = new List<ISpatialObject>();
			var splitHighY = new List<ISpatialObject>();
			var splitLowZ = new List<ISpatialObject>();
			var splitHighZ = new List<ISpatialObject>();
			for (var i = 0; i < m_CornerSplitChildren.Length; ++i)
			{
				((i >> 2) % 2 > 0 ? splitHighX : splitLowX).AddRange(m_CornerSplitChildren[i]);
				((i >> 1) % 2 > 0 ? splitHighY : splitLowY).AddRange(m_CornerSplitChildren[i]);
				(i % 2 > 0 ? splitHighZ : splitLowZ).AddRange(m_CornerSplitChildren[i]);
			}

			// find the diff between the split node child counts
			var dx = Mathf.Abs(splitLowX.Count - splitHighX.Count);
			var dy = Mathf.Abs(splitLowY.Count - splitHighY.Count);
			var dz = Mathf.Abs(splitLowZ.Count - splitHighZ.Count);

			var bestSplits = new List<int>();
			if (dx <= dy && dx <= dz)
				bestSplits.Add(k_X);
			if (dy <= dx && dy <= dz)
				bestSplits.Add(k_Y);
			if (dz <= dx && dz <= dy)
				bestSplits.Add(k_Z);

			// handle tiebreakers if multiple equal counts
			var bestSplit = 0;
			if (bestSplits.Count == 1)
				bestSplit = bestSplits[0];
			else
			{
				var overlaps = new List<float>();
				var volumes = new List<float>();
				foreach (var axis in bestSplits)
				{
					// tiebreaker uses overlaps and volume totals
					switch (axis)
					{
						case k_X:
							CalculateMinMax(splitLowX, out var minLowX, out var maxLowX);
							CalculateMinMax(splitHighX, out var minHighX, out var maxHighX);
							overlaps.Add(CalculateOverlap(minLowX, maxLowX, minHighX, maxHighX));
							volumes.Add(CalculateVolume(minLowX, maxLowX) + CalculateVolume(minHighX, maxHighX));
							break;
						case k_Y:
							CalculateMinMax(splitLowY, out var minLowY, out var maxLowY);
							CalculateMinMax(splitHighY, out var minHighY, out var maxHighY);
							overlaps.Add(CalculateOverlap(minLowY, maxLowY, minHighY, maxHighY));
							volumes.Add(CalculateVolume(minLowY, maxLowY) + CalculateVolume(minHighY, maxHighY));
							break;
						case k_Z:
							CalculateMinMax(splitLowZ, out var minLowZ, out var maxLowZ);
							CalculateMinMax(splitHighZ, out var minHighZ, out var maxHighZ);
							overlaps.Add(CalculateOverlap(minLowZ, maxLowZ, minHighZ, maxHighZ));
							volumes.Add(CalculateVolume(minLowZ, maxLowZ) + CalculateVolume(minHighZ, maxHighZ));
							break;
					}
				}

				var bestOverlap = float.MaxValue;
				var bestVolume = float.MaxValue;
				for (var i = 0; i < bestSplits.Count; ++i)
				{
					// check overlap first, if equal check total volume
					if (overlaps[i] > bestOverlap ||
					    Mathf.Approximately(overlaps[i], bestOverlap) && volumes[i] > bestVolume)
						continue;

					bestSplit = bestSplits[i];
					bestOverlap = overlaps[i];
					bestVolume = volumes[i];
				}
			}

			SpatialNode smallNode = null;
			SpatialNode bigNode = null;
			node.children.Clear();
			splitNode = new SpatialNode { parent = node.parent };

			var nodeComparer = Comparer<ISpatialObject>.Default;
			var splitNodeComparer = Comparer<ISpatialObject>.Default;
			switch (bestSplit)
			{
				case k_X:
					node.children.AddRange(splitLowX);
					nodeComparer = m_NodeComparerX;
					splitNode.children.AddRange(splitHighX);
					splitNodeComparer = m_SplitNodeComparerX;
					break;
				case k_Y:
					node.children.AddRange(splitLowY);
					nodeComparer = m_NodeComparerY;
					splitNode.children.AddRange(splitHighY);
					splitNodeComparer = m_SplitNodeComparerY;
					break;
				case k_Z:
					node.children.AddRange(splitLowZ);
					nodeComparer = m_NodeComparerZ;
					splitNode.children.AddRange(splitHighZ);
					splitNodeComparer = m_SplitNodeComparerZ;
					break;
			}

			// if a node has too few children, sort them by distance from split axis and get ready for next step
			if (node.children.Count < m_MinPerNode && nodeComparer != null)
			{
				node.children.Sort(nodeComparer);
				smallNode = node;
				bigNode = splitNode;
			}
			else if (splitNode.children.Count < m_MinPerNode && splitNodeComparer != null)
			{
				splitNode.children.Sort(splitNodeComparer);
				smallNode = splitNode;
				bigNode = node;
			}

			// move closest children from big node to small node until small node has the minimum amount required
			if (smallNode != null && bigNode != null)
			{
				var amount = m_MinPerNode - smallNode.children.Count;
				var index = bigNode.children.Count - amount;
				smallNode.children.AddRange(bigNode.children.GetRange(index, amount));
				bigNode.children.RemoveRange(index, amount);
			}

			node.AdjustBounds();
			splitNode.AdjustBounds();
			foreach (var child in splitNode.children)
			{
				if (child is SpatialNode childNode)
				{
					childNode.parent = splitNode;
				}
			}
		}

		static int FindClosestCornerIndex(SpatialNode node, ISpatialObject obj)
		{
			return (obj.center.x <= node.center.x ? 0 : k_X) +
			       (obj.center.y <= node.center.y ? 0 : k_Y) +
			       (obj.center.z <= node.center.z ? 0 : k_Z);
		}

		public void DrawDebug(Gradient nodeGradient, Gradient objectGradient, float maxPriority, int maxDepth)
		{
			lock (m_Lock)
			{
				var originalColor = Gizmos.color;
				DrawDebugRecursive(m_RootNode, nodeGradient, objectGradient, maxPriority, maxDepth);
				Gizmos.color = originalColor;
			}
		}

		void DrawDebugRecursive(ISpatialObject obj, Gradient nodeGradient, Gradient objectGradient, float maxPriority, int maxDepth, float currentDepth = 0f)
		{
			if (maxDepth < currentDepth)
				return;

			if (obj is SpatialNode node)
			{
				if (nodeGradient != null)
				{
					Gizmos.color = nodeGradient.Evaluate(currentDepth / depth);
					Gizmos.DrawWireCube(obj.center, obj.max - obj.min);
				}
				foreach (var child in node.children)
				{
					DrawDebugRecursive(child, nodeGradient, objectGradient, maxPriority, maxDepth, currentDepth + 1f);
				}
				return;
			}

			if (objectGradient == null)
				return;

			Gizmos.color = objectGradient.Evaluate(maxPriority > 0 ? Math.Abs(obj.priority / maxPriority) : 0);
			Gizmos.DrawCube(obj.center, obj.max - obj.min);
		}

		public void Dispose()
		{
			lock (m_Lock)
				m_RootNode.Dispose();
		}
	}
}
