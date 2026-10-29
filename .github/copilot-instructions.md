# GitHub Copilot Instructions for RimWorld Modding

Welcome to the GitHub Copilot Instructions for the Stuff List (Continued) mod! This document will provide an overview of the mod, its key features and systems, coding patterns and conventions, XML integration, and Harmony patching. Additionally, it will offer suggestions for using Copilot to enhance your development experience.

## Mod Overview and Purpose

**Mod Name:** Stuff List (Continued)  
**Mod Description:** An updated version of Pretzalcohuatl's mod, providing a tool to compare stats for stuffs in RimWorld. This mod helps players manage and sort resources by displaying various attributes such as color, amount, mass, stack size, and the Softness-stat from Soft Warm Beds.

**Purpose:**  
- To provide a comprehensive view of resources on the current map.
- To aid players in decision-making by offering a dynamic comparison tool.
- To enhance the modding experience by supporting modded stuff categories and customizable stat displays.

## Key Features and Systems

1. **Dynamic List Window:**
   - The list window adjusts to 95% of screen width and 70% of screen height, ensuring a comfortable viewing experience.
   
2. **Color-Coded Readout:**
   - Added color to the readout to improve visual overview and differentiation of resources.

3. **Amount Column:**
   - Displays the total count of all resources on the current map.

4. **Tooltips:**
   - Tooltips are available on all items and stats to provide additional information.

5. **Mass and Stack Size:**
   - Added mass and stack size columns to help players manage resources more effectively.

6. **Softness Support:**
   - Supports the Softness-stat from the Soft Warm Beds mod, allowing players to consider this attribute when selecting resources.

7. **Customizable Stat Display:**
   - Planned updates include the ability to select base, factor, and offset stats for display.

8. **Modded Stuff Categories Support:**
   - Future updates will support additional categories introduced by modded content.

## Coding Patterns and Conventions

### Directory Structure
- **Source:** Contains the main C# source files.
- **Defs:** Contains XML definition files.
- **About:** Contains metadata about the mod.
- **Manifest:** Contains the mod's manifest file.
- **TranslationTemplate:** Contains translation templates for different languages.

### C# Source Files
- **MainTabWindow_StuffList.cs:** Handles the main tab window for the Stuff List, including rendering and user interactions.
- **StuffList.cs:** Manages the list of stuffs and their associated data.
- **StuffListMod.cs:** Manages the mod settings and configurations.
- **StuffListSettings.cs:** Handles the saving and loading of settings data.

### XML Integration
- **Defs/MainButtonDefs/StuffList.xml:** Defines the main button for accessing the Stuff List.
- **About.xml:** Contains metadata about the mod.
- **Manifest.xml:** Contains the mod's manifest information.
- **TranslationTemplate/MainButtonDef/StuffList.xml:** Contains translation templates for the main button.

### Harmony Patching
The mod uses Harmony patching to integrate with RimWorld's existing systems. Key areas patched include:
- **Resource Management:** To add the amount column and dynamic list sizing.
- **Stat Display:** To include additional attributes like mass and stack size.
- **Tooltips:** To enhance user experience with tooltips.

## Suggestions for Copilot

1. **C# Code Generation:**
   - Use Copilot to quickly generate methods and classes related to resource management and user interactions.
   - For example, generating a new method to handle the display of additional stats.

2. **XML Defs Creation:**
   - Utilize Copilot to create or modify XML definition files for new stats or categories.
   - For instance, generating a new XML definition for a new resource attribute.

3. **Harmony Patching:**
   - Use Copilot to generate Harmony patches for integrating new features or fixing existing issues.
   - For example, creating a patch to add the Softness attribute to the list display.

4. **Error Handling:**
   - Use Copilot to generate error handling code for debugging and ensuring mod compatibility.
   - For example, generating code to log errors or handle exceptions gracefully.

5. **Documentation:**
   - Use Copilot to generate documentation for new methods and classes, improving code readability and maintainability.
   - For example, generating comments for a new method explaining its purpose and parameters.

6. **Testing:**
   - Use Copilot to generate test cases for new features, ensuring that the mod functions as intended.
   - For example, generating unit tests for a new method to verify its behavior.

## Conclusion

The Stuff List (Continued) mod provides a powerful tool for managing and comparing resources in RimWorld. By following the coding patterns and conventions outlined in this document, and leveraging GitHub Copilot for code generation and integration tasks, you can enhance your development experience and create a robust mod.

Feel free to reach out to the mod's community for support or contribute improvements through the GitHub repository. Happy modding!

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

