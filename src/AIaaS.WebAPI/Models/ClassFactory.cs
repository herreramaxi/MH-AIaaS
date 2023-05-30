namespace AIaaS.WebAPI.Models
{
    using Microsoft.ML;
    using Microsoft.ML.Data;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    public static class ClassFactory
    {

        public static object CreateObject(string[] PropertyNames, Type[] Types, bool[] addColumnName = null)
        {
            if (PropertyNames.Length != Types.Length)
            {
                Console.WriteLine("The number of property names should match their corresponding types number");
            }

            TypeBuilder DynamicClass = CreateTypeBuilder();
            CreateConstructor(DynamicClass);
            for (int ind = 0; ind < PropertyNames.Count(); ind++)
            {
                CreateProperty(DynamicClass, PropertyNames[ind], Types[ind], addColumnName != null ? addColumnName[ind] : false);
            }
            Type type = DynamicClass.CreateType();

            return Activator.CreateInstance(type);
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
                CreateProperty(typeBuilder, item.Name, item.Type.RawType);
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

        private static void CreateProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType, bool addColumnName = false)
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

            if (addColumnName)
            {

                var constructorInfo = typeof(ColumnNameAttribute).GetConstructor(new Type[] { typeof(string) });
                if (constructorInfo == null) return;

                var attrBuilder = new CustomAttributeBuilder(constructorInfo, new object[] { "Score" });
                propertyBuilder.SetCustomAttribute(attrBuilder);
            }
        }
    }
}
