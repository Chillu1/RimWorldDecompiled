using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class PocketMapExit : MapPortal
{
	private static readonly CachedTexture ExitMapTex = new CachedTexture("UI/Commands/ExitCave");

	private static readonly CachedTexture ViewEntranceTex = new CachedTexture("UI/Commands/ViewCave");

	public MapPortal entrance;

	public override string EnterString => "ExitPortal".Translate(entrance?.def.portal.pocketMapGenerator.label);

	public override string CancelEnterString => "CommandCancelExitPortal".Translate();

	protected override Texture2D EnterTex => ExitMapTex.Texture;

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (!respawningAfterLoad)
		{
			if (PocketMapUtility.currentlyGeneratingPortal == null)
			{
				Log.Error("Pocket map exit could not find map portal to connect to");
				return;
			}
			entrance = PocketMapUtility.currentlyGeneratingPortal;
			entrance.exit = this;
		}
	}

	public override Map GetOtherMap()
	{
		return entrance.Map;
	}

	public override IntVec3 GetDestinationLocation()
	{
		return entrance.Position;
	}

	public override void OnEntered(Pawn pawn)
	{
		Notify_ThingAdded(pawn);
		if (Find.CurrentMap == base.Map)
		{
			entrance.def.portal.traverseSound?.PlayOneShot(this);
		}
		else if (Find.CurrentMap == entrance.Map)
		{
			entrance.def.portal.traverseSound?.PlayOneShot(entrance);
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		yield return new Command_Action
		{
			defaultLabel = "CommandViewSurface".Translate(),
			defaultDesc = "CommandViewSurfaceDesc".Translate(),
			icon = ViewEntranceTex.Texture,
			action = delegate
			{
				CameraJumper.TryJumpAndSelect(entrance);
			}
		};
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref entrance, "pitGate");
	}
}
