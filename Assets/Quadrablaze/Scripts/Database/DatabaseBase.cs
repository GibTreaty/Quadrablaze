using System;
using System.Collections.Generic;

public class DatabaseBase<T, T1> {

    Dictionary<T, T1> database = null;

    public DatabaseBase() {
        database = new Dictionary<T, T1>();
    }

    public void Add(T key, T1 value) {
        database[key] = value;
    }

    public void Clear() {
        database.Clear();
    }

    public void ForEach(Action<T1> method) {
        foreach(var value in database)
            method(value.Value);
    }

    public T1 Get(T key) {
        T1 item;

        database.TryGetValue(key, out item);

        return item;
    }
    public T2 Get<T2>(T key) where T2 : class, T1 {
        return Get(key) as T2;
    }

    public bool Remove(T key) {
        return database.Remove(key);
    }
}