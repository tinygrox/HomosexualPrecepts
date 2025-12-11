using RimWorld;
// ReSharper disable InconsistentNaming
// ReSharper disable UnassignedField.Global

namespace HomosexualPrecepts;
[DefOf]
public static class ModDefOf
{
    [MayRequireIdeology]
    public static PreceptDef Homo_Lesbian_Approved;
    [MayRequireIdeology]
    public static PreceptDef Homo_Lesbian_Abhorrent;
    [MayRequireIdeology]
    public static PreceptDef Homo_Lesbian_Honorable;

    [MayRequireIdeology]
    public static PreceptDef Homo_Gay_Approved;
    [MayRequireIdeology]
    public static PreceptDef Homo_Gay_Abhorrent;
    [MayRequireIdeology]
    public static PreceptDef Homo_Gay_Honorable;

    static ModDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(ModDefOf));
    }
}
