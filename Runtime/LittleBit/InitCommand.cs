using System;

namespace LittleBit
{
    public abstract class InitCommand<T> : IInitCommand
    {
        public event Action<T> OnInit;
        public abstract bool IsInit { get; protected set; }
        public abstract void Init();
        public event Action OnCompleteInit;

        protected void CompleteInit(T message)
        {
            IsInit = true;
            OnInit?.Invoke(message);
            OnCompleteInit?.Invoke();
        }
    }
}