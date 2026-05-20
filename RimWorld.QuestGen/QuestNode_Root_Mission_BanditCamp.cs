using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_Mission_BanditCamp : QuestNode_Root_Mission
{
	private const float LeaderChance = 0.1f;

	private const float MinSiteThreatPoints = 200f;

	private static readonly SimpleCurve PawnCountToSitePointsFactorCurve = new SimpleCurve
	{
		new CurvePoint(1f, 0.33f),
		new CurvePoint(3f, 0.37f),
		new CurvePoint(5f, 0.45f),
		new CurvePoint(10f, 0.5f)
	};

	public List<FactionDef> factionsToDrawLeaderFrom;

	public List<FactionDef> siteFactions;

	protected override string QuestTag => "BanditCamp";

	protected override bool AddCampLootReward => true;

	private QuestGen_Pawns.GetPawnParms GetAskerParms => new QuestGen_Pawns.GetPawnParms
	{
		mustBeOfKind = PawnKindDefOf.Empire_Royal_NobleWimp,
		mustHaveRoyalTitleInCurrentFaction = true,
		canGeneratePawn = true
	};

	protected override Pawn GetAsker(Quest quest)
	{
		if (Rand.Chance(0.1f))
		{
			return Find.FactionManager.AllFactions.Where((Faction f) => factionsToDrawLeaderFrom.Contains(f.def)).RandomElement().leader;
		}
		return quest.GetPawn(GetAskerParms);
	}

	protected override bool CanGetAsker()
	{
		Pawn pawn;
		return QuestGen_Pawns.GetPawnTest(GetAskerParms, out pawn);
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
		for (int i = 1; i < population; i++)
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
		int maxInclusive = Math.Max(num, (int)(0.5f * (float)population));
		return Rand.RangeInclusive(num, maxInclusive);
	}

	protected override Site GenerateSite(Pawn asker, float threatPoints, int pawnCount, int population, PlanetTile tile)
	{
		TryGetSiteFaction(out var faction);
		Site site = QuestGen_Sites.GenerateSite(new SitePartDefWithParams[1]
		{
			new SitePartDefWithParams(SitePartDefOf.BanditCamp, new SitePartParams
			{
				threatPoints = GetSiteThreatPoints(threatPoints, population, pawnCount)
			})
		}, tile, faction);
		site.factionMustRemainHostile = true;
		site.desiredThreatPoints = site.ActualThreatPoints;
		site.preventGravshipLanding = true;
		return site;
	}

	private bool TryGetSiteFaction(out Faction faction)
	{
		return Find.FactionManager.AllFactions.Where((Faction f) => !f.temporary && siteFactions.Contains(f.def)).TryRandomElement(out faction);
	}

	protected override bool DoesPawnCountAsAvailableForFight(Pawn p)
	{
		return QuestNode_Root_Mission.PawnCanFight(p);
	}

	protected override bool TestRunInt(Slate slate)
	{
		if (!base.TestRunInt(slate))
		{
			return false;
		}
		Faction faction;
		return TryGetSiteFaction(out faction);
	}
}
