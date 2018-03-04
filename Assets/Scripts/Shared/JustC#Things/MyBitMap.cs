using UnityEngine;
using System.Collections;

/// <summary>
/// A 2d array of bits which is dense and fast-access
/// Square maps only
/// </summary>
public class MyBitMap
{
    public readonly BitArray bitMap;
    public readonly int width, height;

    public MyBitMap(int width, int height)
    {
        this.width = width;
        this.height = height;
        bitMap = new BitArray(width * height);
    }

    public MyBitMap(int width, int height, BitArray bitMap)
    {
        if (bitMap.Count != width * height)
        {
            throw new System.Exception("Invalid bitMap size passed to constructor");
        }
        this.width = width;
        this.height = height;
        this.bitMap = bitMap;
    }

    public bool this[int x, int y]
    {
        get { return bitMap[x + y * width];  }
        set { bitMap[x + y * width] = value; }
    }

    public bool Get(int x, int y)
    {
        return bitMap[x + y * width];
    }

    public void Set(int x, int y, bool value)
    {
        bitMap[x + y * width] = value;
    }

    public override string ToString()
    {
        string bitMap = "";
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bitMap += this[x, y] ? "1" : "0";
            }
            bitMap += "\r\n";
        }
        return bitMap;
    }

}
