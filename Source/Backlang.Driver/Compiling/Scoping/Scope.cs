namespace Backlang.Driver.Compiling.Scoping;

public class Scope
{
    private readonly List<ScopeItem> _items = new();

    public Scope(Scope parent)
    {
        Parent = parent;
    }

    public Scope Parent { get; set; }

    public void Add(string name, ScopeType itemType, bool isMutable = false)
    {
        _items.Add(new ScopeItem(name, itemType, isMutable));
    }

    public bool Contains(string name, ScopeType scopeType)
    {
        var containsItem = _items.Any(i => i.Name == name && i.Type == scopeType);

        if (Parent != null && !containsItem)
        {
            containsItem = Parent.Contains(name, scopeType);
        }

        return containsItem;
    }

    public bool Contains(string name)
    {
        return _items.Any(i => i.Name == name);
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