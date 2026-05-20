using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet;

public class Caravan_CarryTracker : IExposable
{
	public Caravan caravan;

	private List<Pawn> carriedPawns = new List<Pawn>();

	private static List<Pawn> tmpPawnsWhoCanCarry = new List<Pawn>();

	private static readonly List<string> tmpPawnLabels = new List<string>();

	public List<Pawn> CarriedPawnsListForReading => carriedPawns;

	public Caravan_CarryTracker()
	{
	}

	public Caravan_CarryTracker(Caravan caravan)
	{
		this.caravan = caravan;
	}

	public void CarryTrackerTickInterval(int delta)
	{
		RecalculateCarriedPawns();
	}

	public void ExposeData()
	{
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			RecalculateCarriedPawns();
		}
	}

	public bool IsCarried(Pawn p)
	{
		return carriedPawns.Contains(p);
	}

	private void RecalculateCarriedPawns()
	{
		carriedPawns.Clear();
		if (!caravan.Spawned || !caravan.pather.MovingNow)
		{
			return;
		}
		tmpPawnsWhoCanCarry.Clear();
		CalculatePawnsWhoCanCarry(tmpPawnsWhoCanCarry);
		for (int i = 0; i < caravan.pawns.Count; i++)
		{
			if (!tmpPawnsWhoCanCarry.Any())
			{
				break;
			}
			Pawn pawn = caravan.pawns[i];
			if (WantsToBeCarried(pawn) && tmpPawnsWhoCanCarry.Any())
			{
				carriedPawns.Add(pawn);
				tmpPawnsWhoCanCarry.RemoveLast();
			}
		}
		tmpPawnsWhoCanCarry.Clear();
	}

	public void Notify_CaravanSpawned()
	{
		RecalculateCarriedPawns();
	}

	public void Notify_PawnRemoved()
	{
		RecalculateCarriedPawns();
	}

	private void CalculatePawnsWhoCanCarry(List<Pawn> outPawns)
	{
		outPawns.Clear();
		for (int i = 0; i < caravan.pawns.Count; i++)
		{
			Pawn pawn = caravan.pawns[i];
			if (pawn.RaceProps.Humanlike && !pawn.Downed && !pawn.InMentalState && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && !WantsToBeCarried(pawn))
			{
				outPawns.Add(pawn);
			}
		}
	}

	private bool WantsToBeCarried(Pawn p)
	{
		if (p.health.beCarriedByCaravanIfSick)
		{
			return CaravanCarryUtility.WouldBenefitFromBeingCarried(p);
		}
		return false;
	}

	public string GetInspectStringLine()
	{
		if (!carriedPawns.Any())
		{
			return null;
		}
		tmpPawnLabels.Clear();
		int num = 0;
		for (int i = 0; i < carriedPawns.Count; i++)
		{
			tmpPawnLabels.Add(carriedPawns[i].LabelShort);
			if (caravan.beds.IsInBed(carriedPawns[i]))
			{
				num++;
			}
		}
		string str = ((tmpPawnLabels.Count > 5) ? (tmpPawnLabels.Take(5).ToCommaList() + "...") : tmpPawnLabels.ToCommaList(useAnd: true));
		string result = CaravanBedUtility.AppendUsingBedsLabel("BeingCarriedDueToIllness".Translate() + ": " + str.CapitalizeFirst(), num);
		tmpPawnLabels.Clear();
		return result;
	}
}
