using System;

namespace LittleBit
{
    public interface IInitCommand
    {
        public void Init();
        public event Action OnCompleteInit;
        public bool IsInit { get; }
    }
}