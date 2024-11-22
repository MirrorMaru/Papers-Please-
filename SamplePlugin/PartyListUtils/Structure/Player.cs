using System.Collections.Generic;
using xiv.raid.DataUtils.FFLogsStructure;

namespace xiv.raid.PartyListUtils.Structure;

public class Player
{
    public string name { get; set; }
    public string world {get; set;}
    public string region {get; set;}
    public string job { get; set; }
    public List<Ranking> papers;
    public bool isFetching;
    public bool needFetching;

    public Player(string name, string world, string region, string job)
    {
        this.name = name;
        this.world = world;
        this.region = region;
        this.job = job;
        papers = new List<Ranking>();
        isFetching = false;
        needFetching = false;
    }
}
