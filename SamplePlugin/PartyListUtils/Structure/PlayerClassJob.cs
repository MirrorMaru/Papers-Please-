namespace xiv.raid.PartyListUtils.Structure;

public class PlayerClassJob
{
    public string ClassJobName { get; set; }
    public string ClassJobAbbreviation { get; set; }

    public PlayerClassJob(string classJobName, string classJobAbbreviation)
    {
        ClassJobName = classJobName;
        ClassJobAbbreviation = classJobAbbreviation;
    }
}
