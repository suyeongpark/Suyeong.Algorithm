using System.Collections.Generic;

namespace Suyeong.Algorithm.Type
{
    public interface IPoly<T> : IDiagram<T>
    {
        IEnumerable<IPoint<T>> Points { get; }
    }
}
