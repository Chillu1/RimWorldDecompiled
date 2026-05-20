using System.Collections.Generic;
using Verse;

namespace RimWorld;

public struct IdeoGenerationParms
{
	public FactionDef forFaction;

	public bool forceNoExpansionIdeo;

	public bool classicExtra;

	public List<PreceptDef> disallowedPrecepts;

	public List<MemeDef> disallowedMemes;

	public List<MemeDef> forcedMemes;

	public bool forceNoWeaponPreference;

	public bool forNewFluidIdeo;

	public bool fixedIdeo;

	public string name;

	public List<StyleCategoryDef> styles;

	public List<DeityPreset> deities;

	public bool hidden;

	public string description;

	public bool requiredPreceptsOnly;

	public IdeoGenerationParms(FactionDef forFaction, bool forceNoExpansionIdeo = false, List<PreceptDef> disallowedPrecepts = null, List<MemeDef> disallowedMemes = null, List<MemeDef> forcedMemes = null, bool classicExtra = false, bool forceNoWeaponPreference = false, bool forNewFluidIdeo = false, bool fixedIdeo = false, string name = "", List<StyleCategoryDef> styles = null, List<DeityPreset> deities = null, bool hidden = false, string description = "", bool requiredPreceptsOnly = false)
	{
		this.forFaction = forFaction;
		this.forceNoExpansionIdeo = forceNoExpansionIdeo;
		this.disallowedPrecepts = disallowedPrecepts;
		this.disallowedMemes = disallowedMemes;
		this.forcedMemes = forcedMemes;
		this.classicExtra = classicExtra;
		this.forceNoWeaponPreference = forceNoWeaponPreference;
		this.forNewFluidIdeo = forNewFluidIdeo;
		this.fixedIdeo = fixedIdeo;
		this.name = name;
		this.styles = styles;
		this.deities = deities;
		this.hidden = hidden;
		this.description = description;
		this.requiredPreceptsOnly = requiredPreceptsOnly;
	}
}
