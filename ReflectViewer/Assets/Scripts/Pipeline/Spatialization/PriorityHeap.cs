using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Reflect.Viewer.Pipeline
{
    public class PriorityHeap<T>
    {
        readonly Comparer<T> m_Comparer;
        readonly List<T> m_Heap;
        T m_Swap;

        public int count => m_Heap.Count;
        public bool isEmpty => m_Heap.Count == 0;

        public PriorityHeap(int capacity = 16, Comparer<T> comparer = null)
        {
            m_Comparer = comparer ?? Comparer<T>.Default;
            m_Heap = new List<T>(capacity);
        }

        public void Push(T obj)
        {
            m_Heap.Add(obj);
            HeapifyUp();
        }

        public bool TryPeek(out T value)
        {
            if (isEmpty)
            {
                value = default;
                return false;
            }

            value = m_Heap[0];
            return true;
        }

        public bool TryPop(out T value)
        {
            if (isEmpty)
            {
                value = default;
                return false;
            }

            value = m_Heap[0];
            var last = m_Heap.Count - 1;
            m_Heap[0] = m_Heap[last];
            m_Heap.RemoveAt(last);
            if (m_Heap.Count > 1)
                HeapifyDown();
            return true;
        }

        public void Clear()
        {
            m_Heap.Clear();
        }

        static int GetParent(int index) { return (index - 1) / 2; }
        static int GetLeft(int index) { return index * 2 + 1; }
        static int GetRight(int index) { return index * 2 + 2; }

        void HeapifyUp()
        {
            var index = m_Heap.Count - 1;

            while (index > 0)
            {
                var parent = GetParent(index);
                if (Compare(parent, index) <= 0)
                    return;

                Swap(parent, index);
                index = parent;
            }
        }

        void HeapifyDown()
        {
            var index = 0;

            while (index < m_Heap.Count)
            {
                var left = GetLeft(index);
                var right = GetRight(index);
                var best = index;
                if (left < m_Heap.Count && Compare(best, left) > 0)
                    best = left;
                if (right < m_Heap.Count && Compare(best, right) > 0)
                    best = right;

                if (best == index)
                    return;

                Swap(best, index);
                index = best;
            }
        }

        int Compare(int a, int b)
        {
            return m_Comparer.Compare(m_Heap[a], m_Heap[b]);
        }

        void Swap(int a, int b)
        {
            m_Swap = m_Heap[a];
            m_Heap[a] = m_Heap[b];
            m_Heap[b] = m_Swap;
        }
    }
}
