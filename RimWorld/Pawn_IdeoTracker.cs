using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Pawn_IdeoTracker : IExposable
{
	public class IdeoExposureWeight : IExposable, IComparable<IdeoExposureWeight>
	{
		public Ideo ideo;

		public float exposure;

		public IdeoExposureWeight()
		{
		}

		public IdeoExposureWeight(Ideo ideo, float exposure)
		{
			this.ideo = ideo;
			this.exposure = exposure;
		}

		public int CompareTo(IdeoExposureWeight other)
		{
			return other.exposure.CompareTo(exposure);
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref ideo, "ideo");
			Scribe_Values.Look(ref exposure, "exposure", 0f);
		}
	}

	private Pawn pawn;

	public int joinTick;

	private Ideo ideo;

	private List<Ideo> previousIdeos;

	private float certaintyInt;

	private List<IdeoExposureWeight> babyIdeoExposure;

	private bool isIdeoExposureSortDirty = true;

	private int lastCacheId;

	private const float ExposurePointsPerHour = 1f;

	public const float ExposurePointsPerTick = 0.0004f;

	private static SimpleCurve pawnAgeCertaintyCurve;

	public Ideo Ideo => ideo;

	public List<Ideo> PreviousIdeos => previousIdeos;

	public float Certainty
	{
		get
		{
			if (pawn.DevelopmentalStage.Baby())
			{
				certaintyInt = 0f;
			}
			return certaintyInt;
		}
		private set
		{
			if (pawn.DevelopmentalStage.Baby())
			{
				certaintyInt = 0f;
			}
			else
			{
				certaintyInt = value;
			}
			certaintyInt = Mathf.Clamp01(certaintyInt);
		}
	}

	private float CertaintyChangeFactor
	{
		get
		{
			if (!ModsConfig.BiotechActive)
			{
				return 1f;
			}
			if (pawnAgeCertaintyCurve == null)
			{
				pawnAgeCertaintyCurve = new SimpleCurve
				{
					new CurvePoint(pawn.ageTracker.LifeStageMinAge(LifeStageDefOf.HumanlikeChild), 2f),
					new CurvePoint(pawn.ageTracker.LifeStageMinAge(LifeStageDefOf.HumanlikeAdult), 1f)
				};
			}
			return pawnAgeCertaintyCurve.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
		}
	}

	public float CertaintyChangePerDay
	{
		get
		{
			if (pawn.needs.mood != null)
			{
				return ConversionTuning.CertaintyPerDayByMoodCurve.Evaluate(pawn.needs.mood.CurLevelPercentage);
			}
			return 0f;
		}
	}

	public List<IdeoExposureWeight> BabyIdeoExposureSorted
	{
		get
		{
			if (!IsBabyIdeoExposureSorted())
			{
				babyIdeoExposure.Sort();
				isIdeoExposureSortDirty = false;
			}
			return babyIdeoExposure;
		}
	}

	public float BabyIdeoExposureTotal
	{
		get
		{
			if (babyIdeoExposure.NullOrEmpty())
			{
				return 0f;
			}
			float num = 0f;
			foreach (IdeoExposureWeight item in babyIdeoExposure)
			{
				num += item.exposure;
			}
			return num;
		}
	}

	public Pawn_IdeoTracker(Pawn pawn)
	{
		this.pawn = pawn;
		previousIdeos = new List<Ideo>();
	}

	public Pawn_IdeoTracker()
	{
	}

	private float ApplyCertaintyChangeFactor(float certaintyOffset)
	{
		if (!(certaintyOffset > 0f))
		{
			return certaintyOffset * CertaintyChangeFactor;
		}
		return certaintyOffset / CertaintyChangeFactor;
	}

	public void IdeoTrackerTickInterval(int delta)
	{
		if (!pawn.Destroyed && !pawn.InMentalState && ideo != null && !Find.IdeoManager.classicMode && !pawn.Deathresting)
		{
			Certainty += ApplyCertaintyChangeFactor(CertaintyChangePerDay / 60000f) * (float)delta;
		}
		if (ideo != null && ideo.currentCacheId != lastCacheId)
		{
			lastCacheId = ideo.currentCacheId;
			RecacheIdeoComponents();
		}
	}

	public void SetIdeo(Ideo ideo)
	{
		if (this.ideo == ideo || pawn.DevelopmentalStage.Baby())
		{
			return;
		}
		if (this.ideo != null)
		{
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.ChangedIdeo, pawn.Named(HistoryEventArgsNames.Doer)));
		}
		if (previousIdeos.Contains(ideo))
		{
			previousIdeos.Remove(ideo);
		}
		if (this.ideo != null)
		{
			previousIdeos.Add(this.ideo);
		}
		Ideo ideo2 = this.ideo;
		if (pawn.Faction != null && pawn.Faction.IsPlayer)
		{
			this.ideo?.Notify_MemberLost(pawn, pawn.Map);
			this.ideo?.RecacheColonistBelieverCount();
		}
		this.ideo = ideo;
		Certainty = Mathf.Clamp01(ConversionTuning.InitialCertaintyRange.RandomInRange);
		if (pawn.Faction != null && pawn.Faction.IsPlayer)
		{
			pawn.Faction.ideos.Notify_ColonistChangedIdeo();
			ideo2?.RecacheColonistBelieverCount();
			this.ideo?.RecacheColonistBelieverCount();
		}
		this.ideo?.Notify_MemberGained(pawn);
		if (pawn.ownership.OwnedBed != null && pawn.ownership.OwnedBed.CompAssignableToPawn.IdeoligionForbids(pawn))
		{
			pawn.ownership.UnclaimBed();
		}
		SpouseRelationUtility.RemoveSpousesAsForbiddenByIdeo(pawn);
		if (ideo != null && !ideo.MemberWillingToDo(new HistoryEvent(HistoryEventDefOf.Bonded)))
		{
			List<Pawn> list = new List<Pawn>();
			List<DirectPawnRelation> directRelations = pawn.relations.DirectRelations;
			for (int num = directRelations.Count - 1; num >= 0; num--)
			{
				DirectPawnRelation directPawnRelation = directRelations[num];
				if (directPawnRelation.def == PawnRelationDefOf.Bond)
				{
					list.Add(directPawnRelation.otherPawn);
					pawn.relations.RemoveDirectRelation(directPawnRelation);
				}
			}
			if (list.Count > 0)
			{
				Find.LetterStack.ReceiveLetter("LetterBondRemoved".Translate(), "LetterBondRemovedDesc".Translate(ideo.Named("IDEO"), pawn.Named("PAWN"), list.Select((Pawn b) => b.LabelCap).ToLineList().Named("BONDS")), LetterDefOf.NeutralEvent, new LookTargets(list.Concat(new Pawn[1] { pawn })));
			}
		}
		joinTick = Find.TickManager.TicksGame;
		RecacheIdeoComponents();
		pawn.ageTracker?.Notify_IdeoChanged();
	}

	private void RecacheIdeoComponents()
	{
		pawn.needs?.mood?.thoughts.situational.Notify_SituationalThoughtsDirty();
		pawn.needs?.AddOrRemoveNeedsAsAppropriate();
		pawn.apparel?.Notify_IdeoChanged();
		pawn.abilities?.Notify_TemporaryAbilitiesChanged();
	}

	public bool IdeoConversionAttempt(float certaintyReduction, Ideo initiatorIdeo, bool applyCertaintyFactor = true)
	{
		if (!ModLister.CheckIdeology("Ideoligion conversion") || pawn.DevelopmentalStage.Baby())
		{
			return false;
		}
		if (Find.IdeoManager.classicMode)
		{
			return false;
		}
		float num = Mathf.Clamp01(Certainty + (applyCertaintyFactor ? ApplyCertaintyChangeFactor(0f - certaintyReduction) : (0f - certaintyReduction)));
		if (pawn.Spawned)
		{
			string text = "Certainty".Translate() + "\n" + Certainty.ToStringPercent() + " -> " + num.ToStringPercent();
			MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, text, 8f);
		}
		Certainty = num;
		if (Certainty <= 0f)
		{
			bool num2 = PreviousIdeos.Contains(initiatorIdeo);
			SetIdeo(initiatorIdeo);
			Certainty = 0.5f;
			initiatorIdeo.Notify_MemberGainedByConversion();
			if (!num2)
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.ConvertedNewMember, pawn.Named(HistoryEventArgsNames.Doer), initiatorIdeo.Named(HistoryEventArgsNames.Ideo)));
			}
			return true;
		}
		return false;
	}

	public void IncreaseIdeoExposureIfBabyTick(Ideo ideo, int ticks = 1)
	{
		IncreaseIdeoExposureIfBaby(ideo, (float)ticks * 0.0004f);
	}

	public void IncreaseIdeoExposureIfBaby(Ideo ideo, float exposure)
	{
		if (!ModsConfig.BiotechActive || !ModsConfig.IdeologyActive || ideo == null || Find.IdeoManager.classicMode || !pawn.DevelopmentalStage.Baby() || (ModsConfig.AnomalyActive && ideo == Find.IdeoManager.Horaxian))
		{
			return;
		}
		if (babyIdeoExposure == null)
		{
			babyIdeoExposure = new List<IdeoExposureWeight>();
		}
		isIdeoExposureSortDirty = true;
		foreach (IdeoExposureWeight item in babyIdeoExposure)
		{
			if (item.ideo == ideo)
			{
				item.exposure += exposure;
				return;
			}
		}
		babyIdeoExposure.Add(new IdeoExposureWeight(ideo, exposure));
	}

	private bool IsBabyIdeoExposureSorted()
	{
		if (babyIdeoExposure.NullOrEmpty() || !isIdeoExposureSortDirty)
		{
			return true;
		}
		float exposure = babyIdeoExposure[0].exposure;
		foreach (IdeoExposureWeight item in babyIdeoExposure)
		{
			if (exposure < item.exposure)
			{
				return false;
			}
			exposure = item.exposure;
		}
		isIdeoExposureSortDirty = false;
		return true;
	}

	public bool TryJoinIdeoFromExposures()
	{
		if (Ideo != null)
		{
			return false;
		}
		Ideo ideo;
		if (!babyIdeoExposure.NullOrEmpty() && !Find.IdeoManager.classicMode)
		{
			babyIdeoExposure.TryRandomElementByWeight((IdeoExposureWeight i) => i.exposure, out var result);
			ideo = result.ideo;
		}
		else
		{
			ideo = FallbackIdeo();
		}
		SetIdeo(ideo);
		if (Ideo != null)
		{
			babyIdeoExposure = null;
			return true;
		}
		return false;
	}

	public void Reassure(float certaintyGain)
	{
		OffsetCertainty(certaintyGain);
	}

	public void OffsetCertainty(float offset)
	{
		if (ModLister.CheckIdeology("Ideoligion certainty") && !Find.IdeoManager.classicMode)
		{
			float num = Mathf.Clamp01(Certainty + ApplyCertaintyChangeFactor(offset));
			if (pawn.Spawned)
			{
				string text = "Certainty".Translate() + "\n" + Certainty.ToStringPercent() + " -> " + num.ToStringPercent();
				MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, text, 8f);
			}
			Certainty = num;
		}
	}

	public void Notify_IdeoRemoved(Ideo removedIdeo)
	{
		if (Ideo == removedIdeo)
		{
			SetIdeo(FallbackIdeo(removedIdeo));
		}
		if (previousIdeos.Contains(removedIdeo))
		{
			previousIdeos.Remove(removedIdeo);
		}
	}

	private Ideo FallbackIdeo(Ideo excludeIdeo = null)
	{
		if (pawn.Faction?.ideos != null && pawn.Faction.ideos.PrimaryIdeo != excludeIdeo && pawn.Faction.ideos.PrimaryIdeo != null)
		{
			return pawn.Faction.ideos.PrimaryIdeo;
		}
		return Find.IdeoManager.IdeosListForReading.RandomElement();
	}

	public void Debug_ReduceCertainty(float amt)
	{
		Certainty -= amt;
	}

	public void ExposeData()
	{
		Scribe_References.Look(ref ideo, "ideo");
		Scribe_Collections.Look(ref previousIdeos, "previousIdeos", LookMode.Reference);
		Scribe_Values.Look(ref certaintyInt, "certainty", 0f);
		Scribe_Values.Look(ref joinTick, "joinTick", 0);
		Scribe_Collections.Look(ref babyIdeoExposure, "babyIdeoExposure", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (previousIdeos.RemoveAll((Ideo x) => x == null) != 0)
			{
				Log.Error("Removed null ideos");
			}
			if (Certainty <= 0f)
			{
				Certainty = Mathf.Clamp01(ConversionTuning.InitialCertaintyRange.RandomInRange);
			}
			if (Ideo == null && pawn.ShouldHaveIdeo)
			{
				Ideo arg = FallbackIdeo();
				Log.Warning($"{pawn.ToStringSafe()} did not have an ideo set; assigning fallback ideo {arg}.");
				SetIdeo(arg);
			}
		}
	}
}
