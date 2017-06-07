using System;
using System.Collections.Generic;

public class MyKVPFactory {
    
    public static MyKVP<Type, int> OneFactory()
    {
        return new MyKVP<Type, int>(typeof(Factory), 1);
    }
    public static MyKVP<Type, int> OneHarvestingStation()
    {
        return new MyKVP<Type, int>(typeof(HarvestingStation), 1);
    }
}
