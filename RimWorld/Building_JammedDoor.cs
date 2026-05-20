using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Building_JammedDoor : Building_Door
{
	private bool jammed = true;

	public bool Jammed => jammed;

	public void UnlockDoor()
	{
		jammed = false;
	}

	public void UnlockAndOpenDoor()
	{
		jammed = false;
		DoorOpen();
	}

	protected override void DoorOpen(int ticksToClose = 110)
	{
		base.DoorOpen(ticksToClose);
		holdOpenInt = true;
	}

	public override bool PawnCanOpen(Pawn p)
	{
		return !jammed;
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		if (!jammed)
		{
			base.DrawAt(drawLoc, flip);
			return;
		}
		DoorPreDraw();
		foreach (ThingComp allComp in base.AllComps)
		{
			if (allComp is IJammedDoorDrawer jammedDoorDrawer)
			{
				jammedDoorDrawer.DrawJammed(base.Rotation);
				break;
			}
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (DebugSettings.ShowDevGizmos && !holdOpenInt)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Unlock",
				action = UnlockDoor
			};
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref jammed, "jammed", defaultValue: false);
	}
}
