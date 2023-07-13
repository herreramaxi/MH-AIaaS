namespace AIaaS.Application.Common.Models
{
    using AIaaS.WebAPI.ExtensionMethods;
    using Microsoft.ML;
    using Microsoft.ML.Data;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    public static class ClassFactory
    {

        //public static object CreateObject(string[] PropertyNames, Type[] Types, string[]? columnNamesCustomAttr = null)
        //{
        //    if (PropertyNames.Length != Types.Length)
        //    {
        //        Console.WriteLine("The number of property names should match their corresponding types number");
        //    }

        //    TypeBuilder DynamicClass = CreateTypeBuilder();
        //    CreateConstructor(DynamicClass);
        //    for (int i = 0; i < PropertyNames.Count(); i++)
        //    {
        //        var columnNameCustomAttr = columnNamesCustomAttr?.Any() == true ? columnNamesCustomAttr[i] : null;
        //        CreateProperty(DynamicClass, PropertyNames[i], Types[i], columnNameCustomAttr);
        //    }
        //    Type type = DynamicClass.CreateType();

        //    return Activator.CreateInstance(type);
        //}

        public static Type CreateType(IEnumerable<(string, Type, string)> properties)
        {
            return CreateType(properties.ToArray());
        }

        public static Type CreateType((string, Type, string)[] properties)
        {
            var typeBuilder = CreateTypeBuilder();
            CreateConstructor(typeBuilder);

            foreach (var propertyType in properties)
            {
                CreateProperty(typeBuilder, propertyType.Item1, propertyType.Item2, propertyType.Item3);
            }

            return typeBuilder.CreateType();
        }

        public static Type CreateType(IEnumerable<(string, Type)> properties)
        {
            var typeBuilder = CreateTypeBuilder();
            CreateConstructor(typeBuilder);

            foreach (var propertyType in properties)
            {
                CreateProperty(typeBuilder, propertyType.Item1, propertyType.Item2);
            }

            return typeBuilder.CreateType();
        }

        public static Type CreateType(DataViewSchema dataViewSchema)
        {
            var typeBuilder = CreateTypeBuilder();
            CreateConstructor(typeBuilder);
            foreach (var item in dataViewSchema)
            {
                CreateProperty(typeBuilder, item.Name, item.Type.ToRawType());
            }

            return typeBuilder.CreateType();
        }
        private static TypeBuilder CreateTypeBuilder()
        {
            var assemblyName = new AssemblyName($"DynamicAssembly_{Guid.NewGuid():N}");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
            TypeBuilder typeBuilder = moduleBuilder.DefineType($"DynamicType_{Guid.NewGuid():N}"
                                , TypeAttributes.Public |
                                TypeAttributes.Class |
                                TypeAttributes.AutoClass |
                                TypeAttributes.AnsiClass |
                                TypeAttributes.BeforeFieldInit |
                                TypeAttributes.AutoLayout
                                , null);
            return typeBuilder;
        }

        private static void CreateConstructor(TypeBuilder typeBuilder)
        {
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
        }

        private static void CreateProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType, string? columnNameCustomAttribute = null)
        {
            FieldBuilder fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod("get_" + propertyName,
                MethodAttributes.Public |
                MethodAttributes.SpecialName |
                MethodAttributes.HideBySig,
                propertyType,
                Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr = typeBuilder.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });

            ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            //Label modifyProperty = setIl.DefineLabel();
            //Label exitSet = setIl.DefineLabel();

            //setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            //setIl.Emit(OpCodes.Nop);
            //setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);

            if (!string.IsNullOrEmpty(columnNameCustomAttribute))
            {
                var constructorInfo = typeof(ColumnNameAttribute).GetConstructor(new Type[] { typeof(string) });
                if (constructorInfo == null) return;

                var attrBuilder = new CustomAttributeBuilder(constructorInfo, new object[] { columnNameCustomAttribute });
                propertyBuilder.SetCustomAttribute(attrBuilder);
            }
        }
    }
}
