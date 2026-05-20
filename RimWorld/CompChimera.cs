using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class CompChimera : ThingComp
{
	private float totalDamageTaken;

	public CompProperties_Chimera Props => (CompProperties_Chimera)props;

	public Pawn Pawn => (Pawn)parent;

	public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
		if (Pawn.Dead)
		{
			return;
		}
		totalDamageTaken += totalDamageDealt;
		if (!Pawn.Dead && totalDamageTaken > 0f && !Pawn.health.hediffSet.HasHediff(HediffDefOf.RageSpeed) && !Pawn.health.Downed)
		{
			Pawn.health.AddHediff(HediffMaker.MakeHediff(HediffDefOf.RageSpeed, Pawn));
			if (Pawn.Spawned)
			{
				EffecterDefOf.ChimeraRage.Spawn(Pawn.Position, Pawn.Map).Cleanup();
			}
		}
	}

	public override void Notify_Downed()
	{
		Hediff firstHediffOfDef = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.RageSpeed);
		if (firstHediffOfDef != null)
		{
			Pawn.health.RemoveHediff(firstHediffOfDef);
		}
	}

	public override void CompTickRare()
	{
		base.CompTickRare();
		if (!Pawn.Dead && Pawn.health.summaryHealth.SummaryHealthPercent >= Props.rageEndHealthPercentThreshold)
		{
			Hediff firstHediffOfDef = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.RageSpeed);
			if (firstHediffOfDef != null)
			{
				Pawn.health.RemoveHediff(firstHediffOfDef);
				totalDamageTaken = 0f;
			}
		}
	}

	public Thought_MemoryObservation GiveObservedThought(Pawn observer)
	{
		Thought_MemoryObservation_Chimera obj = (Thought_MemoryObservation_Chimera)ThoughtMaker.MakeThought(ThoughtDefOf.Chimera);
		obj.Target = Pawn;
		return obj;
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		Command_Action command_Action = new Command_Action();
		command_Action.defaultLabel = "DEV: Switch stalk/attack mode";
		command_Action.action = delegate
		{
			Lord lord = Pawn.GetLord();
			if (lord == null)
			{
				lord = LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_ChimeraAssault(), Pawn.Map, new List<Pawn> { Pawn });
				Pawn.SetFaction(Faction.OfEntities);
			}
			if (lord.LordJob is LordJob_ChimeraAssault lordJob_ChimeraAssault)
			{
				lordJob_ChimeraAssault.SwitchMode();
			}
		};
		yield return command_Action;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref totalDamageTaken, "totalDamageTaken", 0f);
	}
}
