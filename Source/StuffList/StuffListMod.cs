using Mlie;
using UnityEngine;
using Verse;

namespace StuffList;

[StaticConstructorOnStartup]
internal class StuffListMod : Mod
{
    /// <summary>
    ///     The instance of the settings to be read by the mod
    /// </summary>
    public static StuffListMod Instance;

    private static string currentVersion;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="content"></param>
    public StuffListMod(ModContentPack content) : base(content)
    {
        Instance = this;
        Settings = GetSettings<StuffListSettings>();
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    /// <summary>
    ///     The instance-settings for the mod
    /// </summary>
    internal StuffListSettings Settings { get; }

    /// <summary>
    ///     The title for the mod-settings
    /// </summary>
    /// <returns></returns>
    public override string SettingsCategory()
    {
        return "Stuff List";
    }

    /// <summary>
    ///     The settings-window
    ///     For more info: https://rimworldwiki.com/wiki/Modding_Tutorials/ModSettings
    /// </summary>
    /// <param name="rect"></param>
    public override void DoSettingsWindowContents(Rect rect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(rect);
        listingStandard.Label("StLi.ScreenWidth".Translate(Settings.ScreenWidth.ToStringPercent()));
        Settings.ScreenWidth = Widgets.HorizontalSlider(listingStandard.GetRect(20), Settings.ScreenWidth, 0.01f, 1f,
            false, Settings.ScreenWidth.ToStringPercent());
        if (currentVersion != null)
        {
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label("StLi.CurrentModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listingStandard.End();
    }
}