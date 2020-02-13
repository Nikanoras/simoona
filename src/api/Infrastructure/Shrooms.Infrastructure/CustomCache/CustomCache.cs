﻿using System.Collections.Concurrent;
using Shrooms.Host.Contracts.Infrastructure;

namespace Shrooms.Infrastructure.CustomCache
{
    public class CustomCache<TKey, TValue> : ConcurrentDictionary<TKey, TValue>, ICustomCache<TKey, TValue>
    {
        public bool TryRemoveEntry(TKey key)
        {
            TValue removedValue;
            return TryRemove(key, out removedValue);
        }
    }
}
