namespace Suyeong.Algorithm.Type
{
    public interface IEllipse<T> : IDiagram<T>
    {
        T MinorAxis { get; }
        T MajorAxis { get; }
        T Radian { get; }
        T Degree { get; }
    }
}
