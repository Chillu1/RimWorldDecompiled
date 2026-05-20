using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld.Planet;

public class CaravanArrivalAction_VisitSite : CaravanArrivalAction
{
	private Site site;

	private static List<SitePartDef> tmpDefs = new List<SitePartDef>();

	private static List<SitePartDef> tmpUsedDefs = new List<SitePartDef>();

	public override string Label => site.ApproachOrderString;

	public override string ReportString => site.ApproachingReportString;

	public CaravanArrivalAction_VisitSite()
	{
	}

	public CaravanArrivalAction_VisitSite(Site site)
	{
		this.site = site;
	}

	public override FloatMenuAcceptanceReport StillValid(Caravan caravan, PlanetTile destinationTile)
	{
		FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
		if (!floatMenuAcceptanceReport)
		{
			return floatMenuAcceptanceReport;
		}
		if (site != null && site.Tile != destinationTile)
		{
			return false;
		}
		return CanVisit(caravan, site);
	}

	public override void Arrived(Caravan caravan)
	{
		if (!site.HasMap)
		{
			LongEventHandler.QueueLongEvent(delegate
			{
				DoEnter(caravan, site);
			}, "GeneratingMapForNewEncounter", doAsynchronously: false, null);
		}
		else
		{
			DoEnter(caravan, site);
		}
	}

	private void DoEnter(Caravan caravan, Site site)
	{
		LookTargets lookTargets = new LookTargets(caravan.PawnsListForReading);
		bool draftColonists = site.Faction == null || site.Faction.HostileTo(Faction.OfPlayer);
		bool num = !site.HasMap;
		Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(site.Tile, site.PreferredMapSize, null);
		if (num)
		{
			Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
			PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(orGenerateMap.mapPawns.AllPawns, "LetterRelatedPawnsSite".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, informEvenIfSeenBefore: true);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("LetterCaravanEnteredMap".Translate(caravan.Label, site).CapitalizeFirst());
			AppendThreatInfo(stringBuilder, site, orGenerateMap, out var letterDef, out var allLookTargets);
			TaggedString letterText = null;
			if (site.parts.Any((SitePart part) => part.def.considerEnteringAsAttack))
			{
				SettlementUtility.AffectRelationsOnAttacked(site, ref letterText);
			}
			if (!letterText.NullOrEmpty())
			{
				if (stringBuilder.Length > 0)
				{
					if (stringBuilder[stringBuilder.Length - 1] != '\n')
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.AppendLine();
				}
				stringBuilder.AppendLineTagged(letterText);
			}
			List<HediffDef> list = null;
			foreach (SitePart part in site.parts)
			{
				if (part.def.arrivedLetterHediffHyperlinks.NullOrEmpty())
				{
					continue;
				}
				if (list == null)
				{
					list = new List<HediffDef>();
				}
				foreach (HediffDef arrivedLetterHediffHyperlink in part.def.arrivedLetterHediffHyperlinks)
				{
					if (!list.Contains(arrivedLetterHediffHyperlink))
					{
						list.Add(arrivedLetterHediffHyperlink);
					}
				}
			}
			ChoiceLetter choiceLetter = LetterMaker.MakeLetter("LetterLabelCaravanEnteredMap".Translate(site), stringBuilder.ToString(), letterDef ?? LetterDefOf.NeutralEvent, allLookTargets.IsValid() ? allLookTargets : lookTargets);
			choiceLetter.hyperlinkHediffDefs = list;
			Find.LetterStack.ReceiveLetter(choiceLetter);
		}
		else
		{
			Find.LetterStack.ReceiveLetter("LetterLabelCaravanEnteredMap".Translate(site), "LetterCaravanEnteredMap".Translate(caravan.Label, site).CapitalizeFirst(), LetterDefOf.NeutralEvent, lookTargets);
		}
		CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, draftColonists);
	}

	private void AppendThreatInfo(StringBuilder sb, Site site, Map map, out LetterDef letterDef, out LookTargets allLookTargets)
	{
		allLookTargets = new LookTargets();
		tmpUsedDefs.Clear();
		tmpDefs.Clear();
		for (int i = 0; i < site.parts.Count; i++)
		{
			tmpDefs.Add(site.parts[i].def);
		}
		letterDef = null;
		for (int j = 0; j < tmpDefs.Count; j++)
		{
			LetterDef preferredLetterDef;
			LookTargets lookTargets;
			string arrivedLetterPart = tmpDefs[j].Worker.GetArrivedLetterPart(map, out preferredLetterDef, out lookTargets);
			if (arrivedLetterPart == null)
			{
				continue;
			}
			if (!tmpUsedDefs.Contains(tmpDefs[j]))
			{
				tmpUsedDefs.Add(tmpDefs[j]);
				if (sb.Length > 0)
				{
					sb.AppendLine();
					sb.AppendLine();
				}
				sb.Append(arrivedLetterPart);
			}
			if (letterDef == null)
			{
				letterDef = preferredLetterDef;
			}
			if (lookTargets.IsValid())
			{
				allLookTargets = new LookTargets(allLookTargets.targets.Concat(lookTargets.targets));
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref site, "site");
	}

	public static FloatMenuAcceptanceReport CanVisit(Caravan caravan, Site site)
	{
		if (site == null || !site.Spawned)
		{
			return false;
		}
		if (site.EnterCooldownBlocksEntering())
		{
			return FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(site.EnterCooldownTicksLeft().ToStringTicksToPeriod()));
		}
		return true;
	}

	public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, Site site)
	{
		return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(caravan, site), () => new CaravanArrivalAction_VisitSite(site), site.ApproachOrderString, caravan, site.Tile, site);
	}
}
