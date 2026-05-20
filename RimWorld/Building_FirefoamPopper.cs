using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Building_FirefoamPopper : Building
{
	private bool autoRebuild;

	private static readonly CachedTexture RebuildCommandTex = new CachedTexture("UI/Commands/RearmFirefoamPopper");

	private bool CanSetAutoRebuild
	{
		get
		{
			if (base.Faction == Faction.OfPlayer && def.blueprintDef != null)
			{
				return def.IsResearchFinished;
			}
			return false;
		}
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (!respawningAfterLoad && !base.BeingTransportedOnGravship)
		{
			autoRebuild = CanSetAutoRebuild && map.areaManager.Home[base.Position];
		}
	}

	public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
	{
		Map map = base.Map;
		base.Destroy(mode);
		if (mode != DestroyMode.Deconstruct && autoRebuild && map != null && CanSetAutoRebuild && GenConstruct.CanPlaceBlueprintAt(def, base.Position, base.Rotation, map).Accepted)
		{
			GenConstruct.PlaceBlueprintForBuild(def, base.Position, map, base.Rotation, Faction.OfPlayer, base.Stuff);
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (CanSetAutoRebuild)
		{
			yield return new Command_Toggle
			{
				defaultLabel = "CommandAutoRebuild_Building".Translate(),
				defaultDesc = "CommandAutoRebuild_BuildingDesc".Translate(this.Named("BUILDING")),
				hotKey = KeyBindingDefOf.Misc3,
				icon = RebuildCommandTex.Texture,
				isActive = () => autoRebuild,
				toggleAction = delegate
				{
					autoRebuild = !autoRebuild;
				}
			};
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref autoRebuild, "autoRebuild", defaultValue: false);
	}
}
