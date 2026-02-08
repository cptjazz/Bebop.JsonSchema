using System.Buffers;

namespace Bebop.JsonSchema
{
    internal static class Pool
    {
        internal struct PoolRental<T>(T[] array) : IDisposable
        {
            private int _position = 0;

            public void Add(T item)
            {
                array[_position++] = item;
            }

            public void Dispose()
            {
                ArrayPool<T>.Shared.Return(array);
            }

#if NET9_0_OR_GREATER
            public ReadOnlySpan<T> AsSpan()
            {
                return array.AsSpan(0, _position);
            }
#else
            public T[] AsSpan()
            {
                return array.AsSpan(0, _position).ToArray();
            }
#endif
        }

        public static PoolRental<T> RentArray<T>(int minimumLength)
        {
            return new (ArrayPool<T>.Shared.Rent(minimumLength));
        }
    }
}
