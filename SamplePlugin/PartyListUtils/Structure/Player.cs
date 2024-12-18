using System.Collections.Generic;
using xiv.raid.DataUtils.FFLogsStructure;
using xiv.raid.DataUtils.TomestoneStructure;

namespace xiv.raid.PartyListUtils.Structure;

public class Player
{
    public string name { get; set; }
    public string world {get; set;}
    public string region {get; set;}
    public string internalIdentifier {get; set;}
    public PlayerClassJob job { get; set; }
    public List<Ranking> papers;
    public TomestoneData TomestoneData;
    public bool isFetching;
    public bool needFetching;
    public bool needTomestoneFetching;
    public bool isTomestoneFetching;
    public bool noLogs;
    public int lodestoneID;

    public Player(string name, string world, string region, PlayerClassJob job)
    {
        this.name = name;
        this.world = world;
        this.region = region;
        this.job = job;
        internalIdentifier = name + world;
        papers = new List<Ranking>();
        isFetching = false;
        needFetching = true;
        noLogs = false;
        needTomestoneFetching = true;
        isTomestoneFetching = false;
    }
}
