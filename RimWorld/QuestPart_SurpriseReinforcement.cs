using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_SurpriseReinforcement : QuestPartActivable
{
	public MapParent mapParent;

	public Faction faction;

	public float reinforcementChance;

	private int ticksTillRaid = -1;

	public const float RaidThreatPointsMultiplier = 2.5f;

	private static readonly FloatRange RandomDelayRange = new FloatRange(300f, 900f);

	public override void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
	{
		if (reinforcementChance != 0f && pawn.Faction == faction && pawn.MapHeld != null && pawn.MapHeld.Parent != null && pawn.MapHeld.Parent == mapParent)
		{
			if (Rand.Chance(reinforcementChance))
			{
				ticksTillRaid = (int)RandomDelayRange.RandomInRange;
			}
			reinforcementChance = 0f;
		}
	}

	public override void QuestPartTick()
	{
		base.QuestPartTick();
		if (ticksTillRaid > -1)
		{
			ticksTillRaid--;
			if (ticksTillRaid == 0)
			{
				IncidentParms incidentParms = new IncidentParms();
				incidentParms.target = mapParent.Map;
				incidentParms.points = StorytellerUtility.DefaultThreatPointsNow(incidentParms.target) * 2.5f;
				incidentParms.faction = faction;
				incidentParms.customLetterLabel = "LetterLabelSurpriseReinforcements".Translate();
				incidentParms.customLetterText = "LetterSurpriseReinforcements".Translate(incidentParms.faction.def.pawnsPlural, incidentParms.faction).Resolve();
				IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParms);
			}
		}
	}

	public override bool QuestPartReserves(Faction f)
	{
		return f == faction;
	}

	public override void Notify_FactionRemoved(Faction removedFaction)
	{
		if (removedFaction == faction)
		{
			faction = null;
		}
	}

	public override void ExposeData()
	{
		if (Scribe.mode == LoadSaveMode.Saving && faction != null && !Find.FactionManager.AllFactions.Contains(faction))
		{
			faction = null;
		}
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_References.Look(ref faction, "faction");
		Scribe_Values.Look(ref reinforcementChance, "reinforcementChance", 0f);
		Scribe_Values.Look(ref ticksTillRaid, "ticksTillRaid", -1);
	}
}
