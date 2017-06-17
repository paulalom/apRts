using System;
using System.Collections.Generic;

public class MyPairFactory {
    
    public static MyPair<Type, int> OneFactory()
    {
        return new MyPair<Type, int>(typeof(Factory), 1);
    }
    public static MyPair<Type, int> OneHarvestingStation()
    {
        return new MyPair<Type, int>(typeof(HarvestingStation), 1);
    }
}
