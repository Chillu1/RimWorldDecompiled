using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class RitualOutcomeEffectWorker_Skylantern : RitualOutcomeEffectWorker_FromQuality
{
	public const float WanderersChance = 0.2f;

	public const float WanderersPoints = 160f;

	public static readonly FloatRange RewardMarketValueRange = new FloatRange(200f, 500f);

	public RitualOutcomeEffectWorker_Skylantern()
	{
	}

	public RitualOutcomeEffectWorker_Skylantern(RitualOutcomeEffectDef def)
		: base(def)
	{
	}

	protected override void ApplyExtraOutcome(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
	{
		extraOutcomeDesc = null;
		if (!outcome.Positive || !Rand.Chance(0.2f))
		{
			return;
		}
		List<Thing> list = new List<Thing>();
		Ideo ideo = jobRitual.Ritual.ideo;
		foreach (Precept item in ideo.PreceptsListForReading)
		{
			if (item is Precept_Building precept_Building && precept_Building.ThingDef.ritualFocus != null && precept_Building.ThingDef.ritualFocus.consumable)
			{
				list.Add(ThingMaker.MakeThing(precept_Building.ThingDef).MakeMinified());
			}
		}
		if (list.Count == 0)
		{
			list.AddRange(ThingSetMakerDefOf.VisitorGift.root.Generate(new ThingSetMakerParams
			{
				totalMarketValueRange = RewardMarketValueRange
			}));
		}
		IncidentParms incidentParms = new IncidentParms
		{
			target = jobRitual.Map,
			points = 160f,
			gifts = list,
			pawnIdeo = ideo,
			pawnCount = 3,
			storeGeneratedNeutralPawns = new List<Pawn>()
		};
		if (IncidentDefOf.WanderersSkylantern.Worker.TryExecute(incidentParms))
		{
			List<GlobalTargetInfo> list2 = new List<GlobalTargetInfo>();
			list2.AddRange(letterLookTargets.targets);
			list2.AddRange(incidentParms.storeGeneratedNeutralPawns.Select((Pawn p) => new GlobalTargetInfo(p)));
			letterLookTargets = new LookTargets(list2);
			extraOutcomeDesc = "RitualOutcomeExtraDesc_SkylanternWanderers".Translate();
		}
	}
}
