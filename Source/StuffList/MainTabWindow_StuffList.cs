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

    private static readonly IEnumerable<ThingDef> metallicStuff =
        from thing in DefDatabase<ThingDef>.AllDefsListForReading
        where thing.category == ThingCategory.Item
              && thing.stuffProps != null
              && !thing.stuffProps.categories.NullOrEmpty()
              && thing.stuffProps.categories.Contains(StuffCategoryDefOf.Metallic)
              && thing.stuffProps.statFactors != null
        select thing;

    private static readonly IEnumerable<ThingDef> woodyStuff =
        from thing in DefDatabase<ThingDef>.AllDefsListForReading
        where thing.category == ThingCategory.Item
              && thing.stuffProps != null
              && !thing.stuffProps.categories.NullOrEmpty()
              && thing.stuffProps.categories.Contains(StuffCategoryDefOf.Woody)
              && thing.stuffProps.statFactors != null
        select thing;

    private static readonly IEnumerable<ThingDef> stonyStuff =
        from thing in DefDatabase<ThingDef>.AllDefsListForReading
        where thing.category == ThingCategory.Item
              && thing.stuffProps != null
              && !thing.stuffProps.categories.NullOrEmpty()
              && thing.stuffProps.categories.Contains(StuffCategoryDefOf.Stony)
              && thing.stuffProps.statFactors != null
        select thing;

    private static readonly IEnumerable<ThingDef> fabricStuff =
        from thing in DefDatabase<ThingDef>.AllDefsListForReading
        where thing.category == ThingCategory.Item
              && thing.stuffProps != null
              && !thing.stuffProps.categories.NullOrEmpty()
              && thing.stuffProps.categories.Contains(StuffCategoryDefOf.Fabric)
              && thing.stuffProps.statFactors != null
        select thing;

    private static readonly IEnumerable<ThingDef> leatheryStuff =
        from thing in DefDatabase<ThingDef>.AllDefsListForReading
        where thing.category == ThingCategory.Item
              && thing.stuffProps != null
              && !thing.stuffProps.categories.NullOrEmpty()
              && thing.stuffProps.categories.Contains(StuffCategoryDefOf.Leathery)
              && thing.stuffProps.statFactors != null
        select thing;


    private static readonly IEnumerable<ThingDef> allStuff =
        from thing in DefDatabase<ThingDef>.AllDefsListForReading
        where thing.IsStuff
        select thing;

    private static Thread thread;

    private readonly Color baseColor = GUI.color;
    private bool isDirty = true;

    private Vector2 scrollPosition = Vector2.zero;
    private string searchText;
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

    private IEnumerable<ThingDef> stuff = [];

    private int stuffCount;
    private Dictionary<ThingDef, int> stuffCountDictionary;

    private float tableHeight;

    public override Vector2 InitialSize
    {
        get
        {
            statCount = 18;
            if (StuffList.SoftWarmBedsLoaded)
            {
                statCount++;
            }

            return new Vector2(
                UI.screenWidth * StuffListMod.Instance.Settings.ScreenWidth,
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
            updateList();
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

        printAutoCheckbox("StuffList.Metallic".Translate(), ref showMetallic, ref currentX, ref rect);
        printAutoCheckbox("StuffList.Woody".Translate(), ref showWoody, ref currentX, ref rect);
        printAutoCheckbox("StuffList.Stony".Translate(), ref showStony, ref currentX, ref rect);
        printAutoCheckbox("StuffList.Fabric".Translate(), ref showFabric, ref currentX, ref rect);
        printAutoCheckbox("StuffList.Leathery".Translate(), ref showLeathery, ref currentX, ref rect);
        searchText = Widgets.TextEntryLabeled(new Rect(currentX, rect.y, 200, 30), "StuffList.Search".Translate(),
            searchText);

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
        printCellSort("label", "Name", ww, LabelWidth);
        ww += LabelWidth;
        printCellSort("amount", "StuffList.Amount".Translate(), ww, statWidth);
        ww += statWidth;
        printCellSort("stacksize", "StuffList.Stacksize".Translate(), ww, statWidth);
        ww += statWidth;
        foreach (var h in colHeaders)
        {
            printCellSort(h.statDef.defName, h.statDef, h.source, h.label, ww);
            ww += statWidth;
        }

        var scrollRect = new Rect(rect.x, rect.y, rect.width, rect.height);
        Widgets.BeginScrollView(scrollRect, ref scrollPosition, inRect);
        var stuffFiltered = stuff.ToList();
        if (!string.IsNullOrEmpty(searchText))
        {
            stuffFiltered = stuffFiltered.Where(thingDef => thingDef.label.ToLower().Contains(searchText.ToLower()))
                .ToList();
        }

        foreach (var thing in stuffFiltered)
        {
            drawRow(thing, num, inRect.width);
            num++;
        }

        Widgets.EndScrollView();
        GUI.EndGroup();
    }

    private void drawRow(ThingDef t, int num, float w)
    {
        drawCommon(num, w);
        var ww = drawIcon(num, t);
        printCell(t.LabelCap, num, ww, LabelWidth, t.description);
        ww += LabelWidth;
        printCell(stuffCountDictionary.TryGetValue(t, out var value) ? value.ToString() : "0", num, ww);
        ww += statWidth;
        printCell(t.stackLimit.ToString(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.statBases.GetStatValueFromList(StatDefOf.MarketValue, 1), 1);
        printCell(t.statBases.GetStatValueFromList(StatDefOf.MarketValue, 1).ToStringMoney(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.statBases.GetStatValueFromList(StatDefOf.Mass, 1), 0.5f, true);
        printCell(t.statBases.GetStatValueFromList(StatDefOf.Mass, 1).ToStringMass(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.statBases.GetStatValueFromList(StatDefOf.StuffPower_Armor_Sharp, 1), 1);
        printCell(t.statBases.GetStatValueFromList(StatDefOf.StuffPower_Armor_Sharp, 1).ToStringPercent(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.statBases.GetStatValueFromList(StatDefOf.StuffPower_Armor_Blunt, 1), 1);
        printCell(t.statBases.GetStatValueFromList(StatDefOf.StuffPower_Armor_Blunt, 1).ToStringPercent(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.statBases.GetStatValueFromList(StatDefOf.StuffPower_Armor_Heat, 1), 1);
        printCell(t.statBases.GetStatValueFromList(StatDefOf.StuffPower_Armor_Heat, 1).ToStringPercent(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.statBases.GetStatValueFromList(StatDefOf.StuffPower_Insulation_Cold, 0), 0);
        printCell(t.statBases.GetStatValueFromList(StatDefOf.StuffPower_Insulation_Cold, 0).ToStringTemperatureOffset(),
            num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.statBases.GetStatValueFromList(StatDefOf.StuffPower_Insulation_Heat, 0), 0);
        printCell(t.statBases.GetStatValueFromList(StatDefOf.StuffPower_Insulation_Heat, 0).ToStringTemperatureOffset(),
            num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.statBases.GetStatValueFromList(StatDefOf.SharpDamageMultiplier, 1), 1);
        printCell(t.statBases.GetStatValueFromList(StatDefOf.SharpDamageMultiplier, 1).ToStringPercent(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.statBases.GetStatValueFromList(StatDefOf.BluntDamageMultiplier, 1), 1);
        printCell(t.statBases.GetStatValueFromList(StatDefOf.BluntDamageMultiplier, 1).ToStringPercent(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.stuffProps.statOffsets.GetStatOffsetFromList(StatDefOf.Beauty), 0);
        printCell(t.stuffProps.statOffsets.GetStatOffsetFromList(StatDefOf.Beauty) + "", num, ww, statWidth,
            "Beauty = ((Base * Factor) + Offset) * Quality");
        ww += statWidth;
        GUI.color = valueColor(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.MaxHitPoints), 1);
        printCell(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.MaxHitPoints).ToStringPercent(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.Beauty), 1);
        printCell(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.Beauty).ToStringPercent(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.WorkToMake), 1, true);
        printCell(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.WorkToMake).ToStringPercent(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.WorkToBuild), 1, true);
        printCell(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.WorkToBuild).ToStringPercent(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.Flammability), 1, true);
        printCell(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.Flammability).ToStringPercent(), num, ww);
        ww += statWidth;
        GUI.color = valueColor(t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.MeleeWeapon_CooldownMultiplier),
            1, true);
        printCell
        (t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.MeleeWeapon_CooldownMultiplier).ToStringPercent(),
            num,
            ww);
        if (StuffList.SoftWarmBedsLoaded)
        {
            ww += statWidth;
            GUI.color = valueColor(t.statBases.GetStatValueFromList(StatDef.Named("Textile_Softness"), 0),
                0);
            printCell
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

    private static float drawIcon(int rowNum, ThingDef t)
    {
        var icoRect = new Rect(0, RowHeight * rowNum, IconWidth, IconWidth);
        Widgets.ThingIcon(icoRect, t);
        return IconWidth + 2f;
    }

    private void printCell(string content, int rowNum, float x, float width = 0, string tooltip = "")
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

    private void printCellSort(string property, StatDef statDef, Source source, string content, float x,
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

    private void printCellSort(string property, string content, float x, float width = 0)
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
                switch (property)
                {
                    case "label":
                        sortSource = Source.Name;
                        break;
                    case "amount":
                        sortSource = Source.Amount;
                        break;
                    case "stacksize":
                        sortSource = Source.Stacksize;
                        break;
                }
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

    private static void drawCommon(int num, float w)
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

    private void updateList()
    {
        stuff = [];
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

        updateListSorting();

        isDirty = false;
    }

    private void updateListSorting()
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
                    ? stuff.OrderByDescending(o => stuffCountDictionary.GetValueOrDefault(o, 0))
                    : stuff.OrderBy(o => stuffCountDictionary.GetValueOrDefault(o, 0));
                break;
            case Source.Stacksize:
                stuff = sortOrder == "DESC"
                    ? stuff.OrderByDescending(o => o.stackLimit)
                    : stuff.OrderBy(o => o.stackLimit);
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

    private static void printAutoCheckbox(string text, ref bool value, ref float currentX, ref Rect rect,
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
        Amount,
        Stacksize
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