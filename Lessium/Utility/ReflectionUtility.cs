using System;
using System.Linq;

namespace Lessium.Utility
{
    public static class ReflectionUtility
    {
        /// <summary>
        /// Test if a typeToCheck derives from genericType.
        /// Example: IsGenericsOf(control.GetType(), typeof(ITestControl<>) - true if control is SimpleTest<SimpleAnswerModel>.
        /// </summary>
        public static bool IsGenericsOf(Type typeToCheck, Type genericType)
        {
            if (typeToCheck == null) throw new ArgumentNullException(nameof(typeToCheck));
            if (genericType == null) throw new ArgumentNullException(nameof(genericType));

            var interfaceTest = new Predicate<Type>(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericType);

            return interfaceTest(typeToCheck) || typeToCheck.GetInterfaces().Any(i => interfaceTest(i));
        }
    }
}
