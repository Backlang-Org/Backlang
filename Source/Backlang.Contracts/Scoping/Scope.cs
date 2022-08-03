namespace Backlang.Contracts.Scoping;

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
        if (Contains(item.Name)) return false;
        _items.Add(item);
        return true;
    }

    public bool Contains(string name) => _items.Any(_ => _.Name == name);

    public bool TryFind<T>(string name, out T result) where T : ScopeItem
    {
        result = _items.FirstOrDefault(_ => _.Name == name) as T;
        return result != null;
    }

    public bool TryFind<T>(Func<ScopeItem, bool> test, out T result) where T : ScopeItem
    {
        result = _items.FirstOrDefault(_ => _ is T && test(_)) as T;
        return result != null;
    }

    public Scope CreateChildScope()
    {
        return new Scope(this);
    }

    public bool TryGet(string name, out ScopeItem item)
    {
        item = _items.FirstOrDefault(i => i.Name == name);

        if (Parent != null)
        {
            if (!Parent.TryGet(name, out item))
            {
                item = null;
            }
        }

        return item != null;
    }
}