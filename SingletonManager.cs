using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ReflectionSingleton
{
    public enum SingletonType
    {
        Class = 0,
        Mono = 1
    }

    public class SingletonManager
    {
        private static Dictionary<SingletonType, List<object>> _instances;
        private const string CSharpAssemblyName = "Assembly-CSharp";

        public SingletonManager()
        {
            _instances = new Dictionary<SingletonType, List<object>>()
            {
                { SingletonType.Class, new List<object> () { this } },
                { SingletonType.Mono, new List<object> () }
            };
        }

        public void Bind(object instance)
        {
            Type type = instance.GetType();

            if (Attribute.IsDefined(type, typeof(Injectable)))
            {
                _instances[SingletonType.Class].Add(instance);
            }

            InjectInternal(instance);
        }

        public void Bind(MonoBehaviour instance)
        {
            Type type = instance.GetType();

            if (Attribute.IsDefined(type, typeof(InjectableMono)))
            {
                _instances[SingletonType.Mono].Add(instance);
            }

            InjectInternal(instance);
        }

        public void BindUnityAssembly()
        {
            Assembly assembly = Assembly.Load(CSharpAssemblyName);

            var allMainAssemblyTypes = assembly.GetTypes();

            foreach (var type in allMainAssemblyTypes)
            {
                if (Attribute.IsDefined(type, typeof(Injectable)))
                {
                    object instance = Activator.CreateInstance(type);

                    if (instance == null)
                    {
                        continue;
                    }
                    _instances[SingletonType.Class].Add(instance);
                }
            }

            var monoObjects = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true);
            var shouldInjectIntoMono = new List<object>();

            foreach (var mono in monoObjects)
            {
                var type = mono.GetType();
                if (Attribute.IsDefined(type, typeof(InjectableMono)))
                {
                    _instances[SingletonType.Mono].Add(mono);
                }
                else if (Attribute.IsDefined(type, typeof(InjectIntoMono)))
                {
                    shouldInjectIntoMono.Add(mono);
                }
            }

            for (int i = 0; i < _instances.Count; i++)
            {
                var pair = _instances.ElementAt(i);

                for (int j = 0; j < pair.Value.Count; j++)
                {
                    var instance = pair.Value[j];

                    InjectInternal(instance);
                }
            }

            foreach (var instance in shouldInjectIntoMono)
            {
                InjectInternal(instance, true);
            }
        }

        public void Inject(object obj)
        {
            InjectInternal(obj);
        }

        public T Resolve<T>()
        {
            Type typeToResolve = typeof(T);

            return (T)Resolve(typeToResolve);
        }

        public static T InstantiateInjected<T>(T original, Transform parent) where T : Component
        {
            var obj = UnityEngine.Object.Instantiate(original, parent);
            InjectInternal(obj);
            return obj;
        }

        private static void InjectInternal(object obj, bool injectIntoMonoChildren = true)
        {
            var instanceType = obj.GetType();
            var instanceFields = instanceType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (instanceFields.Length > 0)
            {
                foreach (var field in instanceFields)
                {
                    var attributes = field.GetCustomAttribute(typeof(Singleton), true);

                    if (attributes != null)
                    {
                        var attributeFieldType = field.FieldType;

                        var attributeInstance = Resolve(attributeFieldType);

                        if (attributeInstance != null)
                        {
                            instanceType
                                .GetField(field.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?
                                .SetValue(obj, attributeInstance);
                        }
                        else
                        {
                            throw new Exception($"Type \"{attributeFieldType}\" not added to Injector.");
                        }
                    }
                }
            }

            if (!injectIntoMonoChildren)
            {
                return;
            }

            UnityEngine.Object unityObj = obj as UnityEngine.Object;

            if (unityObj != null)
            {
                if (Attribute.IsDefined(instanceType, typeof(InjectIntoMono)))
                {
                    var childs = new List<MonoBehaviour>();
                    var unityComponent = unityObj as Component;
                    if (unityComponent != null)
                    {
                        SingletonManagerUtils.GetInjectableMonoBehavioursUnderGameObject(unityComponent.gameObject, childs);
                        if (childs.Count > 0)
                        {
                            foreach (var item in childs)
                            {
                                InjectInternal(item, false);
                            }
                        }
                    }
                }
            }
        }

        private static object Resolve(Type type)
        {
            Type typeToResolve = type;

            for (int i = 0; i < _instances.Count; i++)
            {
                var pair = _instances.ElementAt(i);

                for (int j = 0; j < pair.Value.Count; j++)
                {
                    var instance = pair.Value[j];

                    if (instance.GetType() == typeToResolve)
                    {
                        return instance;
                    }
                }
            }

            return default;
        }
    }

    public static class SingletonManagerUtils
    {
        public static void GetInjectableMonoBehavioursUnderGameObject(
               GameObject gameObject, List<MonoBehaviour> injectableComponents)
        {
            if (gameObject == null)
            {
                return;
            }

            var monoBehaviours = gameObject.GetComponents<MonoBehaviour>();

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                var child = gameObject.transform.GetChild(i);

                if (child != null)
                {
                    GetInjectableMonoBehavioursUnderGameObject(child.gameObject, injectableComponents);
                }
            }

            for (int i = 0; i < monoBehaviours.Length; i++)
            {
                var monoBehaviour = monoBehaviours[i];

                if (monoBehaviour != null &&
                      Attribute.IsDefined(monoBehaviour.GetType(), typeof(InjectIntoMono)))
                {
                    injectableComponents.Add(monoBehaviour);
                }
            }
        }
    }
}