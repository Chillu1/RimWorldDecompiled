using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompMechPowerCell : ThingComp
{
	private int powerTicksLeft;

	public bool depleted;

	private MechPowerCellGizmo gizmo;

	public CompProperties_MechPowerCell Props => (CompProperties_MechPowerCell)props;

	public float PercentFull => (float)powerTicksLeft / (float)Props.totalPowerTicks;

	public int PowerTicksLeft => powerTicksLeft;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!ModLister.CheckBiotechOrAnomalyOrOdyssey("Mech power cell"))
		{
			parent.Destroy();
			return;
		}
		base.PostSpawnSetup(respawningAfterLoad);
		if (!respawningAfterLoad && !parent.BeingTransportedOnGravship)
		{
			powerTicksLeft = Props.totalPowerTicks;
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (parent.Faction != Faction.OfPlayer && !Props.showGizmoOnNonPlayerControlled)
		{
			yield break;
		}
		if (Find.Selector.SingleSelectedThing == parent)
		{
			if (gizmo == null)
			{
				gizmo = new MechPowerCellGizmo(this)
				{
					Order = -100f
				};
			}
			yield return gizmo;
		}
		if (DebugSettings.ShowDevGizmos)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Power left 0%",
				action = delegate
				{
					powerTicksLeft = 0;
				}
			};
			yield return new Command_Action
			{
				defaultLabel = "DEV: Power left 100%",
				action = delegate
				{
					powerTicksLeft = Props.totalPowerTicks;
				}
			};
		}
	}

	public override void CompTick()
	{
		base.CompTick();
		if (depleted)
		{
			return;
		}
		powerTicksLeft--;
		if (powerTicksLeft <= 0)
		{
			if (Props.killWhenDepleted)
			{
				KillPowerProcessor();
				return;
			}
			powerTicksLeft = 0;
			depleted = true;
		}
	}

	private void KillPowerProcessor()
	{
		Pawn pawn = (Pawn)parent;
		List<BodyPartRecord> allParts = pawn.def.race.body.AllParts;
		for (int i = 0; i < allParts.Count; i++)
		{
			BodyPartRecord bodyPartRecord = allParts[i];
			if (bodyPartRecord.def.tags.Contains(BodyPartTagDefOf.BloodPumpingSource))
			{
				pawn.health.AddHediff(HediffDefOf.MissingBodyPart, bodyPartRecord);
			}
		}
		if (!pawn.Dead)
		{
			pawn.Kill(null, null);
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref powerTicksLeft, "powerTicksLeft", 0);
		Scribe_Values.Look(ref depleted, "depleted", defaultValue: false);
	}
}
