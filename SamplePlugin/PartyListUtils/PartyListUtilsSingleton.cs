using System;
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

    public unsafe void RefreshPartyList(PartyList partyList)
    {
        
    }

    private Player ConvertCrossRealmMemberToPlayer(CrossRealmMember member)
    {
        string world = DalamudApi.DataManager.GetExcelSheet<World>()[(uint)member.HomeWorld].Name.ToString();
        string region = DCUtils.GetRegionSludFromDCId(DalamudApi.DataManager.GetExcelSheet<World>()[(uint)member.HomeWorld].DataCenter.Value.Region);
        string job = DalamudApi.DataManager.GetExcelSheet<ClassJob>().GetRowAt(member.ClassJobId).Name.ToString();
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
    
}
