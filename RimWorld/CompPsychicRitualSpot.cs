using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class CompPsychicRitualSpot : ThingComp
{
	private List<Thing> obstructingThings = new List<Thing>();

	private HashSet<IntVec3> occupiedCells;

	private bool Obstructed => obstructingThings.Count > 0;

	public HashSet<IntVec3> OccupiedCells => occupiedCells;

	public Lord GetLord()
	{
		if (parent is Building b)
		{
			return b.GetLord();
		}
		if (parent is Pawn p)
		{
			return p.GetLord();
		}
		if (parent is Corpse c)
		{
			return c.GetLord();
		}
		return null;
	}

	public PsychicRitual GetPsychicRitual()
	{
		return (GetLord()?.CurLordToil as LordToil_PsychicRitual)?.RitualData.psychicRitual;
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		occupiedCells = new HashSet<IntVec3>(GenRadial.RadialCellsAround(parent.Position, (float)parent.def.Size.x + 0.9f, useCenter: true));
	}

	public override string CompInspectStringExtra()
	{
		StringBuilder stringBuilder = new StringBuilder(base.CompInspectStringExtra());
		if (Obstructed)
		{
			stringBuilder.AppendInNewLine("PsychicRitualSpotObstructed".Translate());
		}
		Lord lord = GetLord();
		if (lord != null && lord.CurLordToil is LordToil_PsychicRitual lordToil_PsychicRitual)
		{
			PsychicRitual psychicRitual = lordToil_PsychicRitual.RitualData.psychicRitual;
			PsychicRitualToil psychicRitualToil = lordToil_PsychicRitual.RitualData.psychicRitualToil;
			stringBuilder.AppendInNewLine(string.Format("{0}: {1}", "PsychicRitual".Translate().CapitalizeFirst(), psychicRitual.def.LabelCap));
			stringBuilder.AppendInNewLine(psychicRitualToil.GetReport(psychicRitual, null).CapitalizeFirst().EndWithPeriod());
		}
		return stringBuilder.ToString();
	}

	public override void PostDrawExtraSelectionOverlays()
	{
		base.PostDrawExtraSelectionOverlays();
		if (parent.Faction != Faction.OfPlayer)
		{
			return;
		}
		foreach (Thing obstructingThing in obstructingThings)
		{
			GenDraw.DrawLineBetween(parent.TrueCenter(), obstructingThing.TrueCenter(), SimpleColor.Red);
		}
	}

	public override void CompTick()
	{
		base.CompTick();
		obstructingThings.Clear();
		foreach (IntVec3 occupiedCell in occupiedCells)
		{
			List<Thing> list = parent.Map.thingGrid.ThingsListAt(occupiedCell);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def.passability != Traversability.Standable)
				{
					obstructingThings.Add(list[i]);
				}
			}
		}
		PsychicRitual psychicRitual = GetPsychicRitual();
		if (psychicRitual != null && parent.Faction == Faction.OfPlayer && Obstructed)
		{
			psychicRitual.CancelPsychicRitual("PsychicRitualAreaObstructed".Translate());
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		PsychicRitual psychicRitual = GetPsychicRitual();
		if (psychicRitual != null)
		{
			yield return PsychicRitualGizmo.CancelGizmo(psychicRitual);
		}
		else
		{
			foreach (Gizmo gizmo in PsychicRitualGizmo.GetGizmos(parent))
			{
				if (Obstructed)
				{
					gizmo.Disabled = true;
					gizmo.disabledReason = "PsychicRitualAreaObstructed".Translate().CapitalizeFirst();
				}
				yield return gizmo;
			}
		}
		if (DebugSettings.ShowDevGizmos)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Reset psychic ritual cooldowns",
				action = delegate
				{
					Find.PsychicRitualManager.ClearAllCooldowns();
				}
			};
		}
	}
}
