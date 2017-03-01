using System.Collections.Generic;
/// <summary>
/// Nullable, Mutable KeyValue pair (tuple?)
/// </summary>
/// <typeparam name="T1"></typeparam>
/// <typeparam name="T2"></typeparam>
public class MyKVP<T1,T2> {
    public T1 Key;
    public T2 Value;    

    public MyKVP(T1 key, T2 value)
    {
        Key = key;
        Value = value;
    }
    public MyKVP(KeyValuePair<T1, T2> kvp)
    {
        Key = kvp.Key;
        Value = kvp.Value;
    }

    public KeyValuePair<T1, T2> ToKVP()
    {
        return new KeyValuePair<T1, T2>(Key, Value);
    }
}
