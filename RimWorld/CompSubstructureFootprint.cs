using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompSubstructureFootprint : ThingComp
{
	[Unsaved(false)]
	private CompGravshipFacility facilityComp;

	private bool displaySubstructureOverlay = true;

	private static readonly CachedTexture DisplaySubstructureTex = new CachedTexture("UI/Commands/ShowSubstructureOverlay");

	public CompProperties_SubstructureFootprint Props => (CompProperties_SubstructureFootprint)props;

	public bool DisplaySubstructureOverlay
	{
		get
		{
			if (Props.displaySubstructureOverlayWhenSelected)
			{
				return displaySubstructureOverlay;
			}
			return false;
		}
	}

	public bool Valid
	{
		get
		{
			if (facilityComp != null)
			{
				return facilityComp.CanBeActive;
			}
			return true;
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		parent.Map.substructureGrid?.MarkDirty();
		facilityComp = parent.TryGetComp<CompGravshipFacility>();
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		base.PostDeSpawn(map, mode);
		map.substructureGrid?.MarkDirty();
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (Props.displaySubstructureOverlayWhenSelected && parent.Faction == Faction.OfPlayer)
		{
			yield return new Command_Toggle
			{
				defaultLabel = "CommandShowSubstructureOverlay".Translate(),
				defaultDesc = "CommandShowSubstructureOverlayDesc".Translate(),
				icon = DisplaySubstructureTex.Texture,
				isActive = () => displaySubstructureOverlay,
				toggleAction = delegate
				{
					displaySubstructureOverlay = !displaySubstructureOverlay;
				}
			};
		}
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref displaySubstructureOverlay, "displaySubstructureOverlay", defaultValue: true);
	}
}
