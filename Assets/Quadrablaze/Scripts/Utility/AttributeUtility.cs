using System;
using System.Collections.Generic;
using System.Reflection;

public static class AttributeUtility {

    /// <summary>Loops through all references of an attribute</summary>
    public static IEnumerable<(MethodInfo, T)> GetAttributeInstances<T>(bool staticOnly = false) where T : Attribute {
        foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            if(!assembly.FullName.Contains("Assembly-CSharp")) continue;
            
            foreach(var type in assembly.GetTypes()) {
                foreach(var method in type.GetRuntimeMethods()) {
                    if(staticOnly && !method.IsStatic) continue;

                    if(method.GetCustomAttribute<T>() is T attribute)
                        yield return (method, attribute);
                }
            }
        }
    }
}