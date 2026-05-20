using Verse;

namespace RimWorld;

public class FilthGrayFleshNoticeable : Filth
{
	private bool sentLetter;

	public int biosignature;

	private const int DetectDist = 5;

	protected override void Tick()
	{
		if (sentLetter)
		{
			return;
		}
		foreach (Pawn item in base.Map.mapPawns.FreeColonistsSpawned)
		{
			if (item.Position.InHorDistOf(base.Position, 5f) && PawnUtility.ShouldSendNotificationAbout(item) && !MetalhorrorUtility.IsInfected(item) && item.health.capacities.CapableOf(PawnCapacityDefOf.Sight) && item.health.capacities.CanBeAwake)
			{
				SendLetter(item);
				break;
			}
		}
	}

	private void SendLetter(Pawn pawn)
	{
		sentLetter = true;
		string text = "";
		if (!Find.Anomaly.hasSeenGrayFlesh)
		{
			text = "LetterInterrogationUnlocked".Translate() + " ";
		}
		text += "LetterGrayFleshMethodOne".Translate();
		string arg = "- " + text + "\n\n" + string.Format("- {0}\n\n", "LetterGrayFleshMethodTwo".Translate()) + string.Format("- {0}", "LetterGrayFleshMethodThree".Translate());
		TaggedString text2 = "LetterGrayFleshDiscovered".Translate(pawn.Named("PAWN"), arg.Named("METHODS"));
		Find.LetterStack.ReceiveLetter("LetterGrayFleshDiscoveredLabel".Translate(), text2, LetterDefOf.ThreatSmall, new LookTargets(base.Position, base.Map));
		Thing thing = ThingMaker.MakeThing(ThingDefOf.GrayFleshSample);
		thing.TryGetComp<CompAnalyzableBiosignature>().biosignature = biosignature;
		GenSpawn.Spawn(thing, base.Position, base.Map);
		Find.Anomaly.hasSeenGrayFlesh = true;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref sentLetter, "sentLetter", defaultValue: false);
		Scribe_Values.Look(ref biosignature, "biosignature", 0);
	}
}
