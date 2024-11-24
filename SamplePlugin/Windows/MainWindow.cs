using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using xiv.raid.DataUtils;
using xiv.raid.DataUtils.FFLogsStructure;
using xiv.raid.DataUtils.TomestoneStructure;
using xiv.raid.PartyListUtils;
using xiv.raid.PartyListUtils.Structure;
using Task = System.Threading.Tasks.Task;

namespace xiv.raid.Windows;

public class MainWindow : Window, IDisposable
{
    private string imagePath;
    private Plugin Plugin;
    private string response = "Nothing";
    public PartyList partyList;
    private int _SelectedMenuIndex = -1;
    private bool _debugMode = false;
    private Vector2 _MinSize = new Vector2(500, 500);

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, string imagePath)
        : base("Papers Please !##main window", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = _MinSize,
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.imagePath = imagePath;
        Plugin = plugin;
        partyList = PartyListUtilsSingleton.Instance.InitializePartyList();
    }

    public void Dispose() { }

    public override void Draw()
    {
        PartyListUtilsSingleton.Instance.RefreshPartyList(partyList);
        
        Vector2 sideBarSize = new Vector2(200, 0);
        //Sidebar
        ImGui.BeginChild("Sidebar", sideBarSize, true);
        {
            if (ImGui.Selectable("Settings", _SelectedMenuIndex == -1))
            {
                _SelectedMenuIndex = -1;
            }

            ImGui.Separator();

            ImGui.Spacing();
            ImGui.Text("Your party list :");
            ImGui.Spacing();
            for (int i = 0; i < partyList.players.Count; i++)
            {
                if (ImGui.Selectable(partyList.players[i].name + " <\ue05d" + partyList.players[i].world + ">", _SelectedMenuIndex == i))
                {
                    _SelectedMenuIndex = i;
                }
            }
            
            ImGui.Separator();

            if (ImGui.Selectable("Credits", _SelectedMenuIndex == -2))
            {
                _SelectedMenuIndex = -2;
            }
            
            ImGui.Separator();
            
            ImGui.Spacing();
            ImGui.Text("Author : Mirror Marù");
            ImGui.Text("Dino Nuggies  <\ue05dZodiark>");
            ImGui.Spacing();
            var image = Plugin.TextureProvider.GetFromFile(imagePath).GetWrapOrDefault();
            if (image != null)
            {
                float availableWidth = ImGui.GetContentRegionAvail().X;
                float imageWidth = (float)0.25 * image.Width;
                
                ImGui.SetCursorPosX((availableWidth - imageWidth) / 2);
                ImGui.Image(image.ImGuiHandle, new Vector2((float)0.25*image.Width,(float)0.25*image.Height));
            }
            else
            {
                ImGui.Text("Image not found.");
            }
        }
        ImGui.EndChild();

        ImGui.SameLine();
        
        //Content
        ImGui.BeginChild("Content");
        {
            if (_SelectedMenuIndex == -2)
            {
                ImGui.Text("WIP ! ");
                ImGui.Text("//Credits FFLogs");
                ImGui.Text("//Credits Tomestone.gg");
                ImGui.Text("//Credits Dalamud");
                ImGui.Text("//Credits Goat");
            }
            if (_SelectedMenuIndex == -1)
            {
                ImGui.Text("Settings : ");
                ImGui.Spacing();
                if (ImGui.Button("API Keys configuration"))
                {
                    Plugin.ToggleConfigUI();
                }

                var refDebugMode = _debugMode;
                if (ImGui.Checkbox("Debug mode", ref refDebugMode))
                {
                    _debugMode = refDebugMode;
                }

                if (_debugMode)
                {
                    ImGui.Separator();
                    if (ImGui.Button("Manually refresh party list"))
                    {
                        PartyListUtilsSingleton.Instance.RefreshPartyList(partyList);
                    }
                    if (ImGui.Button("Display InMemory Party List"))
                    {
                        foreach (Player player in partyList.players)
                        {
                            DalamudApi.PluginLog.Debug(player.internalIdentifier );
                        }
                    }
                    if (ImGui.Button("Show all ClassJob"))
                    {
                        string allClassJobs = "ClassJobList = ";
                        if (ClassJobUtilsSingleton.Instance.ClassJobs.Count > 0)
                        {
                            foreach (PlayerClassJob classJob in ClassJobUtilsSingleton.Instance.ClassJobs)
                            {
                                allClassJobs += classJob.ClassJobAbbreviation + ", ";
                            }
                        }
                        DalamudApi.PluginLog.Debug(allClassJobs);
                    }

                    if (ImGui.Button("Show all ClassJobsName"))
                    {
                        foreach (PlayerClassJob playerClassJob in ClassJobUtilsSingleton.Instance.ClassJobs)
                        {
                            DalamudApi.PluginLog.Debug(playerClassJob.ClassJobName);
                        }
                    }
                    ImGui.Separator();
                }
            }
            if (_SelectedMenuIndex >= 0)
            {
                ImGui.Text(partyList.players[_SelectedMenuIndex].name + " infos : (" + partyList.players[_SelectedMenuIndex].job.ClassJobName + ")");
                ImGui.Spacing();
                if (!partyList.players[_SelectedMenuIndex].needFetching &&
                    !partyList.players[_SelectedMenuIndex].isFetching)
                {
                    if (partyList.players[_SelectedMenuIndex].noLogs)
                    {
                        ImGui.Text("No papers !");
                    }
                    foreach (Ranking ranking in partyList.players[_SelectedMenuIndex].papers)
                    {
                        string bestRankBlock = "";
                        if (partyList.players[_SelectedMenuIndex].job.ClassJobAbbreviation.Equals("BR"))
                            bestRankBlock += " as " + ClassJobUtilsSingleton.Instance.GetFromName(ranking.Spec)
                                                                           .ClassJobAbbreviation;
                        Vector4 parseColor;
                        switch (ranking.RankPercent)
                        {
                            case -1 : parseColor = new Vector4(181, 0, 0, 1); break;
                            case >= 0 and <25 : parseColor = new Vector4(0.4f, 0.4f, 0.4f, 1f); break;
                            case >= 25 and <50 : parseColor = new Vector4(0.118f, 1f, 0f, 1f); break;
                            case >= 50 and < 75 : parseColor = new Vector4(0f, 0.439f, 1f, 1f); break;
                            case >= 75 and < 95 : parseColor = new Vector4(0.639f, 0.208f, 0.933f, 1f); break;
                            case >= 95 and < 99 : parseColor = new Vector4(1f, 0.502f, 0f, 1f); break;
                            case >= 99 and < 100 : parseColor = new Vector4(0.886f, 0.408f, 0.659f, 1f); break;
                            case 100 : parseColor = new Vector4(0.898f, 0.8f, 0.502f, 1f); break;
                            default: parseColor = new Vector4(0, 0, 0, 1); break;
                        }
                        ImGui.Text(ranking.Encounter.Name + " : ");
                        ImGui.SameLine();
                        if (ranking.RankPercent >= 0)
                        {
                            ImGui.TextColored(parseColor, $"{ranking.RankPercent:F1}");
                            ImGui.SameLine(0, 0);
                            ImGui.TextUnformatted("%");
                            ImGui.SameLine();
                            ImGui.Text(bestRankBlock);
                        }
                        else
                        {
                            ImGui.TextColored(parseColor, "Not killed !");
                        }
                    }
                }
                else
                {
                    ImGui.Text("Loading ...");
                }
                ImGui.Spacing();
                if (ImGui.Button("Refresh Logs"))
                {
                    partyList.players[_SelectedMenuIndex].needFetching = true;
                }
                ImGui.SameLine();
                if (ImGui.BeginCombo("##dropdown", partyList.players[_SelectedMenuIndex].job.ClassJobAbbreviation))
                {
                    foreach (PlayerClassJob instanceClassJob in ClassJobUtilsSingleton.Instance.ClassJobs)
                    {
                        bool isSelected = (partyList.players[_SelectedMenuIndex].job.ClassJobAbbreviation
                                                    .Equals(instanceClassJob.ClassJobAbbreviation));
                        if (ImGui.Selectable(instanceClassJob.ClassJobAbbreviation, isSelected))
                        {
                            partyList.players[_SelectedMenuIndex].job = instanceClassJob;
                            partyList.players[_SelectedMenuIndex].needFetching = true;
                        }
                        
                        if (isSelected)
                        {
                            ImGui.SetItemDefaultFocus();
                        }
                    }
                    ImGui.EndChild();
                }
                ImGui.Separator();
                ImGui.Text("Tomestone infos : ");
                ImGui.Spacing();
                if (partyList.players[_SelectedMenuIndex].TomestoneData != null)
                {
                    if (partyList.players[_SelectedMenuIndex].TomestoneData.Encounters != null)
                    {
                        var encountersUltimates = partyList.players[_SelectedMenuIndex].TomestoneData
                                                           .Encounters?.Ultimates;
                        if (encountersUltimates != null)
                        {
                            foreach (UltimateEncounter ultimate in encountersUltimates)
                            {
                                ImGui.Text(ultimate.CompactName + " : ");
                                ImGui.SameLine();
                                if (ultimate.Achievement != null)
                                {
                                    ImGui.TextColored(new Vector4(0.118f, 1f, 0f, 1f), "Killed !");
                                }
                                else
                                {
                                    ImGui.TextColored(new Vector4(181, 0, 0, 1), "Not Killed !");
                                }
                            }
                        }
                    }

                    ImGui.Spacing();
                    ImGui.Text("Prog point : ");
                    ImGui.SameLine();
                    if (partyList.players[_SelectedMenuIndex].TomestoneData.progPoint != null)
                    {
                        ImGui.Text(partyList.players[_SelectedMenuIndex].TomestoneData.progPoint);
                    }
                    else
                    {
                        ImGui.Text("No prog point !");
                    }
                }
                if (ImGui.Button("Get Tomestone info"))
                {
                    partyList.players[_SelectedMenuIndex].needTomestoneFetching = true;
                }
            }
        }
        ImGui.EndChild();
        
        //FFlogs loop
        foreach (Player player in partyList.players)
        {
            // Check if fetching is needed and avoid simultaneous fetches
            if (player.needFetching && !player.isFetching)
            {
                // Mark the player as being fetched
                player.isFetching = true;

                Task.Run(async () =>
                {
                    try
                    {
                        // Fetch player info asynchronously
                        var responseJson = await LogsRequester.GetPlayerInfo(player.name.ToLower(), player.region.ToLower(), player.world.ToLower(), player.job.ClassJobName);
                        DalamudApi.PluginLog.Info("Getting info for player : "+responseJson);

                        // Parse the data and safely update the player's papers
                        var parsedPapers = FFLogsDataParser.GetFflogsResponseFromJson(responseJson);

                        lock (player) // Ensure thread safety
                        {
                            player.papers.Clear();
                            player.papers.AddRange(parsedPapers);
                            foreach (Ranking ranking in parsedPapers)
                            {
                                DalamudApi.PluginLog.Debug("Added for "+player.name + " : " + ranking.Encounter.Name + " (" + ranking.RankPercent + "%)");
                            }

                            player.noLogs = player.papers.Count == 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        DalamudApi.PluginLog.Error($"Error fetching info for player {player.name} ({player.world}/{player.region}): {ex}");
                    }
                    finally
                    {
                        // Reset fetching flags
                        player.needFetching = false;
                        player.isFetching = false;
                    }
                });
            }
        }
        
        //Tomestone loop
        foreach (Player player in partyList.players)
        {
            if (player.needTomestoneFetching && !player.isTomestoneFetching)
            {
                player.isTomestoneFetching = true;

                Task.Run(async () =>
                {
                    try
                    {
                        var responseJson = await TomestoneRequester.GetTomestoneInfo(player.world, player.name);
                        DalamudApi.PluginLog.Info("Getting tomestone info for player : " + responseJson);

                        var tomestoneData = TomestoneDataParser.TomestoneDataFromJson(responseJson);

                        lock (player)
                        {
                            player.TomestoneData = tomestoneData;
                        }
                    }
                    catch (Exception e)
                    {
                        DalamudApi.PluginLog.Debug("Error fetching info for player");
                    } finally
                    {
                        player.needTomestoneFetching = false;
                        player.isTomestoneFetching = false;
                    }
                });
            }
        }
    }
}
