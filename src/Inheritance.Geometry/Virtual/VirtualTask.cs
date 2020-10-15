using System;
using System.Collections.Generic;
using System.Linq;

namespace Inheritance.Geometry.Virtual
{
    public abstract class Body
    {
        public Vector3 Position { get; }

        protected Body(Vector3 position)
        {
            Position = position;
        }

        public abstract bool ContainsPoint(Vector3 point);

        public abstract RectangularCuboid GetBoundingBox();
    }

    public class Ball : Body
    {
        public double Radius { get; }

        public Ball(Vector3 position, double radius) : base(position)
        {
            Radius = radius;
        }

        public override bool ContainsPoint(Vector3 point)
        {
            var vector = point - Position;
            var length2 = vector.GetLength2();

            return length2 <= Radius * Radius;
        }

        public override RectangularCuboid GetBoundingBox()
        {
            var d = Radius * 2;

            return new RectangularCuboid(Position, d, d, d);
        }
    }

    public class RectangularCuboid : Body
    {
        public double SizeX { get; }
        public double SizeY { get; }
        public double SizeZ { get; }

        public Vector3 MinPoint => new Vector3(
            Position.X - SizeX / 2,
            Position.Y - SizeY / 2,
            Position.Z - SizeZ / 2);
        public Vector3 MaxPoint => new Vector3(
            Position.X + SizeX / 2,
            Position.Y + SizeY / 2,
            Position.Z + SizeZ / 2);

        public RectangularCuboid(Vector3 position, double sizeX, double sizeY, double sizeZ) : base(position)
        {
            SizeX = sizeX;
            SizeY = sizeY;
            SizeZ = sizeZ;
        }

        public override bool ContainsPoint(Vector3 point)
        {
            return point >= MinPoint && point <= MaxPoint;
        }

        public override RectangularCuboid GetBoundingBox()
        {
            return new RectangularCuboid(Position, SizeX, SizeY, SizeZ);
        }

        public static RectangularCuboid FromMinMaxPoint(Vector3 minPoint, Vector3 maxPoint)
        {
            var centerX = (maxPoint.X + minPoint.X) / 2;
            var centerY = (maxPoint.Y + minPoint.Y) / 2;
            var centerZ = (maxPoint.Z + minPoint.Z) / 2;

            var sizeX = Math.Abs(maxPoint.X - minPoint.X) / 2;
            var sizeY = Math.Abs(maxPoint.Y - minPoint.Z) / 2;
            var sizeZ = Math.Abs(maxPoint.Z - minPoint.Z) / 2;

            return new RectangularCuboid(new Vector3(centerX, centerY, centerZ), sizeZ, sizeY, sizeZ);
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

        public override bool ContainsPoint(Vector3 point)
        {
            var vectorX = point.X - Position.X;
            var vectorY = point.Y - Position.Y;
            var length2 = vectorX * vectorX + vectorY * vectorY;
            var minZ = Position.Z - SizeZ / 2;
            var maxZ = minZ + SizeZ;

            return length2 <= Radius * Radius && point.Z >= minZ && point.Z <= maxZ;
        }

        public override RectangularCuboid GetBoundingBox()
        {
            var d = 2 * Radius;

           return new RectangularCuboid(Position, d, d, SizeZ);
        }
    }

    public class CompoundBody : Body
    {
        public IReadOnlyList<Body> Parts { get; }

        public CompoundBody(IReadOnlyList<Body> parts) : base(parts[0].Position)
        {
            Parts = parts;
        }

        public override bool ContainsPoint(Vector3 point)
        {
            return Parts.Any(body => body.ContainsPoint(point));
        }

        public override RectangularCuboid GetBoundingBox()
        {
            var x = FindBoundingBoxLine(pos => pos.X);
            var y = FindBoundingBoxLine(pos => pos.Y);
            var z = FindBoundingBoxLine(pos => pos.Z);

            return new RectangularCuboid(new Vector3(x.Center, y.Center, z.Center), x.Length, y.Length, z.Length);
        }

        private Line FindBoundingBoxLine(Func<Vector3, double> coordinateSelector)
        {
            var min = Parts.Min(p => coordinateSelector(p.GetBoundingBox().MinPoint));
            var max = Parts.Max(p => coordinateSelector(p.GetBoundingBox().MaxPoint));

            return new Line(min, max);
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
}