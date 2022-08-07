using Backlang.Contracts.Scoping.Items;

namespace Backlang.Contracts.Scoping;

#nullable enable

public class Scope
{
    private readonly List<ScopeItem> _items = new();

    public Scope(Scope parent)
    {
        Parent = parent;
    }

    public Scope Parent { get; set; }

    public bool Add(ScopeItem item)
    {
        if (_items.FirstOrDefault(_ => _.Name == item.Name) is FunctionScopeItem fsi && item is FunctionScopeItem isI)
        {
            fsi.Overloads.AddRange(isI.Overloads);
            return true;
        }
        else
        {
            if (Contains(item.Name)) return false;

            _items.Add(item);
            return true;
        }
    }

    public bool Contains(string name) => _items.Any(_ => _.Name == name);

    public Scope CreateChildScope()
    {
        return new Scope(this);
    }

    public bool TryGet<T>(string name, out T? item)
        where T : ScopeItem
    {
        item = (T)_items.FirstOrDefault(i => i is T && i.Name == name);

        if (item == null && Parent != null)
        {
            if (!Parent.TryGet(name, out item))
            {
                item = null;
            }
        }

        return item != null;
    }
}