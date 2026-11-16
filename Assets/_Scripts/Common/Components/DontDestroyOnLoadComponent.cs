using System.Collections.Generic;
using UnityEngine;

namespace Common.Components
{
    public class DontDestroyOnLoadComponent : MonoBehaviour
    {
        [SerializeField] private string _uniqueId;

        private static readonly Dictionary<string, DontDestroyOnLoadComponent> Instances = new();

        private string _registeredId;

        private void Awake()
        {
            if (transform.parent != null)
            {
                Debug.LogError($"{nameof(DontDestroyOnLoadComponent)} requires a root GameObject.", this);
                Destroy(this);
                return;
            }

            string id = string.IsNullOrWhiteSpace(_uniqueId) ? gameObject.name : _uniqueId.Trim();

            if (Instances.TryGetValue(id, out var instance) && instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instances[id] = this;
            _registeredId = id;

            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_registeredId != null && Instances.TryGetValue(_registeredId, out var instance) && instance == this)
                Instances.Remove(_registeredId);
        }
    }
}