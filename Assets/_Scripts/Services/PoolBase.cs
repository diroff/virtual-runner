using System;
using System.Collections.Generic;

namespace Services
{
    public class PoolBase<T>
    {
        private readonly Queue<T> _inactiveItems = new();
        private readonly List<T> _activeItems = new();

        private readonly Func<T> _createItem;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onReturn;

        public IReadOnlyList<T> ActiveItems => _activeItems;

        public int ActiveCount => _activeItems.Count;
        public int InactiveCount => _inactiveItems.Count;

        public PoolBase(Func<T> createItem, Action<T> onGet, Action<T> onReturn, int preloadCount)
        {
            _createItem = createItem;
            _onGet = onGet;
            _onReturn = onReturn;

            Preload(preloadCount);
        }

        public void Preload(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T item = _createItem();

                _onReturn?.Invoke(item);
                _inactiveItems.Enqueue(item);
            }
        }

        public T Get()
        {
            return Get(null);
        }

        public T Get(Action<T> beforeGet)
        {
            T item = _inactiveItems.Count > 0 ? _inactiveItems.Dequeue() : _createItem();

            beforeGet?.Invoke(item);

            _activeItems.Add(item);
            _onGet?.Invoke(item);

            return item;
        }

        public void Return(T item)
        {
            if (!_activeItems.Remove(item))
                return;

            _onReturn?.Invoke(item);
            _inactiveItems.Enqueue(item);
        }

        public void ReturnAll()
        {
            while (_activeItems.Count > 0)
                Return(_activeItems[0]);
        }

        public bool ContainsActive(T item) => _activeItems.Contains(item);

        public bool ContainsInactive(T item) => _inactiveItems.Contains(item);
    }
}