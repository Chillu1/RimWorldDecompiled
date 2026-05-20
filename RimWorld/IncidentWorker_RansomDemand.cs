using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class IncidentWorker_RansomDemand : IncidentWorker
{
	private const int TimeoutTicks = 60000;

	private static List<Pawn> candidates = new List<Pawn>();

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!CommsConsoleUtility.PlayerHasPoweredCommsConsole((Map)parms.target))
		{
			return false;
		}
		if (RandomKidnappedColonist() == null)
		{
			return false;
		}
		return base.CanFireNowSub(parms);
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		Pawn pawn = RandomKidnappedColonist();
		if (pawn == null)
		{
			return false;
		}
		Faction faction = FactionWhichKidnapped(pawn);
		int num = RandomFee(pawn);
		ChoiceLetter_RansomDemand choiceLetter_RansomDemand = (ChoiceLetter_RansomDemand)LetterMaker.MakeLetter(def.letterLabel, "RansomDemand".Translate(pawn.LabelShort, faction.NameColored, num, pawn.Named("PAWN")).AdjustedFor(pawn), def.letterDef);
		choiceLetter_RansomDemand.title = "RansomDemandTitle".Translate(map.Parent.Label);
		choiceLetter_RansomDemand.radioMode = true;
		choiceLetter_RansomDemand.kidnapped = pawn;
		choiceLetter_RansomDemand.faction = faction;
		choiceLetter_RansomDemand.map = map;
		choiceLetter_RansomDemand.fee = num;
		choiceLetter_RansomDemand.relatedFaction = faction;
		choiceLetter_RansomDemand.quest = parms.quest;
		choiceLetter_RansomDemand.StartTimeout(60000);
		Find.LetterStack.ReceiveLetter(choiceLetter_RansomDemand);
		return true;
	}

	private Pawn RandomKidnappedColonist()
	{
		candidates.Clear();
		List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
		for (int i = 0; i < allFactionsListForReading.Count; i++)
		{
			List<Pawn> kidnappedPawnsListForReading = allFactionsListForReading[i].kidnapped.KidnappedPawnsListForReading;
			for (int j = 0; j < kidnappedPawnsListForReading.Count; j++)
			{
				if (kidnappedPawnsListForReading[j].Faction == Faction.OfPlayer && kidnappedPawnsListForReading[j].RaceProps.Humanlike)
				{
					candidates.Add(kidnappedPawnsListForReading[j]);
				}
			}
		}
		List<Letter> lettersListForReading = Find.LetterStack.LettersListForReading;
		for (int k = 0; k < lettersListForReading.Count; k++)
		{
			if (lettersListForReading[k] is ChoiceLetter_RansomDemand choiceLetter_RansomDemand)
			{
				candidates.Remove(choiceLetter_RansomDemand.kidnapped);
			}
		}
		if (!candidates.TryRandomElement(out var result))
		{
			return null;
		}
		candidates.Clear();
		return result;
	}

	private Faction FactionWhichKidnapped(Pawn pawn)
	{
		return Find.FactionManager.AllFactionsListForReading.Find((Faction x) => x.kidnapped.KidnappedPawnsListForReading.Contains(pawn));
	}

	private int RandomFee(Pawn pawn)
	{
		return (int)(pawn.MarketValue * DiplomacyTuning.RansomFeeMarketValueFactorRange.RandomInRange);
	}
}
