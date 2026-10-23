using Abstraction.Interfaces;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using Reflex.Core;
using Reflex.Injectors;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Services
{
    public class PoolService : MonoBehaviour
    {
        [SerializeField, Min(0)] private int _defaultPreloadCount = 3;

        private readonly Dictionary<GameObject, PoolBase<GameObject>> _pools = new();
        private readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new();

        private readonly HashSet<GameObject> _createdInstances = new();
        private readonly HashSet<GameObject> _activeInstances = new();

        private readonly Dictionary<Type, List<Delegate>> _creationSubscribers = new();

        private Container _container;
        private GameObject _stagingRoot;

        public event Action<GameObject> OnPoolCreated;

        [Inject]
        private void Initialize(Container container)
        {
            _container = container;
        }

        public GameObject Get(GameObject prefab)
        {
            return Get(prefab, null);
        }

        public GameObject Get(GameObject prefab, Action<GameObject> beforeSpawn)
        {
            if (prefab == null)
            {
                Debug.LogError($"{nameof(PoolService)}: Prefab is null.", this);
                return null;
            }

            PoolBase<GameObject> pool = GetOrCreatePool(prefab);
            GameObject instance = pool.Get(beforeSpawn);

            _activeInstances.Add(instance);

            if (instance.TryGetComponent(out IPoolable poolable))
                poolable.OnSpawned();

            return instance;
        }

        public T Get<T>(T prefab) where T : Component
        {
            return Get(prefab, (Action<T>)null);
        }

        public T Get<T>(T prefab, Action<T> beforeSpawn) where T : Component
        {
            if (prefab == null)
            {
                Debug.LogError($"{nameof(PoolService)}: Prefab is null.", this);
                return null;
            }

            Action<GameObject> configure = beforeSpawn == null ? null : instance => beforeSpawn(instance.GetComponent<T>());
            GameObject instance = Get(prefab.gameObject, configure);

            return instance != null ? instance.GetComponent<T>() : null;
        }

        public void Return(GameObject instance)
        {
            if (instance == null)
                return;

            if (!_instanceToPrefab.TryGetValue(instance, out GameObject prefab))
            {
                Destroy(instance);
                return;
            }

            if (!_activeInstances.Remove(instance))
                return;

            if (instance.TryGetComponent(out IPoolable poolable))
                poolable.OnDespawned();

            _pools[prefab].Return(instance);
        }

        public void Return(Component component)
        {
            if (component != null)
                Return(component.gameObject);
        }

        public void Preload(GameObject prefab, int count)
        {
            if (prefab == null)
                return;

            count = Mathf.Max(0, count);

            if (!_pools.TryGetValue(prefab, out PoolBase<GameObject> pool))
            {
                CreatePool(prefab, count);
                return;
            }

            int missingCount = count - pool.ActiveCount - pool.InactiveCount;

            if (missingCount > 0)
                pool.Preload(missingCount);
        }

        public async UniTask PreloadAsync(GameObject prefab, int count, int itemsPerFrame = 2, CancellationToken cancellationToken = default)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            count = Mathf.Max(0, count);
            itemsPerFrame = Mathf.Max(1, itemsPerFrame);

            if (!_pools.TryGetValue(prefab, out PoolBase<GameObject> pool))
            {
                CreatePool(prefab, 0);
                pool = _pools[prefab];
            }

            int missingCount = count - pool.ActiveCount - pool.InactiveCount;

            while (missingCount > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int batchSize = Mathf.Min(itemsPerFrame, missingCount);

                pool.Preload(batchSize);
                missingCount -= batchSize;

                if (missingCount > 0)
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, cancellationToken);
            }
        }

        public IReadOnlyCollection<GameObject> GetActiveRaw()
        {
            return _activeInstances;
        }

        public List<T> GetActive<T>() where T : Component
        {
            List<T> result = new();

            foreach (GameObject instance in _activeInstances)
            {
                if (instance != null && instance.TryGetComponent(out T component))
                    result.Add(component);
            }

            return result;
        }

        public void SubscribeToCreated<T>(Action<T> callback) where T : class
        {
            if (callback == null)
                return;

            Type type = typeof(T);

            if (!_creationSubscribers.TryGetValue(type, out List<Delegate> subscribers))
            {
                subscribers = new List<Delegate>();
                _creationSubscribers.Add(type, subscribers);
            }

            subscribers.Add(callback);

            foreach (GameObject instance in _createdInstances)
            {
                if (instance == null)
                    continue;

                foreach (Component component in instance.GetComponents<Component>())
                    NotifyComponent(component, callback);
            }
        }

        public void UnsubscribeFromCreated<T>(Action<T> callback) where T : class
        {
            Type type = typeof(T);

            if (!_creationSubscribers.TryGetValue(type, out List<Delegate> subscribers))
                return;

            subscribers.Remove(callback);

            if (subscribers.Count == 0)
                _creationSubscribers.Remove(type);
        }

        private PoolBase<GameObject> GetOrCreatePool(GameObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out PoolBase<GameObject> pool))
            {
                CreatePool(prefab);
                pool = _pools[prefab];
            }

            return pool;
        }

        private void CreatePool(GameObject prefab, int preloadCount = -1)
        {
            if (_container == null)
                throw new InvalidOperationException($"{nameof(PoolService)} was not injected by Reflex.");

            if (preloadCount < 0)
                preloadCount = _defaultPreloadCount;

            _pools[prefab] = new PoolBase<GameObject>(CreateInstance, ActivateInstance, DeactivateInstance, preloadCount);

            OnPoolCreated?.Invoke(prefab);

            GameObject CreateInstance()
            {
                GameObject instance = Instantiate(prefab, GetStagingRoot().transform, false);

                try
                {
                    GameObjectInjector.InjectRecursive(instance, _container);

                    _instanceToPrefab.Add(instance, prefab);
                    _createdInstances.Add(instance);

                    if (instance.TryGetComponent(out IPoolable poolable))
                        poolable.OnCreated();

                    NotifySubscribers(instance);

                    instance.SetActive(false);
                    instance.transform.SetParent(transform, true);

                    return instance;
                }
                catch
                {
                    _instanceToPrefab.Remove(instance);
                    _createdInstances.Remove(instance);

                    Destroy(instance);

                    throw;
                }
            }
        }

        private GameObject GetStagingRoot()
        {
            if (_stagingRoot != null)
                return _stagingRoot;

            _stagingRoot = new GameObject($"{nameof(PoolService)}_Staging");
            _stagingRoot.transform.SetParent(transform, false);
            _stagingRoot.SetActive(false);

            return _stagingRoot;
        }

        private static void ActivateInstance(GameObject instance)
        {
            instance.SetActive(true);
        }

        private static void DeactivateInstance(GameObject instance)
        {
            instance.SetActive(false);
        }

        private void NotifySubscribers(GameObject instance)
        {
            foreach (Component component in instance.GetComponents<Component>())
            {
                if (component == null)
                    continue;

                foreach (KeyValuePair<Type, List<Delegate>> subscriber in _creationSubscribers)
                {
                    if (!subscriber.Key.IsAssignableFrom(component.GetType()))
                        continue;

                    foreach (Delegate callback in subscriber.Value)
                        NotifyComponent(component, callback);
                }
            }
        }

        private static void NotifyComponent(Component component, Delegate callback)
        {
            if (component == null || callback == null)
                return;

            Type[] genericArguments = callback.GetType().GenericTypeArguments;

            if (genericArguments.Length == 0 || !genericArguments[0].IsAssignableFrom(component.GetType()))
                return;

            callback.DynamicInvoke(component);
        }
    }
}
