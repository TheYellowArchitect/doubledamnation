using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

//Namespace here

// See this: https://blog.buffalogamespace.com/2017/10/26/marks-favorite-unity-c-extensions/ and there are other such "libraries" to implement in the future.
public static class ExtensionMethods 
{
    //=================
    //  Randomness
    //=================

            //====================
            //  RandomIndexElement
            //====================

            /// <summary>
            /// Returns an element from inside the array, randomly
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="array"></param>
            /// <returns></returns>
            public static T GetRandomArrayIndexElement<T>(this T[] array)
            {
                if (array.Length == 0)
                {
                    Debug.LogError("Static Method: \"GetRandomArrayIndexElement\" failed, because the assigned array, has no elements! Like, at all! Empty. length = 0.");//tfw the game will go open-source so i detail more
                    return default(T);
                }

                return array[Random.Range(0, array.Length)];//Note that the array.Length is NOT included, because [exclusive] parameter in the Random.Range!
            }

            /// <summary>
            /// Returns an element from inside the array, randomly, EXCEPT, the one given in the parameter.\nJust like GetRandomIndexElement, but avoiding a duplicate :P
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="array"></param>
            /// <param name="elementException"></param>
            /// <returns></returns>
            public static T GetRandomArrayIndexElementExcept<T>(this T[] array, T elementException)//... Can we add an exception array there instead?!//Another time, gotta go eat now
            {
                if (array.Length == 0)
                {
                    Debug.LogError("Static Method: \"GetRandomArrayIndexElementExcept\" failed, because the assigned array, has no elements! Like, at all! Empty. length = 0.");//tfw the game will go open-source so i detail more
                    return default(T);
                }
            

                T tempElement;
        
                while (true)//could put the below line in (true) parenthesis, but would be harder to read.
                {
                    tempElement = array[Random.Range(0, array.Length)];//RandomSeed is seeded from InitGame()

                    if (tempElement.Equals(elementException) == false)
                        break;
                }

                return tempElement;
            }

            //The same as 2 above, but with lists
            public static T GetRandomListIndexElement<T>(this IList<T> list)
            {
                if (list.Count == 0)
                {
                    Debug.LogError("Static Method: \"GetRandomListIndexElement\" failed, because the assigned array, has no elements! Like, at all! Empty. length = 0.");//tfw the game will go open-source so i detail more
                    return default(T);
                }

                return list[Random.Range(0, list.Count)];
            }

            public static T GetRandomListIndexElementExcept<T>(this IList<T> list, T elementException)
            {
                if (list.Count == 0)
                {
                    Debug.LogError("Static Method: \"GetRandomListIndexElement\" failed, because the assigned array, has no elements! Like, at all! Empty. length = 0.");//tfw the game will go open-source so i detail more
                    return default(T);
                }

                T tempElement;

                while (true)//could put the below line in (true) parenthesis, but would be harder to read.
                {
                    tempElement = list[Random.Range(0, list.Count)];

                    if (tempElement.Equals(elementException) == false)
                        break;
                }

                return tempElement;
            }

    //=================
    //  Transform
    //=================

    /// <summary>
    /// Resets the transform in the parameter, just like pressing "Reset Transform" from Unity's Inspector UI.
    /// </summary>
    /// <param name="transform"></param>
    public static void ResetTransform(this Transform transform)
    {
        transform.position = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    //=================
    //  Vector
    //=================

    /// <summary>
    /// Converts a Vector3, to Vector2 by removing its Z.
    /// </summary>
    /// <param name="vector3ToConvert"></param>
    /// <returns></returns>
    public static Vector2 ToVector2 (this Vector3 vector3ToConvert)//should make alternatives with different axis removals.
    {
        return new Vector2(vector3ToConvert.x, vector3ToConvert.y);
    }

    //=================
    //  Netcoding
    //=================

    // Converts an Object, to a byte[]
    public static byte[] ObjectToByteArray (this object obj)
    {
        if (obj == null)
            return null;

        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, obj);

        return ms.ToArray();
    }

    // Converts a byte[] to an Object<T>
    public static T ByteArrayToObject<T> (this byte[] arrBytes)
    {
        MemoryStream ms = new MemoryStream();
        BinaryFormatter bf = new BinaryFormatter();
        ms.Write(arrBytes, 0, arrBytes.Length);
        ms.Seek(0, SeekOrigin.Begin);
        T obj = (T)bf.Deserialize(ms);

        return obj;
    }
}
