# GitHub Copilot Instructions for RimWorld Modding Project: Stuff List (Continued)

## Overview and Purpose
The "Stuff List (Continued)" mod is an update to Pretzalcohuatls' original mod for RimWorld. It provides players with an enhanced tool for comparing materials ("stuffs") used for crafting items. This is particularly useful in scenarios where multiple mods introduce a large variety of new materials, and players need an efficient way to determine the best materials for crafting purposes.

## Key Features and Systems
- **Colored Readout:** Improved visualization by adding color coding to the readout, enhancing user experience and readability.
- **Resource Count:** An "amount-column" that displays the total count of each resource present on the current map.
- **Responsive List-Window:** Adapts to screen size by using 95% of the screen width and 70% of the screen height.
- **Tooltips:** Provides detailed information on all items and their statistics when hovered over.
- **Additional Data Display:** Includes mass and stack-size in the item details.
- **Softness Stat Integration:** Adds support for the Softness-stat, specifically for compatibility with the Soft Warm Beds mod.
- **Planned Enhancements:**
  - Option to select which base, factor, and offset stats to display.
  - Support for additional categories of modded stuffs.

This mod is safe to add to existing savegames without causing issues.

## Coding Patterns and Conventions
- Follow C# conventions for naming, such as PascalCase for class names and camelCase for method names.
- Methods are designed to be cohesive and focused on a single task to promote code clarity.
- Use of the `MainTabWindow_StuffList` class to encapsulate the main functionalities involved in rendering and interacting with the stuff list window.
- Utilize private helper methods to manage complexity and keep the main methods clean and focused on high-level logic.

## XML Integration
- Utilize XML files to define item attributes and statistics where necessary.
- Ensure that any XML configurations comply with RimWorld's modding framework.
- Use XML for defining new stuff categories planned for future updates.

## Harmony Patching
- Employ Harmony patches to modify existing game functions safely without altering core game files directly.
- Use Harmony annotations such as `[HarmonyPatch]` to specify methods for overwriting or expanding.
- Ensure that patches are compatible with other mods by adopting non-intrusive patching methods where possible.

## Suggestions for Copilot
- **Code Completion:** Assist in completing repetitive code tasks such as initializing UI elements or setting up loops for rendering rows of items.
- **Refactoring Suggestions:** Recommend ways to streamline methods like `updateList` and `updateListSorting` that may receive more functionality with future updates.
- **Error Checking:** Analyze method calls for potential runtime errors or exceptions, like null references when accessing item data.
- **Harmony Integration:** Suggest patterns for setting up new Harmony patches or improving existing ones, especially concerning compatibility with other mods.
- **Performance Optimization:** Offer insights on optimizing methods like `triggerThread` for better responsiveness and minimal performance impact.

By following these guidelines, contributors and copilot can collaborate effectively to enhance the Stuff List (Continued) mod for RimWorld.
