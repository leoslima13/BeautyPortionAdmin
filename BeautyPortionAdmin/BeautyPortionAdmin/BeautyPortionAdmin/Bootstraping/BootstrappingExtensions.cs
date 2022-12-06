using DryIoc;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BeautyPortionAdmin.Bootstraping
{
    public static class BootstrappingExtensions
    {
        public static void BootstrapTypes(this IContainer container, params Assembly[] assemblies)
        {
            container.RegisterAssemblyTypesWithDefaultConvention(assemblies);
        }

        private static void RegisterAssemblyTypesWithDefaultConvention(this IContainer container, params Assembly[] assemblies)
        {
            foreach (var asm in assemblies)
            {
                var servicesTypes = asm.GetTypes()
                    .Where(MatchesDefaultConvetion)
                    .Where(t => !HasSingletonAttribute(t));

                container.RegisterMany(servicesTypes, reuse: Reuse.Transient, ifAlreadyRegistered: IfAlreadyRegistered.Replace);

                var singletonTypes = asm.GetTypes()
                    .Where(MatchesDefaultConvetion)
                    .Where(HasSingletonAttribute);

                container.RegisterMany(singletonTypes, reuse: Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            }
        }

        private static bool MatchesDefaultConvetion(Type type)
        {
            return type.GetCustomAttribute<IgnoreDefaultConventionAttribute>() == null && GetDefaultConventionInteraces(type).Any();
        }

        private static bool HasSingletonAttribute(Type type)
        {
            return type.GetCustomAttributes<SingletonAttribute>().Any();
        }

        private static IEnumerable<Type> GetDefaultConventionInteraces(Type type)
        {
            return type.GetInterfaces().Where(i => i.Name == $"I{type.Name}");
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class SingletonAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class IgnoreDefaultConventionAttribute : Attribute
    {

    }
}
