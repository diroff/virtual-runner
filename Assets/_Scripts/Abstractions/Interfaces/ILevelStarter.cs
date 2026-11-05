using System;

namespace Abstractions.Interfaces
{
    public interface ILevelStarter
    {
        public event Action<ILevelStarter> OnLevelStarted;
        public bool IsStarted { get; }
        public void Start();
    }
}