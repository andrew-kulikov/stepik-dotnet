
using System;
using System.Collections.Generic;

namespace Inheritance.Geometry.Visitor
{
    public abstract class Body: IVisitable
    {
        public Vector3 Position { get; }

        protected Body(Vector3 position)
        {
            Position = position;
        }

        public TResult Accept<TResult>(IVisitor visitor)
        {
            return visitor.Visit<TResult>(this);
        }
    }

    public class Ball : Body
    {
        public double Radius { get; }

        public Ball(Vector3 position, double radius) : base(position)
        {
            Radius = radius;
        }
    }

    public class RectangularCuboid : Body
    {
        public double SizeX { get; }
        public double SizeY { get; }
        public double SizeZ { get; }

        public RectangularCuboid(Vector3 position, double sizeX, double sizeY, double sizeZ) : base(position)
        {
            SizeX = sizeX;
            SizeY = sizeY;
            SizeZ = sizeZ;
        }
    }

    public class Cylinder : Body
    {
        public double SizeZ { get; }

        public double Radius { get; }

        public Cylinder(Vector3 position, double sizeZ, double radius) : base(position)
        {
            SizeZ = sizeZ;
            Radius = radius;
        }
    }

    public class CompoundBody : Body
    {
        public IReadOnlyList<Body> Parts { get; }

        public CompoundBody(IReadOnlyList<Body> parts) : base(parts[0].Position)
        {
            Parts = parts;
        }
    }


    public interface IVisitor<TVisited, TResult> : IVisitor where TVisited : IVisitable
    {
        TResult Visit(TVisited visited);
    }

    public interface IVisitor
    {
        TResult Visit<TResult>(IVisitable visited);
    }

    public interface IVisitable
    {
        TResult Accept<TResult>(IVisitor visitor);
    }

    public abstract class Visitor : IVisitor
    {
        private Dictionary<string, object> _typedVisitors = new Dictionary<string, object>();

        protected void AddTypedVisitor<TVisited, TResult>(IVisitor<TVisited, TResult> visitor) where TVisited : IVisitable
        {
            var key = $"{typeof(TVisited).FullName}_{typeof(TResult).FullName}";

            _typedVisitors[key] = visitor;
        }

        public TResult Visit<TResult>(IVisitable visited)
        {
            var key = $"{visited.GetType().FullName}_{typeof(TResult).FullName}";

            if (!_typedVisitors.TryGetValue(key, out var visitorObj)) return default;

            throw new NotImplementedException();
            
        }

        protected abstract TResult Visit<TVisited, TResult>(IVisitor<TVisited, TResult> visitor)
            where TVisited : IVisitable;
    }

    public class BoundingBoxVisitor: IVisitor
    {
        
        public TResult Visit<TResult>(IVisitable visited)
        {
            throw new System.NotImplementedException();
        }
    }

    public class BoxifyVisitor: IVisitor
    {
        public TResult Visit<TResult>(IVisitable visited)
        {
            throw new System.NotImplementedException();
        }
    }
}