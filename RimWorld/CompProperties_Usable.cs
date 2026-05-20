using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_Usable : CompProperties
{
	public JobDef useJob;

	[MustTranslate]
	public string useLabel;

	[MustTranslate]
	public string useMessage;

	public int useDuration = 100;

	public HediffDef userMustHaveHediff;

	public MenuOptionPriority floatMenuOptionPriority = MenuOptionPriority.Default;

	public FactionDef floatMenuFactionIcon;

	public ThingDef warmupMote;

	public ThingDef finalizeMote;

	public bool ignoreOtherReservations;

	public bool showUseGizmo;

	public List<MutantDef> allowedMutants = new List<MutantDef>();

	public List<PlanetLayerDef> layerWhitelist = new List<PlanetLayerDef>();

	public List<PlanetLayerDef> layerBlacklist = new List<PlanetLayerDef>();

	public CompProperties_Usable()
	{
		compClass = typeof(CompUsable);
	}
}
