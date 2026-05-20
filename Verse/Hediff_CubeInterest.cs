using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;

namespace Verse;

public class Hediff_CubeInterest : Hediff
{
	private List<Thing> sculptures = new List<Thing>();

	private Hediff_CubeWithdrawal cachedHediff;

	private const float MTBPlayWantedDays = 1.5f;

	private const int InitialDelayTicks = 1000;

	private const int SculptureUpdateRateTicks = 60;

	private static readonly FloatRange SeverityPerPlay = new FloatRange(0.045f, 0.105f);

	private static readonly SimpleCurve MentalBreakSeverityDaysMTB = new SimpleCurve
	{
		new CurvePoint(0.01f, 5f),
		new CurvePoint(0.5f, 3f),
		new CurvePoint(0.8f, 2f)
	};

	private static readonly SimpleCurve CubeRageSeverityIncrease = new SimpleCurve
	{
		new CurvePoint(0.1f, 0.1f),
		new CurvePoint(0.5f, 0.2f),
		new CurvePoint(1f, 0.4f)
	};

	public Hediff_CubeWithdrawal WithdrawalHediff => cachedHediff ?? (cachedHediff = pawn.health.hediffSet.GetFirstHediff<Hediff_CubeWithdrawal>());

	public bool InWithdrawal => pawn.health.hediffSet.HasHediff(HediffDefOf.CubeWithdrawal);

	public int Sculptures => sculptures.Count;

	public bool InCaravanWithCube
	{
		get
		{
			Caravan caravan = pawn.GetCaravan();
			if (caravan == null)
			{
				return false;
			}
			return CaravanInventoryUtility.HasThings(caravan, ThingDefOf.GoldenCube, 1);
		}
	}

	public override void PostAdd(DamageInfo? dinfo)
	{
		if (!ModLister.CheckAnomaly("Cube interest"))
		{
			pawn.health.RemoveHediff(this);
		}
		else
		{
			base.PostAdd(dinfo);
		}
	}

	public override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		if (ageTicks > 1000)
		{
			if (Rand.MTBEventOccurs(1.5f, 60000f, 1f) && !InWithdrawal && !InCaravanWithCube)
			{
				StartWithdrawal();
			}
			if (Rand.MTBEventOccurs(MentalBreakSeverityDaysMTB.Evaluate(Severity), 60000f, 1f) && !pawn.InMentalState && pawn.CurJobDef != JobDefOf.GoldenCubePlay && pawn.SpawnedOrAnyParentSpawned)
			{
				DoMentalBreak();
			}
			if (GenTicks.TicksGame % 60 == 0)
			{
				UpdateSculptures();
			}
		}
	}

	private void UpdateSculptures()
	{
		bool flag = false;
		for (int num = sculptures.Count - 1; num >= 0; num--)
		{
			if (sculptures[num].DestroyedOrNull() || (!sculptures[num].SpawnedOrAnyParentSpawned && !sculptures[num].IsInCaravan()))
			{
				flag = true;
				pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.DestroyedCubeSculpture);
				sculptures.RemoveAt(num);
				float num2 = CubeRageSeverityIncrease.Evaluate(Severity);
				if (num2 > 0f)
				{
					HealthUtility.AdjustSeverity(pawn, HediffDefOf.CubeRage, num2);
				}
			}
		}
		if (flag)
		{
			Messages.Message("MessageGoldenCubeSculptureDestroyed".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.NeutralEvent);
		}
	}

	public override void PostRemoved()
	{
		base.PostRemoved();
		if (pawn.health.hediffSet.TryGetHediff(HediffDefOf.CubeRage, out var hediff))
		{
			pawn.health.RemoveHediff(hediff);
		}
	}

	protected override void OnStageIndexChanged(int stageIndex)
	{
		if (PawnUtility.ShouldSendNotificationAbout(pawn))
		{
			Messages.Message("MessageGoldenCubeSeverityIncreased".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.NeutralEvent);
		}
	}

	public void Notify_BuiltSculpture(Thing sculpture)
	{
		sculptures.Add(sculpture);
	}

	public void Notify_PlayedWith()
	{
		if (InWithdrawal)
		{
			pawn.health.RemoveHediff(WithdrawalHediff);
			cachedHediff = null;
		}
		Severity += SeverityPerPlay.RandomInRange;
		pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.PlayedWithCube);
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		if (DebugSettings.ShowDevGizmos)
		{
			if (!InWithdrawal)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEV: Cube withdrawal",
					action = StartWithdrawal
				};
			}
			yield return new Command_Action
			{
				defaultLabel = "DEV: Cube obsession",
				action = DoMentalBreak
			};
		}
	}

	private void DoMentalBreak()
	{
		if (!pawn.InMentalState)
		{
			pawn.mindState.mentalBreaker.TryDoMentalBreak("MentalBreakReason_CubeSculpting".Translate(pawn.Named("PAWN")), MentalBreakDefOf.CubeSculpting);
		}
	}

	private void StartWithdrawal()
	{
		if (!InWithdrawal)
		{
			cachedHediff = (Hediff_CubeWithdrawal)pawn.health.AddHediff(HediffDefOf.CubeWithdrawal);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref sculptures, "sculptures", LookMode.Reference);
	}
}
