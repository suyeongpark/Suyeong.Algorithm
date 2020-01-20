namespace Suyeong.Algorithm.Type
{
    public interface IRect<T> : IDiagram<T>
    {
        T Width { get; }
        T Height { get; }
        T Diagonal { get; }
    }
}
