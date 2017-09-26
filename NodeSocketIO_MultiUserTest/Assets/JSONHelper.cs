//modified from https://stackoverflow.com/questions/38390274/unity-c-how-to-convert-an-array-of-a-class-into-json
using System;

public class JSONHelper
{
    public static T FromJsonObject<T>(string json)
    {
        T obj = UnityEngine.JsonUtility.FromJson<T>(json);
        return obj;
    }

    public static T[] FromJsonArray<T>(string json)
    {
        Wrapper<T> wrapper = UnityEngine.JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.items;
    }

    public static string ToJsonObject<T>(T obj)
    {
        return UnityEngine.JsonUtility.ToJson(obj);
    }

    public static string ToJsonArray<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.items = array;
        return UnityEngine.JsonUtility.ToJson(wrapper);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }
}