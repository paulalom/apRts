﻿
public class WorldSettings {

    public static int starterWorldSizeRating = 8,
                      starterWorldAIPresenceRating = 2,
                      starterWorldNumStartLocations = 8;
    public static float starterWorldResourceAbundance = 6,
                        starterWorldResourceRarity = 3,
                        starterWorldStartLocationSizeRating = 2.2f,
                        starterWorldAIStrengthRating = 2;

    public int randomSeed;
    public int sizeRating,
               aiPresenceRating,
               numStartLocations;
    public float resourceAbundanceRating, 
                resourceQualityRating, 
                startLocationSizeRating, // 10 means 100% planet space will be used by start locations
                aiStrengthRating;
}
