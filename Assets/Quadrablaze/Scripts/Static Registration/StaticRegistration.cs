using System;
using System.Reflection;

namespace Quadrablaze {
    public class RegisterNetworkHandlersAttribute : Attribute { }

    public static class NetworkRegistrationHelper {
        public static void InvokeRegisterNetworkHandlers() {
            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                if(!assembly.FullName.Contains("Assembly-CSharp")) continue;

                foreach(var type in assembly.GetTypes()) {
                    if(!type.IsClass) continue;

                    foreach(var method in type.GetRuntimeMethods()) {
                        if(!method.IsStatic) continue;

                        var attribute = method.GetCustomAttribute<RegisterNetworkHandlersAttribute>();

                        if(attribute == null)
                            continue;

                        method.Invoke(null, null);
                    }
                }
            }
        }
    }
}