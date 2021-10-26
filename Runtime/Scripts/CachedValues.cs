using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CachedValues : IDisposable
{
    private List<CachedValue> _values = new List<CachedValue>();

    public void CacheValue<Tvalue>(Tvalue value, Action<Tvalue> restorer)
        => _values.Add(new CachedValueT<Tvalue>(value, restorer));

    public void Restore()
    {
        foreach (var value in _values)
            value.Restore();
    }

    public void Dispose()
    {
        foreach (var value in _values)
            value.Dispose();
    }

    protected abstract class CachedValue : IDisposable
    {
        public abstract void Dispose();
        public abstract void Restore();
    }

    protected class CachedValueT<Tvalue> : CachedValue
    {
        private Tvalue _value;
        private Action<Tvalue> _set;

        public CachedValueT(Tvalue value, Action<Tvalue> set)
        {
            _value = value;
            _set = set;
        }

        public override void Dispose() => _value = default(Tvalue);
        public override void Restore() => _set.Invoke(_value);
    }
}