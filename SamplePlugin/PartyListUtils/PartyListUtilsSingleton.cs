using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dalamud.Game.ClientState.Party;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Lumina.Excel.Sheets;
using xiv.raid.DataUtils;
using xiv.raid.PartyListUtils.Structure;

namespace xiv.raid.PartyListUtils;

public class PartyListUtilsSingleton
{
    private static PartyListUtilsSingleton _instance;
    private static readonly object _lock = new();

    public static PartyListUtilsSingleton Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("PartyListUtils is not initialized.");
            }
            return _instance;
        }
    }

    public static void Initialize()
    {
        if (_instance != null)
        {
            throw new InvalidOperationException("PartyListUtils has already been initialized.");
        }

        lock (_lock)
        {
            if (_instance == null)
            {
                _instance = new PartyListUtilsSingleton();
            }
        }
    }

    public unsafe PartyList InitializePartyList()
    {
        PartyList partyList = new();
        for (int i = 0; i < InfoProxyCrossRealm.GetPartyMemberCount(); i++)
        {
            CrossRealmMember member = GetCrossRealmMember(InfoProxyCrossRealm.GetGroupMember((uint)i));
            partyList.AddPlayer(ConvertCrossRealmMemberToPlayer(member));
        }
        return partyList;
    }

    public void RefreshPartyList(PartyList partyList)
    {
        PartyList newPlayers = GetNewPlayer(partyList);
        PartyList leavers = GetLeavers(partyList);

        foreach (Player player in newPlayers.players)
        {
            partyList.AddPlayer(new Player(player.name, player.world, player.region, player.job));
        }

        List<string> idToRemove = new List<string>();
        foreach (Player player in leavers.players)
        {
            for (int i = 0; i < partyList.players.Count; i++)
            {
                if (player.internalIdentifier.Equals(partyList.players[i].internalIdentifier))
                {
                    idToRemove.Add(partyList.players[i].internalIdentifier);
                }
            }
        }

        foreach (string s in idToRemove)
        {
            partyList.RemovePlayer(s);
        }
    }

    public PartyList GetNewPlayer(PartyList partyList)
    {
        PartyList proxy = InitializePartyList();
        PartyList iterationProxy = CreatePartyListCopy(proxy);

        for (int inMemPlayerIndex = 0; inMemPlayerIndex < partyList.players.Count; inMemPlayerIndex++)
        {
            for (int proxyPlayerIndex = 0; proxyPlayerIndex < iterationProxy.players.Count; proxyPlayerIndex++)
            {
                if (partyList.players[inMemPlayerIndex].internalIdentifier
                             .Equals(iterationProxy.players[proxyPlayerIndex].internalIdentifier))
                {
                    //Player was already here => Remove from proxy list ( potential new player list )
                    proxy.RemovePlayer(partyList.players[proxyPlayerIndex].internalIdentifier);
                }
            }
        }

        return proxy;
    }

    public PartyList GetLeavers(PartyList partyList)
    {
        PartyList copyOfMemory = CreatePartyListCopy(partyList);
        PartyList proxy = InitializePartyList();

        for (int proxyPlayerIndex = 0; proxyPlayerIndex < proxy.players.Count; proxyPlayerIndex++)
        {
            for (int memoryPlayerIndex = 0; memoryPlayerIndex < partyList.players.Count; memoryPlayerIndex++)
            {
                if (proxy.players[proxyPlayerIndex].internalIdentifier
                         .Equals(partyList.players[memoryPlayerIndex].internalIdentifier))
                {
                    //Player still in proxy = player still in party => Remove from copy of memory ( potential leavers list )
                    copyOfMemory.RemovePlayer(proxy.players[memoryPlayerIndex].internalIdentifier);
                }
            }
        }

        return copyOfMemory;
    }

    private Player ConvertCrossRealmMemberToPlayer(CrossRealmMember member)
    {
        string world = DalamudApi.DataManager.GetExcelSheet<World>()[(uint)member.HomeWorld].Name.ToString();
        string region = DCUtils.GetRegionSludFromDCId(DalamudApi.DataManager.GetExcelSheet<World>()[(uint)member.HomeWorld].DataCenter.Value.Region);
        string capitalizedJobName = "Best Rank";
        if (DalamudApi.DataManager.GetExcelSheet<ClassJob>().GetRowAt(member.ClassJobId).JobIndex != 0)
        {
            string jobName = DalamudApi.DataManager.GetExcelSheet<ClassJob>().GetRowAt(member.ClassJobId).NameEnglish.ToString();
            capitalizedJobName = string.Join("", jobName
                                                        .Split(' ')
                                                        .Select(word => char.ToUpper(word[0]) +
                                                                        word.Substring(1).ToLower()));
            capitalizedJobName = capitalizedJobName.Replace(" ", "");
        }
        PlayerClassJob job = ClassJobUtilsSingleton.Instance.GetFromName(capitalizedJobName);
        Player player = new Player(member.NameString, world, region, job);
        return player;
    }

    public static unsafe CrossRealmMember GetCrossRealmMember(CrossRealmMember* memberPointer)
    {
        if (memberPointer == null)
        {
            throw new ArgumentNullException(nameof(memberPointer));
        }

        return *memberPointer;
    }

    public static PartyList CreatePartyListCopy(PartyList partyList)
    {
        PartyList newPartyList = new();
        foreach (Player player in partyList.players)
        {
            newPartyList.AddPlayer(new Player(player.name, player.world, player.region, player.job));
        }
        return newPartyList;
    }
    
}
