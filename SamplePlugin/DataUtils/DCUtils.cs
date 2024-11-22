namespace xiv.raid.DataUtils;

public static class DCUtils
{
    public static string GetRegionSludFromDCId(byte dcId)
    {
        return dcId switch
        {
            1 => "jp",
            2 => "na",
            3 => "eu",
            4 => "oce"
        };
    }
}
