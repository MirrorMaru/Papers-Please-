namespace xiv.raid.DataUtils.TomestoneStructure;

public class TomestoneData
{
    public Encounters? Encounters;
    public string? progPoint;

    public TomestoneData(Encounters encounters, string progPoint)
    {
        Encounters = encounters;
        progPoint = progPoint;
    }
}
