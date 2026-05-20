using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet;

public class Caravan_BedsTracker : IExposable
{
	public Caravan caravan;

	private Dictionary<Pawn, Building_Bed> usedBeds = new Dictionary<Pawn, Building_Bed>();

	private static List<Building_Bed> tmpUsableBeds = new List<Building_Bed>();

	private static List<string> tmpPawnLabels = new List<string>();

	public Caravan_BedsTracker()
	{
	}

	public Caravan_BedsTracker(Caravan caravan)
	{
		this.caravan = caravan;
	}

	public void BedsTrackerTickInterval(int delta)
	{
		RecalculateUsedBeds();
		foreach (KeyValuePair<Pawn, Building_Bed> usedBed in usedBeds)
		{
			PawnUtility.GainComfortFromThingIfPossible(usedBed.Key, usedBed.Value, delta);
		}
	}

	public void ExposeData()
	{
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			RecalculateUsedBeds();
		}
	}

	private void RecalculateUsedBeds()
	{
		usedBeds.Clear();
		if (!caravan.Spawned)
		{
			return;
		}
		tmpUsableBeds.Clear();
		GetUsableBeds(tmpUsableBeds);
		if (!caravan.pather.MovingNow)
		{
			tmpUsableBeds.SortByDescending((Building_Bed x) => x.GetStatValue(StatDefOf.BedRestEffectiveness));
			for (int num = 0; num < caravan.pawns.Count; num++)
			{
				Pawn pawn = caravan.pawns[num];
				if (pawn.needs != null && pawn.needs.rest != null)
				{
					Building_Bed andRemoveFirstAvailableBedFor = GetAndRemoveFirstAvailableBedFor(pawn, tmpUsableBeds);
					if (andRemoveFirstAvailableBedFor != null)
					{
						usedBeds.Add(pawn, andRemoveFirstAvailableBedFor);
					}
				}
			}
		}
		else
		{
			tmpUsableBeds.SortByDescending((Building_Bed x) => x.GetStatValue(StatDefOf.ImmunityGainSpeedFactor));
			for (int num2 = 0; num2 < caravan.pawns.Count; num2++)
			{
				Pawn pawn2 = caravan.pawns[num2];
				if (pawn2.needs != null && pawn2.needs.rest != null && CaravanBedUtility.WouldBenefitFromRestingInBed(pawn2) && (!caravan.pather.MovingNow || pawn2.CarriedByCaravan()))
				{
					Building_Bed andRemoveFirstAvailableBedFor2 = GetAndRemoveFirstAvailableBedFor(pawn2, tmpUsableBeds);
					if (andRemoveFirstAvailableBedFor2 != null)
					{
						usedBeds.Add(pawn2, andRemoveFirstAvailableBedFor2);
					}
				}
			}
		}
		tmpUsableBeds.Clear();
	}

	public void Notify_CaravanSpawned()
	{
		RecalculateUsedBeds();
	}

	public void Notify_PawnRemoved()
	{
		RecalculateUsedBeds();
	}

	public Building_Bed GetBedUsedBy(Pawn p)
	{
		if (usedBeds.TryGetValue(p, out var value) && !value.DestroyedOrNull())
		{
			return value;
		}
		return null;
	}

	public bool IsInBed(Pawn p)
	{
		return GetBedUsedBy(p) != null;
	}

	public int GetUsedBedCount()
	{
		return usedBeds.Count;
	}

	private void GetUsableBeds(List<Building_Bed> outBeds)
	{
		outBeds.Clear();
		List<Thing> list = CaravanInventoryUtility.AllInventoryItems(caravan);
		for (int i = 0; i < list.Count; i++)
		{
			if (!(list[i].GetInnerIfMinified() is Building_Bed building_Bed) || !building_Bed.def.building.bed_caravansCanUse)
			{
				continue;
			}
			for (int j = 0; j < list[i].stackCount; j++)
			{
				for (int k = 0; k < building_Bed.SleepingSlotsCount; k++)
				{
					outBeds.Add(building_Bed);
				}
			}
		}
	}

	private Building_Bed GetAndRemoveFirstAvailableBedFor(Pawn p, List<Building_Bed> beds)
	{
		for (int i = 0; i < beds.Count; i++)
		{
			if (RestUtility.CanUseBedEver(p, beds[i].def))
			{
				Building_Bed result = beds[i];
				beds.RemoveAt(i);
				return result;
			}
		}
		return null;
	}

	public string GetInBedForMedicalReasonsInspectStringLine()
	{
		if (usedBeds.Count == 0)
		{
			return null;
		}
		tmpPawnLabels.Clear();
		foreach (KeyValuePair<Pawn, Building_Bed> usedBed in usedBeds)
		{
			if (!caravan.carryTracker.IsCarried(usedBed.Key) && CaravanBedUtility.WouldBenefitFromRestingInBed(usedBed.Key))
			{
				tmpPawnLabels.Add(usedBed.Key.LabelShort);
			}
		}
		if (!tmpPawnLabels.Any())
		{
			return null;
		}
		string text = ((tmpPawnLabels.Count > 5) ? (tmpPawnLabels.Take(5).ToCommaList() + "...") : tmpPawnLabels.ToCommaList(useAnd: true));
		tmpPawnLabels.Clear();
		return "UsingBedrollsDueToIllness".Translate() + ": " + text;
	}
}
