using System;
using System.Collections.Generic;
using System.Linq;

namespace Inheritance.Geometry.Visitor
{
    public abstract class Body : IVisitable
    {
        protected Body(Vector3 position)
        {
            Position = position;
        }

        public Vector3 Position { get; }

        public abstract object Accept(IVisitor visitor);
    }

    public class Ball : Body
    {
        public Ball(Vector3 position, double radius) : base(position)
        {
            Radius = radius;
        }

        public double Radius { get; }

        public override object Accept(IVisitor visitor)
        {
            return visitor.VisitBall(this);
        }
    }

    public class RectangularCuboid : Body
    {
        public RectangularCuboid(Vector3 position, double sizeX, double sizeY, double sizeZ) : base(position)
        {
            SizeX = sizeX;
            SizeY = sizeY;
            SizeZ = sizeZ;
        }

        public double SizeX { get; }
        public double SizeY { get; }
        public double SizeZ { get; }

        public override object Accept(IVisitor visitor)
        {
            return visitor.VisitRectangle(this);
        }
    }

    public class Cylinder : Body
    {
        public Cylinder(Vector3 position, double sizeZ, double radius) : base(position)
        {
            SizeZ = sizeZ;
            Radius = radius;
        }

        public double SizeZ { get; }

        public double Radius { get; }

        public override object Accept(IVisitor visitor)
        {
            return visitor.VisitCylinder(this);
        }
    }

    public class CompoundBody : Body
    {
        public CompoundBody(IReadOnlyList<Body> parts) : base(parts[0].Position)
        {
            Parts = parts;
        }

        public IReadOnlyList<Body> Parts { get; }

        public override object Accept(IVisitor visitor)
        {
            return visitor.VisitCompound(this);
        }
    }

    public interface IVisitor
    {
        object VisitBall(Ball visited);
        object VisitCylinder(Cylinder visited);
        object VisitRectangle(RectangularCuboid visited);
        object VisitCompound(CompoundBody visited);
    }

    public interface IVisitable
    {
        object Accept(IVisitor visitor);
    }

    public class BoundingBoxVisitor : IVisitor
    {
        public object VisitBall(Ball visited)
        {
            var d = visited.Radius * 2;

            return new RectangularCuboid(visited.Position, d, d, d);
        }

        public object VisitCylinder(Cylinder visited)
        {
            var d = 2 * visited.Radius;

            return new RectangularCuboid(visited.Position, d, d, visited.SizeZ);
        }

        public object VisitRectangle(RectangularCuboid visited)
        {
            return new RectangularCuboid(visited.Position, visited.SizeX, visited.SizeY, visited.SizeZ);
        }

        public object VisitCompound(CompoundBody visited)
        {
            var x = FindBoundingBoxLine(visited, pos => pos.X);
            var y = FindBoundingBoxLine(visited, pos => pos.Y);
            var z = FindBoundingBoxLine(visited, pos => pos.Z);

            return new RectangularCuboid(new Vector3(x.Center, y.Center, z.Center), x.Length, y.Length, z.Length);
        }

        private Line FindBoundingBoxLine(CompoundBody body, Func<Vector3, double> coordinateSelector)
        {
            var min = body.Parts.Min(p => coordinateSelector(GetMinPoint(p.TryAcceptVisitor<RectangularCuboid>(this))));
            var max = body.Parts.Max(p => coordinateSelector(GetMaxPoint(p.TryAcceptVisitor<RectangularCuboid>(this))));

            return new Line(min, max);
        }

        public Vector3 GetMinPoint(RectangularCuboid r)
        {
            return new Vector3(
                r.Position.X - r.SizeX / 2,
                r.Position.Y - r.SizeY / 2,
                r.Position.Z - r.SizeZ / 2);
        }

        public Vector3 GetMaxPoint(RectangularCuboid r)
        {
            return new Vector3(
                r.Position.X + r.SizeX / 2,
                r.Position.Y + r.SizeY / 2,
                r.Position.Z + r.SizeZ / 2);
        }

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
        public object VisitBall(Ball visited)
        {
            return new BoundingBoxVisitor().VisitBall(visited);
        }

        public object VisitCylinder(Cylinder visited)
        {
            return new BoundingBoxVisitor().VisitCylinder(visited);
        }

        public object VisitRectangle(RectangularCuboid visited)
        {
            return new BoundingBoxVisitor().VisitRectangle(visited);
        }

        public object VisitCompound(CompoundBody visited)
        {
            var parts = visited.Parts.Select(p =>
                {
                    if (p is CompoundBody cb) return (Body) (CompoundBody) VisitCompound(cb);
                    return (RectangularCuboid) p.Accept(this);
                })
                .ToList();

            return new CompoundBody(parts);
        }
    }
}