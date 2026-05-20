using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace RimWorld.QuestGen;

public class QuestPart_DistressCallAmbush : QuestPart
{
	private string inSignal;

	private Site site;

	private float points;

	public QuestPart_DistressCallAmbush()
	{
	}

	public QuestPart_DistressCallAmbush(string inSignal, Site site, float points)
	{
		this.inSignal = inSignal;
		this.site = site;
		this.points = points;
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (signal.tag != inSignal)
		{
			return;
		}
		List<Thing> list = (from b in site.Map.listerThings.ThingsOfDef(ThingDefOf.PitBurrow)
			where !b.Fogged()
			select b).ToList();
		if (list.NullOrEmpty())
		{
			return;
		}
		List<Pawn> source = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
		{
			groupKind = PawnGroupKindDefOf.Fleshbeasts,
			points = points,
			faction = Faction.OfEntities,
			raidStrategy = RaidStrategyDefOf.ImmediateAttack
		}).ToList();
		Lord lord = LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_FleshbeastAssault(), site.Map);
		Thing burrow = list.RandomElement();
		IntVec3 result;
		SpawnRequest request = new SpawnRequest(spawnPositions: source.Select((Pawn p) => (!CellFinder.TryFindRandomCellNear(burrow.Position, site.Map, 2, (IntVec3 c) => c.Walkable(site.Map) && !c.Fogged(site.Map), out result)) ? burrow.Position : result).ToList(), thingsToSpawn: source.Cast<Thing>().ToList(), batchSize: 1, intervalSeconds: 0.5f, lord: lord);
		site.Map.deferredSpawner.AddRequest(request);
		Find.LetterStack.ReceiveLetter("DistressSignalAmbushLabel".Translate(), "DistressSignalAmbushText".Translate(), LetterDefOf.ThreatBig, burrow);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_References.Look(ref site, "site");
		Scribe_Values.Look(ref points, "points", 0f);
	}
}
