namespace AIaaS.WebAPI.ExtensionMethods
{
    public static class TypeReflectionExtensions
    {
        public static object? InvokeGenericMethod(this object sender, Type genericType, string methodName, Type[] methodParameterTypes, object[] parameters)
        {
            var value = sender.InvokeGenericMethod(new Type[] { genericType }, methodName, methodParameterTypes, parameters);
            return value;
        }

        public static object? InvokeGenericMethod(this object sender, Type[] genericTypes, string methodName, Type[] methodParameterTypes, object[] parameters)
        {
            var type = sender.GetType();
            var methodInfo = type.GetMethods().FirstOrDefault(m => m.Name == methodName &&
                    m.GetParameters()
                    .Select(p => p.ParameterType)
                    .SequenceEqual(methodParameterTypes));

            var processMethod = methodInfo?.MakeGenericMethod(genericTypes);
            var value = processMethod?.Invoke(sender, parameters);

            return value;
        }

        public static object? InvokeMethod(this object sender, string methodName, Type[] methodParameterTypes, object[] parameters)
        {
            var type = sender.GetType();
            var methodInfo = type.GetMethods().FirstOrDefault(m => m.Name == methodName &&
                    m.GetParameters()
                    .Select(p => p.ParameterType)
                    .SequenceEqual(methodParameterTypes));

            var value = methodInfo?.Invoke(sender, parameters);

            return value;
        }

    }
}
