using System;
using System.Collections.Generic;
using System.Linq;

namespace Delegates.TreeTraversal
{
    public class Traverser<TTree, TOut> where TTree : class
    {
        private readonly Func<TTree, IEnumerable<TTree>> _next;
        private readonly Func<TTree, IEnumerable<TOut>> _valuesSelector;

        public Traverser(Func<TTree, IEnumerable<TTree>> next, Func<TTree, IEnumerable<TOut>> valuesSelector)
        {
            _next = next;
            _valuesSelector = valuesSelector;
        }

        public IEnumerable<TOut> Traverse(TTree tree)
        {
            if (tree == null) return new List<TOut>();

            var values = _valuesSelector(tree).ToList();
            var childrenValues = _next(tree).SelectMany(Traverse);

            return childrenValues.Union(values);
        }
    }

    public static class Traversal
    {
        public static IEnumerable<Product> GetProducts(ProductCategory root)
        {
            var traverser = new Traverser<ProductCategory, Product>(
                pc => pc.Categories,
                pc => pc.Products);

            return traverser.Traverse(root);
        }

        public static IEnumerable<Job> GetEndJobs(Job root)
        {
            var traverser = new Traverser<Job, Job>(
                job => job.Subjobs,
                job => job.Subjobs.Any() ? new Job[0] : new[] {job});

            return traverser.Traverse(root);
        }

        public static IEnumerable<T> GetBinaryTreeValues<T>(BinaryTree<T> root)
        {
            var traverser = new Traverser<BinaryTree<T>, T>(
                tree => new[] {tree.Left, tree.Right},
                tree => tree.Right == null && tree.Left == null ? new[] {tree.Value} : new T[0]);

            return traverser.Traverse(root);
        }
    }
}