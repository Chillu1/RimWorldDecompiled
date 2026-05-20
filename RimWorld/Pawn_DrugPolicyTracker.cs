using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Pawn_DrugPolicyTracker : IExposable
{
	public Pawn pawn;

	private DrugPolicy curPolicy;

	private List<DrugTakeRecord> drugTakeRecords = new List<DrugTakeRecord>();

	private const float DangerousDrugOverdoseSeverity = 0.5f;

	public DrugPolicy CurrentPolicy
	{
		get
		{
			if (pawn.IsMutant && pawn.mutant.Def.disablePolicies)
			{
				return null;
			}
			if (curPolicy == null)
			{
				curPolicy = Current.Game.drugPolicyDatabase.DefaultDrugPolicy();
			}
			return curPolicy;
		}
		set
		{
			if (curPolicy != value)
			{
				curPolicy = value;
			}
		}
	}

	private float DayPercentNotSleeping
	{
		get
		{
			if (pawn.IsCaravanMember())
			{
				return Mathf.InverseLerp(6f, 22f, GenLocalDate.HourFloat(pawn));
			}
			if (pawn.timetable == null)
			{
				return GenLocalDate.DayPercent(pawn);
			}
			float hoursPerDayNotSleeping = HoursPerDayNotSleeping;
			if (hoursPerDayNotSleeping == 0f)
			{
				return 1f;
			}
			float num = 0f;
			int num2 = GenLocalDate.HourOfDay(pawn);
			for (int i = 0; i < num2; i++)
			{
				if (pawn.timetable.times[i] != TimeAssignmentDefOf.Sleep)
				{
					num += 1f;
				}
			}
			if (pawn.timetable.CurrentAssignment != TimeAssignmentDefOf.Sleep)
			{
				float num3 = (float)(Find.TickManager.TicksAbs % 2500) / 2500f;
				num += num3;
			}
			return num / hoursPerDayNotSleeping;
		}
	}

	private float HoursPerDayNotSleeping
	{
		get
		{
			if (pawn.IsCaravanMember())
			{
				return 16f;
			}
			int num = 0;
			for (int i = 0; i < 24; i++)
			{
				if (pawn.timetable.times[i] != TimeAssignmentDefOf.Sleep)
				{
					num++;
				}
			}
			return num;
		}
	}

	public Pawn_DrugPolicyTracker()
	{
	}

	public Pawn_DrugPolicyTracker(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void ExposeData()
	{
		Scribe_References.Look(ref curPolicy, "curAssignedDrugs");
		Scribe_Collections.Look(ref drugTakeRecords, "drugTakeRecords", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && drugTakeRecords.RemoveAll((DrugTakeRecord x) => x.drug == null) != 0)
		{
			Log.ErrorOnce("Removed some null drugs from drug policy tracker", 816929737);
		}
	}

	public bool HasEverTaken(ThingDef drug)
	{
		if (!drug.IsDrug)
		{
			Log.Warning(drug?.ToString() + " is not a drug.");
			return false;
		}
		return drugTakeRecords.Any((DrugTakeRecord x) => x.drug == drug);
	}

	public bool AllowedToTakeToInventory(ThingDef thingDef)
	{
		if (!thingDef.IsIngestible)
		{
			Log.Error(thingDef?.ToString() + " is not ingestible.");
			return false;
		}
		if (!thingDef.IsDrug)
		{
			Log.Error("AllowedToTakeScheduledEver on non-drug " + thingDef);
			return false;
		}
		if (thingDef.IsNonMedicalDrug && !pawn.CanTakeDrug(thingDef))
		{
			return false;
		}
		DrugPolicyEntry drugPolicyEntry = CurrentPolicy[thingDef];
		if (drugPolicyEntry.allowScheduled)
		{
			return false;
		}
		if (drugPolicyEntry.takeToInventory > 0)
		{
			return !pawn.inventory.innerContainer.Contains(thingDef);
		}
		return false;
	}

	public bool AllowedToTakeScheduledEver(ThingDef thingDef)
	{
		if (!thingDef.IsIngestible)
		{
			Log.Error(thingDef?.ToString() + " is not ingestible.");
			return false;
		}
		if (!thingDef.IsDrug)
		{
			Log.Error("AllowedToTakeScheduledEver on non-drug " + thingDef);
			return false;
		}
		if (!CurrentPolicy[thingDef].allowScheduled)
		{
			return false;
		}
		if (thingDef.IsNonMedicalDrug && !pawn.CanTakeDrug(thingDef))
		{
			return false;
		}
		if (thingDef.ingestible.drugCategory == DrugCategory.Hard && !new HistoryEvent(HistoryEventDefOf.IngestedHardDrug, pawn.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
		{
			return false;
		}
		return true;
	}

	public bool AllowedToTakeScheduledNow(ThingDef thingDef)
	{
		if (!thingDef.IsIngestible)
		{
			Log.Error(thingDef?.ToString() + " is not ingestible.");
			return false;
		}
		if (!thingDef.IsDrug)
		{
			Log.Error("AllowedToTakeScheduledEver on non-drug " + thingDef);
			return false;
		}
		if (!AllowedToTakeScheduledEver(thingDef))
		{
			return false;
		}
		DrugPolicyEntry drugPolicyEntry = CurrentPolicy[thingDef];
		if (drugPolicyEntry.onlyIfMoodBelow < 1f && pawn.needs.mood != null && pawn.needs.mood.CurLevelPercentage >= drugPolicyEntry.onlyIfMoodBelow)
		{
			return false;
		}
		if (drugPolicyEntry.onlyIfJoyBelow < 1f && pawn.needs.joy != null && pawn.needs.joy.CurLevelPercentage >= drugPolicyEntry.onlyIfJoyBelow)
		{
			return false;
		}
		DrugTakeRecord drugTakeRecord = drugTakeRecords.Find((DrugTakeRecord x) => x.drug == thingDef);
		if (drugTakeRecord != null)
		{
			if (drugPolicyEntry.daysFrequency < 1f)
			{
				int num = Mathf.RoundToInt(1f / drugPolicyEntry.daysFrequency);
				if (drugTakeRecord.TimesTakenThisDay >= num)
				{
					return false;
				}
			}
			else
			{
				int num2 = Mathf.Abs(GenDate.DaysPassed - drugTakeRecord.LastTakenDays);
				int num3 = Mathf.RoundToInt(drugPolicyEntry.daysFrequency);
				if (num2 < num3)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool ShouldTryToTakeScheduledNow(ThingDef ingestible)
	{
		if (!ingestible.IsDrug)
		{
			return false;
		}
		if (!AllowedToTakeScheduledNow(ingestible))
		{
			return false;
		}
		Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.DrugOverdose);
		if (firstHediffOfDef != null && firstHediffOfDef.Severity > 0.5f && CanCauseOverdose(ingestible))
		{
			int num = LastTicksWhenTakenDrugWhichCanCauseOverdose();
			if (Find.TickManager.TicksGame - num < 1250)
			{
				return false;
			}
		}
		DrugTakeRecord drugTakeRecord = drugTakeRecords.Find((DrugTakeRecord x) => x.drug == ingestible);
		if (drugTakeRecord == null)
		{
			return true;
		}
		DrugPolicyEntry drugPolicyEntry = CurrentPolicy[ingestible];
		if (drugPolicyEntry.daysFrequency < 1f)
		{
			int num2 = Mathf.RoundToInt(1f / drugPolicyEntry.daysFrequency);
			float num3 = 1f / (float)(num2 + 1);
			int num4 = 0;
			float dayPercentNotSleeping = DayPercentNotSleeping;
			for (int num5 = 0; num5 < num2; num5++)
			{
				if (dayPercentNotSleeping > (float)(num5 + 1) * num3 - num3 * 0.5f)
				{
					num4++;
				}
			}
			if (drugTakeRecord.TimesTakenThisDay >= num4)
			{
				return false;
			}
			if (drugTakeRecord.TimesTakenThisDay != 0 && (float)(Find.TickManager.TicksGame - drugTakeRecord.lastTakenTicks) / (HoursPerDayNotSleeping * 2500f) < 0.6f * num3)
			{
				return false;
			}
			return true;
		}
		float dayPercentNotSleeping2 = DayPercentNotSleeping;
		Rand.PushState();
		Rand.Seed = Gen.HashCombineInt(GenDate.DaysPassed, pawn.thingIDNumber);
		bool result = dayPercentNotSleeping2 >= Rand.Range(0.1f, 0.35f);
		Rand.PopState();
		return result;
	}

	public void Notify_DrugIngested(Thing drug)
	{
		DrugTakeRecord drugTakeRecord = drugTakeRecords.Find((DrugTakeRecord x) => x.drug == drug.def);
		if (drugTakeRecord == null)
		{
			drugTakeRecord = new DrugTakeRecord();
			drugTakeRecord.drug = drug.def;
			drugTakeRecords.Add(drugTakeRecord);
		}
		drugTakeRecord.lastTakenTicks = Find.TickManager.TicksGame;
		drugTakeRecord.TimesTakenThisDay++;
	}

	public void Notify_LeftSuspension(int suspendedTicks)
	{
		foreach (DrugTakeRecord drugTakeRecord in drugTakeRecords)
		{
			drugTakeRecord.Notify_LeftSuspension(suspendedTicks);
		}
	}

	private int LastTicksWhenTakenDrugWhichCanCauseOverdose()
	{
		int num = -999999;
		for (int i = 0; i < drugTakeRecords.Count; i++)
		{
			if (CanCauseOverdose(drugTakeRecords[i].drug))
			{
				num = Mathf.Max(num, drugTakeRecords[i].lastTakenTicks);
			}
		}
		return num;
	}

	private bool CanCauseOverdose(ThingDef drug)
	{
		return drug.GetCompProperties<CompProperties_Drug>()?.CanCauseOverdose ?? false;
	}
}
