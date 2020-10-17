using System;
using System.Collections;
using System.Collections.Generic;

namespace Generics.BinaryTrees
{
    public class BinaryTree<T> : IEnumerable<T> where T : IComparable<T>
    {
        private bool _initialized;
        private T _value;

        public T Value
        {
            get => _value;
            private set
            {
                _initialized = true;
                _value = value;
            }
        }

        public BinaryTree<T> Left { get; private set; }
        public BinaryTree<T> Right { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            if (!_initialized) yield break;

            if (Left != null)
                foreach (var leftItem in Left)
                    yield return leftItem;

            yield return Value;

            if (Right != null)
                foreach (var rightItem in Right)
                    yield return rightItem;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T value)
        {
            if (!_initialized)
            {
                Value = value;
                return;
            }

            if (value.CompareTo(Value) <= 0)
            {
                if (Left == null) Left = new BinaryTree<T>();
                Left.Add(value);
            }
            else
            {
                if (Right == null) Right = new BinaryTree<T>();
                Right.Add(value);
            }
        }
    }

    public class BinaryTree
    {
        public static BinaryTree<T> Create<T>(params T[] items) where T : IComparable<T>
        {
            var tree = new BinaryTree<T>();

            foreach (var item in items) tree.Add(item);

            return tree;
        }
    }
}