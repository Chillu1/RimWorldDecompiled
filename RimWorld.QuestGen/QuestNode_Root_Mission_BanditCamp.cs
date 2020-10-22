using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_Root_Mission_BanditCamp : QuestNode_Root_Mission
	{
		private const float LeaderChance = 0.1f;

		private static readonly SimpleCurve PawnCountToSitePointsFactorCurve = new SimpleCurve
		{
			new CurvePoint(1f, 0.33f),
			new CurvePoint(3f, 0.37f),
			new CurvePoint(5f, 0.45f),
			new CurvePoint(10f, 0.5f)
		};

		private const float MinSiteThreatPoints = 200f;

		public List<FactionDef> factionsToDrawLeaderFrom;

		public FactionDef siteFaction;

		protected override string QuestTag => "BanditCamp";

		protected override bool AddCampLootReward => true;

		protected override Pawn GetAsker(Quest quest)
		{
			if (Rand.Chance(0.1f))
			{
				return Find.FactionManager.AllFactions.Where((Faction f) => factionsToDrawLeaderFrom.Contains(f.def)).RandomElement().leader;
			}
			return quest.GetPawn(new QuestGen_Pawns.GetPawnParms
			{
				mustBeOfKind = PawnKindDefOf.Empire_Royal_NobleWimp,
				mustHaveRoyalTitleInCurrentFaction = true,
				canGeneratePawn = true
			});
		}

		private float GetSiteThreatPoints(float threatPoints, int population, int pawnCount)
		{
			return threatPoints * ((float)pawnCount / (float)population) * PawnCountToSitePointsFactorCurve.Evaluate(pawnCount);
		}

		protected override int GetRequiredPawnCount(int population, float threatPoints)
		{
			if (population == 0)
			{
				return -1;
			}
			int num = -1;
			for (int i = 1; i <= population; i++)
			{
				if (GetSiteThreatPoints(threatPoints, population, i) >= 200f)
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				return -1;
			}
			return Rand.RangeInclusive(num, population);
		}

		protected override Site GenerateSite(Pawn asker, float threatPoints, int pawnCount, int population, int tile)
		{
			Site site = QuestGen_Sites.GenerateSite(new SitePartDefWithParams[1]
			{
				new SitePartDefWithParams(SitePartDefOf.BanditCamp, new SitePartParams
				{
					threatPoints = GetSiteThreatPoints(threatPoints, population, pawnCount)
				})
			}, tile, Find.FactionManager.AllFactions.Where((Faction f) => f.def == siteFaction).FirstOrDefault());
			site.factionMustRemainHostile = true;
			site.desiredThreatPoints = site.ActualThreatPoints;
			return site;
		}

		protected override bool DoesPawnCountAsAvailableForFight(Pawn p)
		{
			if (p.Downed)
			{
				return false;
			}
			if (p.health.hediffSet.BleedRateTotal > 0f)
			{
				return false;
			}
			if (p.health.hediffSet.HasTendableNonInjuryNonMissingPartHediff())
			{
				return false;
			}
			if (p.IsQuestLodger())
			{
				return false;
			}
			return true;
		}
	}
}
