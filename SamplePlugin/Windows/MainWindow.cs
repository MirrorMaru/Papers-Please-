using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using xiv.raid.DataUtils;
using xiv.raid.DataUtils.FFLogsStructure;
using xiv.raid.PartyListUtils;
using xiv.raid.PartyListUtils.Structure;
using Task = System.Threading.Tasks.Task;

namespace xiv.raid.Windows;

public class MainWindow : Window, IDisposable
{
    private string imagePath;
    private Plugin Plugin;
    private string response = "Nothing";
    private PartyList _partyList;
    private int _SelectedMenuIndex = -1;
    private Vector2 _MinSize = new Vector2(400, 500);

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, string imagePath)
        : base("xiv.raid##main window", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = _MinSize,
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.imagePath = imagePath;
        Plugin = plugin;
        _partyList = PartyListUtilsSingleton.Instance.InitializePartyList();
    }

    public void Dispose() { }

    public override void Draw()
    {
        Vector2 sideBarSize = new Vector2(150, 0);
        //Sidebar
        ImGui.BeginChild("Sidebar", sideBarSize, true);
        {
            if (ImGui.Selectable("Settings", _SelectedMenuIndex == -1))
            {
                _SelectedMenuIndex = -1;
            }

            ImGui.Separator();

            for (int i = 0; i < _partyList.players.Count; i++)
            {
                if (ImGui.Selectable(_partyList.players[i].name, _SelectedMenuIndex == i))
                {
                    _SelectedMenuIndex = i;
                }
            }
        }
        ImGui.EndChild();

        ImGui.SameLine();
        
        //Content
        ImGui.BeginChild("Content");
        {
            if (_SelectedMenuIndex == -1)
            {
                ImGui.Text("Settings : ");
                ImGui.Spacing();
                if (ImGui.Button("Show Settings"))
                {
                    Plugin.ToggleConfigUI();
                }
                if (ImGui.Button("Manually refresh party list"))
                {
                    _partyList = PartyListUtilsSingleton.Instance.InitializePartyList();
                }
            }
            if (_SelectedMenuIndex >= 0)
            {
                ImGui.Text(_partyList.players[_SelectedMenuIndex].name + " infos : ");
                ImGui.Spacing();
                if (!_partyList.players[_SelectedMenuIndex].needFetching &&
                    !_partyList.players[_SelectedMenuIndex].isFetching)
                {
                    if (_partyList.players[_SelectedMenuIndex].papers.Count == 0)
                    {
                        ImGui.Text("No papers available yet !");
                    }
                    foreach (Ranking ranking in _partyList.players[_SelectedMenuIndex].papers)
                    {
                    
                        ImGui.Text(ranking.Encounter.Name + " : " + $"{ranking.RankPercent:F1}");
                    }
                }
                else
                {
                    ImGui.Text("Loading ...");
                }
                ImGui.Spacing();
                if (ImGui.Button("Refresh Logs"))
                {
                    _partyList.players[_SelectedMenuIndex].needFetching = true;
                }
                ImGui.Text("Tomestone infos : ");
                ImGui.Spacing();
                ImGui.Text("WIP");
            }
            ImGui.Spacing();
            ImGui.Text("Author : Mirror Marù");
            ImGui.Spacing();
            var image = Plugin.TextureProvider.GetFromFile(imagePath).GetWrapOrDefault();
            if (image != null)
            {
                ImGui.Image(image.ImGuiHandle, new Vector2((float)0.25*image.Width,(float)0.25*image.Height));
            }
            else
            {
                ImGui.Text("Image not found.");
            }
        }
        ImGui.EndChild();
        
        foreach (Player player in _partyList.players)
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
                        var responseJson = await LogsRequester.GetPlayerInfo(player.name.ToLower(), player.region.ToLower(), player.world.ToLower());
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
    }
}

/*

ImGui.Text("Player's parses : ");
for (int i = 0; i < _partyList.players.Count; i++)
{
    Player p = _partyList.players[i];
    string firstLine = p.name + "(" + p.job + ") (" + p.world + ") :";
    string rankingblock = "";
    if (p.isFetching) rankingblock = "Loading...";
    else
    {
        foreach (Ranking ranking in p.papers)
        {
            rankingblock += ranking.Encounter.Name + " : " + ranking.RankPercent + "%\n";
        }
    }

    Vector2 boxSize = new Vector2(6*firstLine.Length, 100);
    Vector2 cursorPos = ImGui.GetCursorScreenPos();

    var drawList = ImGui.GetWindowDrawList();
    drawList.AddRectFilled(cursorPos, cursorPos + boxSize, ImGui.GetColorU32(ImGuiCol.Button));

    ImGui.SetCursorScreenPos(cursorPos + new Vector2(10, 0));
    ImGui.Text(firstLine + "\n" + rankingblock);

    ImGui.SetCursorScreenPos(cursorPos + new Vector2(boxSize.X, 0)); // Move to next position
    if ((i + 1) % 2 != 0 && i != _partyList.players.Count - 1)
    {
        ImGui.SameLine(); // Stay on the same row
    }
}

ImGui.Spacing();
float rows = (float)Math.Ceiling(_partyList.players.Count / (float)2);    // Calculate number of rows
ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (rows *100) + 10); // Adjust Y position

*/
