using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using xiv.raid.OAuth;

namespace xiv.raid.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("xiv.raid configuration###config window")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        // Flags must be added or removed before Draw() is being called, or they won't apply
        // if (Configuration.IsConfigWindowMovable)
        // {
        //     Flags &= ~ImGuiWindowFlags.NoMove;
        // }
        // else
        // {
        //     Flags |= ImGuiWindowFlags.NoMove;
        // }
    }

    public override void Draw()
    {
        ImGui.Text("FFLogs config :");
        ImGui.BeginChild("fflogsConfig");
        // can't ref a property, so use a local copy
        var configClientId = Configuration.ClientId;
        if (ImGui.InputText("Client ID", ref configClientId, 200))
        {
            Configuration.ClientId = configClientId;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            Configuration.Save();
            ApiLink.Instance.RefreshConfig();
        }

        var configSecretId = Configuration.SecretId;
        if (ImGui.InputText("Secret ID", ref configSecretId, 200))
        {
            Configuration.SecretId = configSecretId;
            Configuration.Save();
            ApiLink.Instance.RefreshConfig();
        }
        ImGui.Text("Tomestone config :");
        var configTomestoneToken = Configuration.TomestoneToken;
        if (ImGui.InputText("Tomestone Token", ref configTomestoneToken, (uint)configTomestoneToken.Length))
        {
            Configuration.TomestoneToken = configTomestoneToken;
            Configuration.Save();
            ApiLink.Instance.RefreshConfig();
        }
        ImGui.EndChild();
    }
}
