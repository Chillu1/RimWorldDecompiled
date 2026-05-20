using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_PawnsKilled : QuestPartActivable
{
	public ThingDef race;

	public Faction requiredInstigatorFaction;

	public int count;

	public MapParent mapParent;

	public string outSignalPawnKilled;

	private int killed;

	public override string DescriptionPart => string.Concat("PawnsKilled".Translate(GenLabel.BestKindLabel(race.race.AnyPawnKind, Gender.None, plural: true)).CapitalizeFirst() + ": ", killed.ToString(), " / ", count.ToString());

	public override IEnumerable<Faction> InvolvedFactions
	{
		get
		{
			foreach (Faction involvedFaction in base.InvolvedFactions)
			{
				yield return involvedFaction;
			}
			if (requiredInstigatorFaction != null)
			{
				yield return requiredInstigatorFaction;
			}
		}
	}

	protected override void Enable(SignalArgs receivedArgs)
	{
		base.Enable(receivedArgs);
		killed = 0;
	}

	public override void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
	{
		base.Notify_PawnKilled(pawn, dinfo);
		if (base.State == QuestPartState.Enabled && pawn.def == race && (requiredInstigatorFaction == null || (dinfo.HasValue && (dinfo.Value.Instigator == null || dinfo.Value.Instigator.Faction == requiredInstigatorFaction))))
		{
			killed++;
			Find.SignalManager.SendSignal(new Signal(outSignalPawnKilled));
			if (killed >= count)
			{
				Complete();
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref race, "race");
		Scribe_References.Look(ref requiredInstigatorFaction, "requiredInstigatorFaction");
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_Values.Look(ref count, "count", 0);
		Scribe_Values.Look(ref killed, "killed", 0);
		Scribe_Values.Look(ref outSignalPawnKilled, "outSignalPawnKilled");
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		race = ThingDefOf.Muffalo;
		requiredInstigatorFaction = Faction.OfPlayer;
		count = 10;
	}
}
