using System;

namespace Abstractions.Interfaces
{
    public interface ILevelCompleter
    {
        public event Action<ILevelCompleter> OnLevelCompleted;
        public bool IsCompleted { get; }
    }
}