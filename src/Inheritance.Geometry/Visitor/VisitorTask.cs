
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Inheritance.Geometry.Visitor
{
    public abstract class Body : IVisitable
    {
        public Vector3 Position { get; }

        protected Body(Vector3 position)
        {
            Position = position;
        }

        public abstract TResult Accept<TResult>(IVisitor visitor);
    }

    public class Ball : Body
    {
        public double Radius { get; }

        public Ball(Vector3 position, double radius) : base(position)
        {
            Radius = radius;
        }

        public override TResult Accept<TResult>(IVisitor visitor)
        {
            return visitor.VisitBall<TResult>(this);
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

        public override TResult Accept<TResult>(IVisitor visitor)
        {
            return visitor.VisitRectangle<TResult>(this);
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

        public override TResult Accept<TResult>(IVisitor visitor)
        {
            return visitor.VisitCylinder<TResult>(this);
        }
    }

    public class CompoundBody : Body
    {
        public IReadOnlyList<Body> Parts { get; }

        public CompoundBody(IReadOnlyList<Body> parts) : base(parts[0].Position)
        {
            Parts = parts;
        }

        public override TResult Accept<TResult>(IVisitor visitor)
        {
            return visitor.VisitCompound<TResult>(this);
        }
    }

    public interface IVisitor
    {
        TResult VisitBall<TResult>(Ball visited);
        TResult VisitCylinder<TResult>(Cylinder visited);
        TResult VisitRectangle<TResult>(RectangularCuboid visited);
        TResult VisitCompound<TResult>(CompoundBody visited);
    }

    public interface IVisitable
    {
        TResult Accept<TResult>(IVisitor visitor);
    }

    public class BoundingBoxVisitor : IVisitor
    {
        public TResult VisitBall<TResult>(Ball visited)
        {
            if (typeof(TResult) != typeof(RectangularCuboid)) throw new ArgumentException();

            var d = visited.Radius * 2;
            var bound = new RectangularCuboid(visited.Position, d, d, d);

            return (TResult)(object)bound;
        }

        public TResult VisitCylinder<TResult>(Cylinder visited)
        {
            if (typeof(TResult) != typeof(RectangularCuboid)) throw new ArgumentException();

            var d = 2 * visited.Radius;

            var bound = new RectangularCuboid(visited.Position, d, d, visited.SizeZ);

            return (TResult)(object)bound;
        }

        public TResult VisitRectangle<TResult>(RectangularCuboid visited)
        {
            if (typeof(TResult) != typeof(RectangularCuboid)) throw new ArgumentException();

            var bound = new RectangularCuboid(visited.Position, visited.SizeX, visited.SizeY, visited.SizeZ);

            return (TResult)(object)bound;
        }

        public TResult VisitCompound<TResult>(CompoundBody visited)
        {
            if (typeof(TResult) != typeof(RectangularCuboid)) throw new ArgumentException();

            var x = FindBoundingBoxLine(visited, pos => pos.X);
            var y = FindBoundingBoxLine(visited, pos => pos.Y);
            var z = FindBoundingBoxLine(visited, pos => pos.Z);

            var bound = new RectangularCuboid(new Vector3(x.Center, y.Center, z.Center), x.Length, y.Length, z.Length);

            return (TResult)(object)bound;
        }

        private Line FindBoundingBoxLine(CompoundBody body, Func<Vector3, double> coordinateSelector)
        {
            var min = body.Parts.Min(p => coordinateSelector(GetMinPoint(p.TryAcceptVisitor<RectangularCuboid>(this))));
            var max = body.Parts.Max(p => coordinateSelector(GetMaxPoint(p.TryAcceptVisitor<RectangularCuboid>(this))));

            return new Line(min, max);
        }

        public Vector3 GetMinPoint(RectangularCuboid r) => new Vector3(
            r.Position.X - r.SizeX / 2,
            r.Position.Y - r.SizeY / 2,
            r.Position.Z - r.SizeZ / 2);
        public Vector3 GetMaxPoint(RectangularCuboid r) => new Vector3(
            r.Position.X + r.SizeX / 2,
            r.Position.Y + r.SizeY / 2,
            r.Position.Z + r.SizeZ / 2);

        private class Line
        {
            public Line(double start, double end)
            {
                Start = start;
                End = end;
            }

            private double Start { get; }
            private double End { get; }
            public double Center => (End + Start) / 2;
            public double Length => Math.Abs(End - Start);
        }
    }

    public class BoxifyVisitor : IVisitor
    {
        public TResult VisitBall<TResult>(Ball visited)
        {
            return new BoundingBoxVisitor().VisitBall<TResult>(visited);
        }

        public TResult VisitCylinder<TResult>(Cylinder visited)
        {
            return new BoundingBoxVisitor().VisitCylinder<TResult>(visited);
        }

        public TResult VisitRectangle<TResult>(RectangularCuboid visited)
        {
            return new BoundingBoxVisitor().VisitRectangle<TResult>(visited);
        }

        public TResult VisitCompound<TResult>(CompoundBody visited)
        {
            if (typeof(TResult) == typeof(RectangularCuboid)) return new BoundingBoxVisitor().VisitCompound<TResult>(visited);

            var parts = visited.Parts.Select(p =>
            {
                if (p is CompoundBody cb) return (Body)(CompoundBody)(object)VisitCompound<TResult>(cb);
                return (RectangularCuboid) (object) p.Accept<RectangularCuboid>(this);
            })
                .ToList();

            return (TResult) (object) new CompoundBody(parts);
        }
    }
}