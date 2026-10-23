using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Mechanics.LevelGeneration
{
    public sealed class ShuffleBag<T>
    {
        private readonly List<T> _items = new();

        private int _nextIndex;
        private T _lastItem;
        private bool _hasLastItem;

        public T Next(IReadOnlyList<T> source)
        {
            if (source == null || source.Count == 0)
                throw new ArgumentException("Shuffle bag source cannot be empty.", nameof(source));

            if (_nextIndex >= _items.Count)
                Refill(source);

            T item = _items[_nextIndex++];

            _lastItem = item;
            _hasLastItem = true;

            return item;
        }

        public void Reset()
        {
            _items.Clear();
            _nextIndex = 0;
            _lastItem = default;
            _hasLastItem = false;
        }

        private void Refill(IReadOnlyList<T> source)
        {
            _items.Clear();

            for (int i = 0; i < source.Count; i++)
                _items.Add(source[i]);

            for (int i = _items.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (_items[i], _items[j]) = (_items[j], _items[i]);
            }

            if (_items.Count > 1 && _hasLastItem && EqualityComparer<T>.Default.Equals(_items[0], _lastItem))
            {
                int swapIndex = UnityEngine.Random.Range(1, _items.Count);
                (_items[0], _items[swapIndex]) = (_items[swapIndex], _items[0]);
            }

            _nextIndex = 0;
        }
    }
}