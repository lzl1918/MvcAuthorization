using System.Linq;
using System.Reflection;

namespace AuthorizationCore.Internal.Helpers
{
    internal static class TypeExtensions
    {
        internal static T[] GetAttributes<T>(this ICustomAttributeProvider attributeProvider, bool inherit)
        {
            return attributeProvider.GetCustomAttributes(typeof(T), inherit).Cast<T>().ToArray();
        }
        internal static bool HasAttribute<T>(this ICustomAttributeProvider attributeProvider, bool inherit)
        {
            return attributeProvider.GetCustomAttributes(typeof(T), inherit).Length > 0;
        }
    }
}
