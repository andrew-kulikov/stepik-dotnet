using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inheritance.Geometry.Visitor
{
    //public interface IVisitor<TVisited, TResult> : IVisitor where TVisited : IVisitable
    //{
    //    TResult Visit(TVisited visited);
    //}

    //public interface IVisitor
    //{
    //    TResult Visit<TResult>(IVisitable visited);
    //}

    //public interface IVisitable
    //{
    //    TResult Accept<TResult>(IVisitor visitor);
    //}

    //public abstract class Visitor : IVisitor
    //{
    //    private Dictionary<string, object> _typedVisitors = new Dictionary<string, object>();

    //    protected void AddTypedVisitor<TVisited, TResult>(IVisitor<TVisited, TResult> visitor) where TVisited : IVisitable
    //    {
    //        var key = $"{typeof(TVisited).FullName}_{typeof(TResult).FullName}";

    //        _typedVisitors[key] = visitor;
    //    }

    //    public TResult Visit<TResult>(IVisitable visited)
    //    {
    //        var key = $"{visited.GetType().FullName}_{typeof(TResult).FullName}";

    //        if (!_typedVisitors.TryGetValue(key, out var visitorObj)) return default;

    //        var genericVisitorType = typeof(IVisitor<,>).MakeGenericType(visited.GetType(), typeof(TResult));
    //        if (!genericVisitorType.IsInstanceOfType(visitorObj)) return default;

    //        var visitMethod = genericVisitorType.GetMethod("Visit");
    //        return (TResult)visitMethod?.Invoke(visitorObj, new object[] { });
    //    }
    //}
}
