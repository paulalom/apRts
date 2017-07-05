using System.Collections.Generic;
/// <summary>
/// Nullable, Mutable generic pair
/// </summary>
/// <typeparam name="T1"></typeparam>
/// <typeparam name="T2"></typeparam>
public class MyPair<T1,T2> {
    public T1 Key;
    public T2 Value;    

    public MyPair() {}

    public MyPair(T1 key, T2 value)
    {
        Key = key;
        Value = value;
    }
    public MyPair(KeyValuePair<T1, T2> kvp)
    {
        Key = kvp.Key;
        Value = kvp.Value;
    }

    public KeyValuePair<T1, T2> ToKVP()
    {
        return new KeyValuePair<T1, T2>(Key, Value);
    }
}
