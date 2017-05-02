
public class WorldSettings {

    public static int starterWorldSizeRating = 8;
    public static float starterWorldResourceAbundance = 6,
                        starterWorldResourceRarity = 3,
                        starterWorldNumStartLocations = 8,
                        starterWorldStartLocationSizeRating = 6,
                        starterWorldAIPresenceRating = 3,
                        starterWorldAIStrengthRating = 2;

    public int randomSeed;
    public int sizeRating;
    public float resourceAbundanceRating, 
                resourceQualityRating, 
                numStartLocations,
                startLocationSizeRating, // 10 means all planet space will be used by start locations
                aiPresenceRating, 
                aiStrengthRating;
}
