using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WebApi.Common.Web.Documentation;

public sealed class ExampleFactory
{
    private readonly Dictionary<Type, Func<object>> _examples = [];

    public void Register<T>(Func<T> factory) where T : class
    {
        _examples[typeof(T)] = () => factory!();
    }

    public JsonNode? Resolve(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        var elementType = GetElementType(type) ?? type;
        if (!_examples.TryGetValue(elementType, out var factory))
        {
            return null;
        }

        var example = factory();
        var obj = elementType == type ? example : WrapList(elementType, example);
        return JsonSerializer.SerializeToNode(obj);
    }

    private static Type? GetElementType(Type type)
    {
        if (type.IsArray)
        {
            return type.GetElementType();
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            return type.GetGenericArguments()[0];
        }

        var genericType = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        return genericType?.GetGenericArguments()[0];
    }

    private static object WrapList(Type elementType, object example)
    {
        var listType = typeof(List<>).MakeGenericType(elementType);
        var list = (IList)Activator.CreateInstance(listType)!;
        list.Add(example);
        return list;
    }
}