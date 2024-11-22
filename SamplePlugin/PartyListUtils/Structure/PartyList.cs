using System.Collections.Generic;

namespace xiv.raid.PartyListUtils.Structure;

public class PartyList
{
    public List<Player> players { get; }

    public PartyList()
    {
        players = new List<Player>();
    }

    public void AddPlayer(Player player)
    {
        players.Add(player);
    }

    public void RemovePlayer(Player player)
    {
        players.Remove(player);
    }
}
