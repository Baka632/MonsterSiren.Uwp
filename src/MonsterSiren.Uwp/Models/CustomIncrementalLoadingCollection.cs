using Microsoft.Toolkit.Collections;
using Microsoft.Toolkit.Uwp;

namespace MonsterSiren.Uwp.Models;

public sealed class CustomIncrementalLoadingCollection<TSource, IType> : IncrementalLoadingCollection<TSource, IType> where TSource : IIncrementalSource<IType>
{
    public TSource CollectionSource => Source;

    public CustomIncrementalLoadingCollection(TSource source, int itemsPerPage) : base(source, itemsPerPage)
    {
    }
}
