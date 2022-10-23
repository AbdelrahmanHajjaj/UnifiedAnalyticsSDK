using UnityEngine;

namespace UnifiedAnalyticsSDK.Utilities
{
    /// <summary>
    /// Be aware this will not prevent a non singleton constructor such as `T myT = new T();`
    /// To prevent that, add `protected T () {}` to your singleton class.
    /// As a note, this is made as MonoBehaviour because we need Coroutines.
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        /// <summary>
        /// A way of checking if the singleton is in the scene.
        /// useful for elements that clean up before destruction to avoid
        /// attempting to generate an instance in cases like application quit. 
        /// </summary>
        public static bool Exists => InstanceSafe != null;

        protected static bool ApplicationIsQuitting = false;

        private static T instance;

        private static readonly object instanceLock = new object();

        public static T Instance
        {
            get
            {

                lock (instanceLock)
                {
                    if (instance != null)
                    {
                        return instance;
                    }

                    if (ApplicationIsQuitting)
                    {
                        Debug.LogWarning(
                            $"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again - returning null.");

                        return null;
                    }

                    instance = (T)FindObjectOfType(typeof(T));


                    if (instance == null)
                    {
                        var singleton = new GameObject();
                        instance = singleton.AddComponent<T>();
                        singleton.name = $"(singleton) {typeof(T)}";

                        Debug.Log(
                            $"[Singleton] An instance of {typeof(T)} is needed in the scene, so '{singleton}' was created with DontDestroyOnLoad.");
                    }
                    else
                    {
#if UNITY_EDITOR
                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            Debug.LogError(
                                "[Singleton] Something went really wrong - there should never be more than 1 singleton! Reopening the scene might fix it.");
                        }
#endif
                        Debug.Log($"[Singleton] Using instance already created: {instance.gameObject.name}");
                    }

                    instance.InitSingleton();

                    return instance;
                }
            }
        }

        /// <summary>
        /// Fetches and returns an instance if it exists, without initializing, and does NOT generate a new one if there is no instance.   
        /// </summary>
        public static T InstanceSafe
        {
            get
            {
                lock (instanceLock)
                {
                    // if there is no instance, attempt to find an existing one.
                    if (instance == null)
                    {
                        instance = GameObject.FindObjectOfType(typeof(T)) as T;
                    }
                    // return the found instance or null if there isn't one; do not generate.
                    return instance;
                }
            }
        }

        /// <summary>
        /// Called the very first time an Instance is referenced.
        /// Put constructor-type initialization or DontDestroyOnLoad in the override.
        /// Not all singletons should persist across scenes.
        /// </summary>
        protected virtual void InitSingleton()
        {

        }

        /// <summary>
        /// OnApplicationQuit is called automatically before OnDisable and OnDestroy when the game is being exited.
        /// This allows us to accurately set a quit flag.    
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            ApplicationIsQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            lock (instanceLock)
            {
                if (ApplicationIsQuitting)
                {
                    instance = null;
                }
                else if (instance && instance == this)
                {
                    instance = null;
                }

                ApplicationIsQuitting = false;
            }
        }
    }
}