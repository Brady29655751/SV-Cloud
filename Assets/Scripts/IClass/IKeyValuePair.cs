using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKeyValuePair<TKey, TValue>
{
    public TKey Key;
    public TValue Value;

    public IKeyValuePair(){}

    public IKeyValuePair(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }
}
