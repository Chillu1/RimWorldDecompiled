using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Grammar;

namespace RimWorld
{
	public class SitePartWorker_SleepingMechanoids : SitePartWorker
	{
		public override string GetArrivedLetterPart(Map map, out LetterDef preferredLetterDef, out LookTargets lookTargets)
		{
			string arrivedLetterPart = base.GetArrivedLetterPart(map, out preferredLetterDef, out lookTargets);
			IEnumerable<Pawn> source = map.mapPawns.AllPawnsSpawned.Where((Pawn x) => x.RaceProps.IsMechanoid);
			Pawn pawn = source.Where((Pawn x) => x.GetLord() != null && x.GetLord().LordJob is LordJob_SleepThenAssaultColony).FirstOrDefault();
			if (pawn == null)
			{
				pawn = source.FirstOrDefault();
			}
			lookTargets = pawn;
			return arrivedLetterPart;
		}

		public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
		{
			base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
			int mechanoidsCount = GetMechanoidsCount(part.site, part.parms);
			outExtraDescriptionRules.Add(new Rule_String("count", mechanoidsCount.ToString()));
			outExtraDescriptionConstants.Add("count", mechanoidsCount.ToString());
		}

		public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
		{
			return base.GetPostProcessedThreatLabel(site, sitePart) + ": " + "KnownSiteThreatEnemyCountAppend".Translate(GetMechanoidsCount(site, sitePart.parms), "Enemies".Translate());
		}

		public override SitePartParams GenerateDefaultParams(float myThreatPoints, int tile, Faction faction)
		{
			SitePartParams sitePartParams = base.GenerateDefaultParams(myThreatPoints, tile, faction);
			sitePartParams.threatPoints = Mathf.Max(sitePartParams.threatPoints, FactionDefOf.Mechanoid.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
			return sitePartParams;
		}

		private int GetMechanoidsCount(Site site, SitePartParams parms)
		{
			return PawnGroupMakerUtility.GeneratePawnKindsExample(new PawnGroupMakerParms
			{
				tile = site.Tile,
				faction = Faction.OfMechanoids,
				groupKind = PawnGroupKindDefOf.Combat,
				points = parms.threatPoints,
				seed = SleepingMechanoidsSitePartUtility.GetPawnGroupMakerSeed(parms)
			}).Count();
		}
	}
}
