using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class NetworkDataAdapter : MyMonoBehaviour {

    // Convert an object to a byte array
    private byte[] ObjectToByteArray(Object obj)
    {
        if (obj == null)
            return null;

        BinaryFormatter bf = new BinaryFormatter();

        using (MemoryStream ms = new MemoryStream())
        {
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

    // Convert a byte array to an Object
    private Object ByteArrayToObject(byte[] arrBytes)
    {
        using (MemoryStream memStream = new MemoryStream())
        {
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);
            return obj;
        }
    }
}
