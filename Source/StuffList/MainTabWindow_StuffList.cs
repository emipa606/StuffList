using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace StuffList;

public class MainTabWindow_StuffList : MainTabWindow
{
    // Display variables
    private const float HeaderHeight = 50f;
    private const float RowHeight = 30f;
    private const float IconWidth = 29f;
    private const float LabelWidth = 200f;

    // Data storage

    internal static readonly IEnumerable<ThingDef> metallicStuff =
        from thing in DefDatabase<ThingDef>.AllDefsListForReading
        where thing.category == ThingCategory.Item
              && thing.stuffProps != null
              && !thing.stuffProps.categories.NullOrEmpty()
              && thing.stuffProps.categories.Contains(StuffCategoryDefOf.Metallic)
              && thing.stuffProps.statFactors != null
        select thing;

    internal static readonly IEnumerable<ThingDef> woodyStuff =
        from thing in DefDatabase<ThingDef>.AllDefsListForReading
        where thing.category == ThingCategory.Item
              && thing.stuffProps != null
              && !thing.stuffProps.categories.NullOrEmpty()
              && thing.stuffProps.categories.Contains(StuffCategoryDefOf.Woody)
              && thing.stuffProps.statFactors != null
        select thing;

    internal static readonly IEnumerable<ThingDef> stonyStuff =
        from thing in DefDatabase<ThingDef>.AllDefsListForReading
        where thing.category == ThingCategory.Item
              && thing.stuffProps != null
              && !thing.stuffProps.categories.NullOrEmpty()
              && thing.stuffProps.categories.Contains(StuffCategoryDefOf.Stony)
              && thing.stuffProps.statFactors != null
        select thing;

    internal static readonly IEnumerable<ThingDef> fabricStuff =
        from thing in DefDatabase<ThingDef>.AllDefsListForReading
        where thing.category == ThingCategory.Item
              && thing.stuffProps != null
              && !thing.stuffProps.categories.NullOrEmpty()
              && thing.stuffProps.categories.Contains(StuffCategoryDefOf.Fabric)
              && thing.stuffProps.statFactors != null
        select thing;

    internal static readonly IEnumerable<ThingDef> leatheryStuff =
        from thing in DefDatabase<ThingDef>.AllDefsListForReading
        where thing.category == ThingCategory.Item
              && thing.stuffProps != null
              && !thing.stuffProps.categories.NullOrEmpty()
              && thing.stuffProps.categories.Contains(StuffCategoryDefOf.Leathery)
              && thing.stuffProps.statFactors != null
        select thing;


    internal static readonly IEnumerable<ThingDef> allStuff =
        from thing in DefDatabase<ThingDef>.AllDefsListForReading
        where thing.IsStuff
        select thing;

    public static Thread thread;

    private readonly Color baseColor = GUI.color;
    private bool isDirty = true;

    public Vector2 scrollPosition = Vector2.zero;
    private bool showFabric = true;
    private bool showLeathery = true;

    private bool showMetallic = true;
    private bool showStony = true;
    private bool showWoody = true;
    private StatDef sortDef;
    private string sortOrder;

    private string sortProperty = "label";
    private Source sortSource = Source.Name;
    private int statCount;
    private float statWidth = 80f;

    internal IEnumerable<ThingDef> stuff = Enumerable.Empty<ThingDef>();

    private int stuffCount;
    private Dictionary<ThingDef, int> stuffCountDictionary;

    private float tableHeight;

    public override Vector2 InitialSize
    {
        get
        {
            statCount = 16;
            if (StuffList.SoftWarmBedsLoaded)
            {
                statCount++;
            }

            return new Vector2(
                UI.screenWidth * 0.95f,
                UI.screenHeight * 0.7f);
        }
    }

    public override void PreOpen()
    {
        base.PreOpen();
        stuffCountDictionary = new Dictionary<ThingDef, int>();
        triggerThread();
        isDirty = true;
    }

    public override void DoWindowContents(Rect rect)
    {
        statWidth = (InitialSize.x - LabelWidth - (IconWidth * 2)) / statCount;
        if (isDirty)
        {
            UpdateList();
        }

        if (GenTicks.TicksGame % GenTicks.TickRareInterval == 0)
        {
            triggerThread();
        }

        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.UpperLeft;
        GUI.color = Color.white;

        var currentX = rect.x;

        var showMetallicOld = showMetallic;
        var showWoodyOld = showWoody;
        var showStonyOld = showStony;
        var showFabricOld = showFabric;
        var showLeatheryOld = showLeathery;

        PrintAutoCheckbox("StuffList.Metallic".Translate(), ref showMetallic, ref currentX, ref rect);
        PrintAutoCheckbox("StuffList.Woody".Translate(), ref showWoody, ref currentX, ref rect);
        PrintAutoCheckbox("StuffList.Stony".Translate(), ref showStony, ref currentX, ref rect);
        PrintAutoCheckbox("StuffList.Fabric".Translate(), ref showFabric, ref currentX, ref rect);
        PrintAutoCheckbox("StuffList.Leathery".Translate(), ref showLeathery, ref currentX, ref rect);

        if (showMetallicOld != showMetallic || showWoodyOld != showWoody || showStonyOld != showStony
            || showFabricOld != showFabric || showLeatheryOld != showLeathery)
        {
            isDirty = true;
        }

        // HEADERS

        var colHeaders = Enumerable.Empty<ColDef>();
        colHeaders = colHeaders.Append(new ColDef("StuffList.Base.MarketValue".Translate(), StatDefOf.MarketValue,
            Source.Bases));
        colHeaders = colHeaders.Append(new ColDef("StuffList.Base.Mass".Translate(), StatDefOf.Mass,
            Source.Bases));
        colHeaders = colHeaders.Append(new ColDef("StuffList.Base.ArmorSharp".Translate(),
            StatDefOf.StuffPower_Armor_Sharp, Source.Bases));
        colHeaders = colHeaders.Append(new ColDef("StuffList.Base.ArmorBlunt".Translate(),
            StatDefOf.StuffPower_Armor_Blunt, Source.Bases));
        colHeaders = colHeaders.Append(new ColDef("StuffList.Base.ArmorHeat".Translate(),
            StatDefOf.StuffPower_Armor_Heat, Source.Bases));
        colHeaders = colHeaders.Append(new ColDef("StuffList.Base.InsulCold".Translate(),
            StatDefOf.StuffPower_Insulation_Cold, Source.Bases));
        colHeaders = colHeaders.Append(new ColDef("StuffList.Base.InsulHot".Translate(),
            StatDefOf.StuffPower_Insulation_Heat, Source.Bases));
        colHeaders = colHeaders.Append(new ColDef("StuffList.Base.DmgSharp".Translate(),
            StatDefOf.SharpDamageMultiplier, Source.Bases));
        colHeaders = colHeaders.Append(new ColDef("StuffList.Base.DmgBlunt".Translate(),
            StatDefOf.BluntDamageMultiplier, Source.Bases));
        colHeaders =
            colHeaders.Append(new ColDef("StuffList.Offset.Beauty".Translate(), StatDefOf.Beauty, Source.Offset));
        colHeaders = colHeaders.Append(new ColDef("StuffList.Factor.MaxHP".Translate(), StatDefOf.MaxHitPoints,
            Source.Factors));
        colHeaders =
            colHeaders.Append(new ColDef("StuffList.Factor.Beauty".Translate(), StatDefOf.Beauty, Source.Factors));
        colHeaders = colHeaders.Append(new ColDef("StuffList.Factor.WorkMake".Translate(), StatDefOf.WorkToMake,
            Source.Factors));
        colHeaders = colHeaders.Append(new ColDef("StuffList.Factor.WorkBuild".Translate(), StatDefOf.WorkToBuild,
            Source.Factors));
        colHeaders = colHeaders.Append(new ColDef("StuffList.Factor.Flammability".Translate(), StatDefOf.Flammability,
            Source.Factors));
        colHeaders = colHeaders.Append(new ColDef("StuffList.Factor.MeleeCooldown".Translate(),
            StatDefOf.MeleeWeapon_CooldownMultiplier, Source.Factors));
        if (StuffList.SoftWarmBedsLoaded)
        {
            colHeaders = colHeaders.Append(new ColDef("StuffList.Base.TextileSoftness".Translate(),
                StatDef.Named("Textile_Softness"), Source.BasesZero));
        }

        rect.y += HeaderHeight;
        GUI.BeginGroup(rect);
        tableHeight = stuffCount * RowHeight;
        var inRect = new Rect(0, 0, rect.width - 4, tableHeight + 100);
        var num = 0;
        var ww = IconWidth;
        GUI.color = new Color(1f, 1f, 1f, 0.2f);
        Widgets.DrawLineHorizontal(0, HeaderHeight, inRect.width);
        GUI.color = Color.white;
        PrintCellSort("label", "Name", ww, LabelWidth);
        ww += LabelWidth;
        PrintCellSort("amount", "Amount", ww, statWidth);
        ww += statWidth;
        foreach (var h in colHeaders)
        {
            PrintCellSort(h.statDef.defName, h.statDef, h.source, h.label, ww);
            ww += statWidth;
        }

        var scrollRect = new Rect(rect.x, rect.y, rect.width, rect.height);
        Widgets.BeginScrollView(scrollRect, ref scrollPosition, inRect);
        foreach (var thing in stuff)
        {
            DrawRow(thing, num, inRect.width);
            num++;
        }

        Widgets.EndScrollView();
        GUI.EndGroup();
    }

    private void DrawRow(ThingDef t, int num, float w)
    {
        DrawCommon(num, w);
        var ww = DrawIcon(num, t);
        PrintCell(t.LabelCap, num, ww, LabelWidth, t.description);
        ww += LabelWidth;
        PrintCell(stuffCountDictionary.ContainsKey(t) ? stuffCountDictionary[t].ToString() : "0", num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.statBases.GetStatValueFromList(StatDefOf.MarketValue, 1), 1);
        PrintCell(t.statBases.GetStatValueFromList(StatDefOf.MarketValue, 1).ToStringMoney(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.statBases.GetStatValueFromList(StatDefOf.Mass, 1), 0.5f, true);
        PrintCell(t.statBases.GetStatValueFromList(StatDefOf.Mass, 1).ToStringMass(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.statBases.GetStatValueFromList(StatDefOf.StuffPower_Armor_Sharp, 1), 1);
        PrintCell(t.statBases.GetStatValueFromList(StatDefOf.StuffPower_Armor_Sharp, 1).ToStringPercent(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.statBases.GetStatValueFromList(StatDefOf.StuffPower_Armor_Blunt, 1), 1);
        PrintCell(t.statBases.GetStatValueFromList(StatDefOf.StuffPower_Armor_Blunt, 1).ToStringPercent(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.statBases.GetStatValueFromList(StatDefOf.StuffPower_Armor_Heat, 1), 1);
        PrintCell(t.statBases.GetStatValueFromList(StatDefOf.StuffPower_Armor_Heat, 1).ToStringPercent(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.statBases.GetStatValueFromList(StatDefOf.StuffPower_Insulation_Cold, 0), 0);
        PrintCell(t.statBases.GetStatValueFromList(StatDefOf.StuffPower_Insulation_Cold, 0).ToStringTemperatureOffset(),
            num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.statBases.GetStatValueFromList(StatDefOf.StuffPower_Insulation_Heat, 0), 0);
        PrintCell(t.statBases.GetStatValueFromList(StatDefOf.StuffPower_Insulation_Heat, 0).ToStringTemperatureOffset(),
            num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.statBases.GetStatValueFromList(StatDefOf.SharpDamageMultiplier, 1), 1);
        PrintCell(t.statBases.GetStatValueFromList(StatDefOf.SharpDamageMultiplier, 1).ToStringPercent(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.statBases.GetStatValueFromList(StatDefOf.BluntDamageMultiplier, 1), 1);
        PrintCell(t.statBases.GetStatValueFromList(StatDefOf.BluntDamageMultiplier, 1).ToStringPercent(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.stuffProps.statOffsets.GetStatOffsetFromList(StatDefOf.Beauty), 0);
        PrintCell(t.stuffProps.statOffsets.GetStatOffsetFromList(StatDefOf.Beauty) + "", num, ww, statWidth,
            "Beauty = ((Base * Factor) + Offset) * Quality");
        ww += statWidth;
        GUI.color = valueColor(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.MaxHitPoints), 1);
        PrintCell(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.MaxHitPoints).ToStringPercent(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.Beauty), 1);
        PrintCell(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.Beauty).ToStringPercent(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.WorkToMake), 1, true);
        PrintCell(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.WorkToMake).ToStringPercent(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.WorkToBuild), 1, true);
        PrintCell(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.WorkToBuild).ToStringPercent(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.Flammability), 1, true);
        PrintCell(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.Flammability).ToStringPercent(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.MeleeWeapon_CooldownMultiplier),
            1, true);
        PrintCell
        (t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.MeleeWeapon_CooldownMultiplier).ToStringPercent(),
            num,
            ww);
        if (StuffList.SoftWarmBedsLoaded)
        {
            ww += statWidth;
            GUI.color = valueColor(t.statBases.GetStatValueFromList(StatDef.Named("Textile_Softness"), 0),
                0);
            PrintCell
            (t.statBases.GetStatValueFromList(StatDef.Named("Textile_Softness"), 0).ToStringPercent(),
                num,
                ww);
        }

        GUI.color = baseColor;
    }

    private Color valueColor(float inputValue, float referenceValue, bool inverted = false)
    {
        if (Math.Abs(inputValue - referenceValue) < 0.001f)
        {
            return baseColor;
        }

        if (inputValue > referenceValue)
        {
            return inverted ? Color.red : Color.green;
        }

        return inverted ? Color.green : Color.red;
    }

    private float DrawIcon(int rowNum, ThingDef t)
    {
        var icoRect = new Rect(0, RowHeight * rowNum, IconWidth, IconWidth);
        Widgets.ThingIcon(icoRect, t);
        return IconWidth + 2f;
    }

    private void PrintCell(string content, int rowNum, float x, float width = 0, string tooltip = "")
    {
        if (width == 0)
        {
            width = statWidth;
        }

        var tmpRec = new Rect(x, (RowHeight * rowNum) + 3, width, RowHeight - 3);
        Widgets.Label(tmpRec, content);
        if (!string.IsNullOrEmpty(tooltip))
        {
            TooltipHandler.TipRegion(tmpRec, tooltip);
        }
    }

    private void PrintCellSort(string property, StatDef statDef, Source source, string content, float x,
        float width = 0)
    {
        if (width == 0)
        {
            width = statWidth;
        }

        var tmpRec = new Rect(x + 2, 2, width - 2, HeaderHeight - 2);
        Text.Font = GameFont.Tiny;
        Widgets.Label(tmpRec, content);
        TooltipHandler.TipRegion(tmpRec, statDef.description);
        Text.Font = GameFont.Small;
        if (Mouse.IsOver(tmpRec))
        {
            GUI.DrawTexture(tmpRec, TexUI.HighlightTex);
        }

        if (Widgets.ButtonInvisible(tmpRec))
        {
            if (sortProperty == property && sortSource == source)
            {
                sortOrder = sortOrder == "ASC" ? "DESC" : "ASC";
            }
            else
            {
                sortDef = statDef;
                sortProperty = property;
                sortSource = source;
            }

            isDirty = true;
        }

        if (sortProperty != property)
        {
            return;
        }

        var texture2D = sortOrder == "ASC"
            ? ContentFinder<Texture2D>.Get("UI/Icons/Sorting")
            : ContentFinder<Texture2D>.Get("UI/Icons/SortingDescending");
        var p = new Rect(tmpRec.xMax - texture2D.width - 30, tmpRec.yMax - texture2D.height - 1, texture2D.width,
            texture2D.height);
        GUI.DrawTexture(p, texture2D);
    }

    private void PrintCellSort(string property, string content, float x, float width = 0)
    {
        if (width == 0)
        {
            width = statWidth;
        }

        var tmpRec = new Rect(x + 2, 2, width - 2, HeaderHeight - 2);

        Widgets.Label(tmpRec, content);
        if (Mouse.IsOver(tmpRec))
        {
            GUI.DrawTexture(tmpRec, TexUI.HighlightTex);
        }

        if (Widgets.ButtonInvisible(tmpRec))
        {
            if (sortProperty == property)
            {
                sortOrder = sortOrder == "ASC" ? "DESC" : "ASC";
            }
            else
            {
                sortProperty = property;
                sortSource = property == "label" ? Source.Name : Source.Amount;
            }

            isDirty = true;
        }

        if (sortProperty != property)
        {
            return;
        }

        var texture2D = sortOrder == "ASC"
            ? ContentFinder<Texture2D>.Get("UI/Icons/Sorting")
            : ContentFinder<Texture2D>.Get("UI/Icons/SortingDescending");
        var p = new Rect(tmpRec.xMax - texture2D.width - 30, tmpRec.yMax - texture2D.height - 1, texture2D.width,
            texture2D.height);
        GUI.DrawTexture(p, texture2D);
    }

    private void DrawCommon(int num, float w)
    {
        var fnum = num;
        if (num == -1)
        {
            fnum = 0;
        }

        GUI.color = new Color(1f, 1f, 1f, 0.2f);
        Widgets.DrawLineHorizontal(0, RowHeight * (fnum + 1), w);
        GUI.color = Color.white;
        var rowRect = new Rect(0, RowHeight * num, w, RowHeight);
        if (num <= -1)
        {
            return;
        }

        if (Mouse.IsOver(rowRect))
        {
            GUI.DrawTexture(rowRect, TexUI.HighlightTex);
        }
    }

    private void UpdateList()
    {
        stuff = Enumerable.Empty<ThingDef>();
        if (showMetallic)
        {
            stuff = stuff.Union(metallicStuff);
        }

        if (showWoody)
        {
            stuff = stuff.Union(woodyStuff);
        }

        if (showStony)
        {
            stuff = stuff.Union(stonyStuff);
        }

        if (showFabric)
        {
            stuff = stuff.Union(fabricStuff);
        }

        if (showLeathery)
        {
            stuff = stuff.Union(leatheryStuff);
        }

        stuffCount = stuff.Count();

        UpdateListSorting();

        isDirty = false;
    }

    private void UpdateListSorting()
    {
        switch (sortSource)
        {
            case Source.Name:
                stuff = sortOrder == "DESC"
                    ? stuff.OrderByDescending(o => o.label)
                    : stuff.OrderBy(o => o.label);
                break;
            case Source.Amount:
                stuff = sortOrder == "DESC"
                    ? stuff.OrderByDescending(o => stuffCountDictionary.ContainsKey(o) ? stuffCountDictionary[o] : 0)
                    : stuff.OrderBy(o => stuffCountDictionary.ContainsKey(o) ? stuffCountDictionary[o] : 0);
                break;
            case Source.Bases:
                stuff = sortOrder == "DESC"
                    ? stuff.OrderByDescending(o => o.statBases.GetStatValueFromList(sortDef, 1))
                    : stuff.OrderBy(o => o.statBases.GetStatValueFromList(sortDef, 1));
                break;
            case Source.BasesZero:
                stuff = sortOrder == "DESC"
                    ? stuff.OrderByDescending(o => o.statBases.GetStatValueFromList(sortDef, 0))
                    : stuff.OrderBy(o => o.statBases.GetStatValueFromList(sortDef, 0));
                break;
            case Source.Factors:
                stuff = sortOrder == "DESC"
                    ? stuff.OrderByDescending(o => o.stuffProps.statFactors.GetStatFactorFromList(sortDef))
                    : stuff.OrderBy(o => o.stuffProps.statFactors.GetStatFactorFromList(sortDef));
                break;
            case Source.Offset:
                stuff = sortOrder == "DESC"
                    ? stuff.OrderByDescending(o => o.stuffProps.statOffsets.GetStatOffsetFromList(sortDef))
                    : stuff.OrderBy(o => o.stuffProps.statOffsets.GetStatOffsetFromList(sortDef));
                break;
        }
    }

    private void triggerThread()
    {
        if (thread?.IsAlive == true)
        {
            return;
        }

        thread = new Thread(updateCurrentStuff);
        thread.Start();
    }

    private void updateCurrentStuff()
    {
        var returnDictionary = new Dictionary<ThingDef, int>();
        foreach (var thingDef in allStuff)
        {
            returnDictionary[thingDef] = Find.CurrentMap.resourceCounter.GetCount(thingDef);
        }

        stuffCountDictionary = returnDictionary;
        isDirty = true;
    }

    private void PrintAutoCheckbox(string text, ref bool value, ref float currentX, ref Rect rect,
        bool defaultValue = false)
    {
        var textWidth = Text.CalcSize(text).x + 25f;
        Widgets.CheckboxLabeled(new Rect(currentX, rect.y, textWidth, 30), text, ref value, defaultValue);
        currentX += textWidth + 25f;
    }

    private enum Source : byte
    {
        Bases,
        BasesZero,
        Factors,
        Offset,
        Name,
        Amount
    }

    private struct ColDef
    {
        public readonly string label;
        [UsedImplicitly] private string property;
        public readonly StatDef statDef;
        public readonly Source source;

        public ColDef(string label, string property, Source source)
        {
            this.label = label;
            this.property = property;
            this.source = source;
            statDef = StatDefOf.MarketValue;
        }

        public ColDef(string label, StatDef statDef, Source source)
        {
            this.label = label;
            this.source = source;
            this.statDef = statDef;
            property = "";
        }
    }
}