using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema;

[ExcludeFromCodeCoverage]
internal static class SyncContext
{
    public static ClearContextAwaiter Drop() => default;

    public readonly struct ClearContextAwaiter : ICriticalNotifyCompletion
    {
        public ClearContextAwaiter GetAwaiter() => this;

        public bool IsCompleted => true;

        public void GetResult() 
        {
            SynchronizationContext.SetSynchronizationContext(null);
        }

        public void OnCompleted(Action continuation) => continuation();

        public void UnsafeOnCompleted(Action continuation) => continuation();
    }
}
