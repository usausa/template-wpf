namespace Template.WindowsApp.Models.Sample;

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed record ColorCategory(string Title, IReadOnlyList<ColorGroup> Groups);

public sealed record ColorGroup(string Title, IReadOnlyList<ColorEntry> Entries);

public sealed record ColorEntry(string Name, string Key);

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class ColorSample
{
    public static IReadOnlyList<ColorCategory> LoadCategories() =>
    [
        new("Text",
        [
            new("Text",
            [
                new("Primary", "TextFillColorPrimaryBrush"),
                new("Secondary", "TextFillColorSecondaryBrush"),
                new("Tertiary", "TextFillColorTertiaryBrush"),
                new("Disabled", "TextFillColorDisabledBrush")
            ]),
            new("Accent text",
            [
                new("Primary", "AccentTextFillColorPrimaryBrush"),
                new("Secondary", "AccentTextFillColorSecondaryBrush"),
                new("Tertiary", "AccentTextFillColorTertiaryBrush"),
                new("Disabled", "AccentTextFillColorDisabledBrush")
            ]),
            new("Text on accent",
            [
                new("Primary", "TextOnAccentFillColorPrimaryBrush"),
                new("Secondary", "TextOnAccentFillColorSecondaryBrush"),
                new("Disabled", "TextOnAccentFillColorDisabledBrush"),
                new("Selected text", "TextOnAccentFillColorSelectedTextBrush")
            ])
        ]),
        new("Fill",
        [
            new("Control",
            [
                new("Default", "ControlFillColorDefaultBrush"),
                new("Secondary", "ControlFillColorSecondaryBrush"),
                new("Tertiary", "ControlFillColorTertiaryBrush"),
                new("Disabled", "ControlFillColorDisabledBrush"),
                new("Transparent", "ControlFillColorTransparentBrush"),
                new("Input active", "ControlFillColorInputActiveBrush")
            ]),
            new("Control strong",
            [
                new("Default", "ControlStrongFillColorDefaultBrush"),
                new("Disabled", "ControlStrongFillColorDisabledBrush")
            ]),
            new("Control solid",
            [
                new("Default", "ControlSolidFillColorDefaultBrush")
            ]),
            new("Subtle",
            [
                new("Transparent", "SubtleFillColorTransparentBrush"),
                new("Secondary", "SubtleFillColorSecondaryBrush"),
                new("Tertiary", "SubtleFillColorTertiaryBrush"),
                new("Disabled", "SubtleFillColorDisabledBrush")
            ]),
            new("Control alt",
            [
                new("Transparent", "ControlAltFillColorTransparentBrush"),
                new("Secondary", "ControlAltFillColorSecondaryBrush"),
                new("Tertiary", "ControlAltFillColorTertiaryBrush"),
                new("Quarternary", "ControlAltFillColorQuarternaryBrush"),
                new("Disabled", "ControlAltFillColorDisabledBrush")
            ]),
            new("Accent",
            [
                new("Default", "AccentFillColorDefaultBrush"),
                new("Secondary", "AccentFillColorSecondaryBrush"),
                new("Tertiary", "AccentFillColorTertiaryBrush"),
                new("Disabled", "AccentFillColorDisabledBrush"),
                new("Selected text background", "AccentFillColorSelectedTextBackgroundBrush")
            ])
        ]),
        new("Stroke",
        [
            new("Control",
            [
                new("Default", "ControlStrokeColorDefaultBrush"),
                new("Secondary", "ControlStrokeColorSecondaryBrush"),
                new("On accent default", "ControlStrokeColorOnAccentDefaultBrush"),
                new("On accent secondary", "ControlStrokeColorOnAccentSecondaryBrush"),
                new("On accent tertiary", "ControlStrokeColorOnAccentTertiaryBrush"),
                new("On accent disabled", "ControlStrokeColorOnAccentDisabledBrush"),
                new("For strong fill when on image", "ControlStrokeColorForStrongFillWhenOnImageBrush")
            ]),
            new("Control strong",
            [
                new("Default", "ControlStrongStrokeColorDefaultBrush"),
                new("Disabled", "ControlStrongStrokeColorDisabledBrush")
            ]),
            new("Card",
            [
                new("Default", "CardStrokeColorDefaultBrush"),
                new("Default solid", "CardStrokeColorDefaultSolidBrush")
            ]),
            new("Divider",
            [
                new("Default", "DividerStrokeColorDefaultBrush")
            ]),
            new("Surface",
            [
                new("Default", "SurfaceStrokeColorDefaultBrush"),
                new("Flyout", "SurfaceStrokeColorFlyoutBrush"),
                new("Inverse", "SurfaceStrokeColorInverseBrush")
            ]),
            new("Focus",
            [
                new("Outer", "FocusStrokeColorOuterBrush"),
                new("Inner", "FocusStrokeColorInnerBrush")
            ])
        ]),
        new("Background",
        [
            new("Card",
            [
                new("Default", "CardBackgroundFillColorDefaultBrush"),
                new("Secondary", "CardBackgroundFillColorSecondaryBrush")
            ]),
            new("Smoke",
            [
                new("Default", "SmokeFillColorDefaultBrush")
            ]),
            new("Layer",
            [
                new("Default", "LayerFillColorDefaultBrush"),
                new("Alt", "LayerFillColorAltBrush"),
                new("On acrylic default", "LayerOnAcrylicFillColorDefaultBrush")
            ]),
            new("Solid background",
            [
                new("Base", "SolidBackgroundFillColorBaseBrush"),
                new("Secondary", "SolidBackgroundFillColorSecondaryBrush"),
                new("Tertiary", "SolidBackgroundFillColorTertiaryBrush"),
                new("Quarternary", "SolidBackgroundFillColorQuarternaryBrush"),
                new("Base alt", "SolidBackgroundFillColorBaseAltBrush")
            ])
        ]),
        new("Signal",
        [
            new("System",
            [
                new("Success", "SystemFillColorSuccessBrush"),
                new("Caution", "SystemFillColorCautionBrush"),
                new("Critical", "SystemFillColorCriticalBrush"),
                new("Attention", "SystemFillColorAttentionBrush"),
                new("Neutral", "SystemFillColorNeutralBrush"),
                new("Solid neutral", "SystemFillColorSolidNeutralBrush")
            ]),
            new("System background",
            [
                new("Success", "SystemFillColorSuccessBackgroundBrush"),
                new("Caution", "SystemFillColorCautionBackgroundBrush"),
                new("Critical", "SystemFillColorCriticalBackgroundBrush"),
                new("Attention", "SystemFillColorAttentionBackgroundBrush"),
                new("Neutral", "SystemFillColorNeutralBackgroundBrush"),
                new("Solid attention", "SystemFillColorSolidAttentionBackgroundBrush"),
                new("Solid neutral", "SystemFillColorSolidNeutralBackgroundBrush")
            ])
        ])
    ];
}
