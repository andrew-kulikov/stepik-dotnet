namespace Inheritance.Geometry.Visitor
{
    public static class BodyExtensions
    {
        public static TResult TryAcceptVisitor<TResult>(this Body body, IVisitor visitor)
        {
            return body.Accept<TResult>(visitor);
        }
    }
}