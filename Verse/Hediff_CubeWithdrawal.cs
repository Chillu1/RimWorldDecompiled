using RimWorld;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;

namespace Verse;

public class Hediff_CubeWithdrawal : Hediff
{
	private Hediff_CubeInterest cachedHediff;

	private static readonly SimpleCurve SeverityPerHour = new SimpleCurve(new CurvePoint[4]
	{
		new CurvePoint(0f, -0.25f),
		new CurvePoint(0.1f, 0.0138f),
		new CurvePoint(0.5f, 0.0208f),
		new CurvePoint(1f, 0.0416f)
	});

	private static readonly SimpleCurve ComaSeverityDaysCurve = new SimpleCurve(new CurvePoint[3]
	{
		new CurvePoint(0.1f, 3f),
		new CurvePoint(0.5f, 10f),
		new CurvePoint(1f, 24f)
	});

	public Hediff_CubeInterest InterestHediff => cachedHediff ?? (cachedHediff = pawn.health.hediffSet.GetFirstHediff<Hediff_CubeInterest>());

	public override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		if (InterestHediff.InCaravanWithCube)
		{
			pawn.health.RemoveHediff(this);
		}
		else if (pawn.CurJobDef != JobDefOf.GoldenCubePlay && pawn.Awake())
		{
			float x = InterestHediff?.Severity ?? 0f;
			Severity += SeverityPerHour.Evaluate(x) / 2500f;
			if (Severity >= 1f && PawnUtility.ShouldSendNotificationAbout(pawn))
			{
				((HediffWithComps)pawn.health.AddHediff(HediffDefOf.CubeComa)).GetComp<HediffComp_Disappears>().ticksToDisappear = Mathf.RoundToInt(ComaSeverityDaysCurve.Evaluate(x) * 60000f);
				pawn.health.RemoveHediff(InterestHediff);
				pawn.health.RemoveHediff(this);
				TaggedString label = "LetterLabelGoldenCubeComa".Translate(pawn.Named("PAWN"));
				TaggedString text = "LetterGoldenCubeComa".Translate(pawn.Named("PAWN"));
				Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NegativeEvent, pawn);
			}
		}
	}

	protected override void OnStageIndexChanged(int stageIndex)
	{
		if (!PawnUtility.ShouldSendNotificationAbout(pawn))
		{
			return;
		}
		string text;
		if (stageIndex == 1)
		{
			text = "MessageGoldenCubeWithdrawal".Translate(pawn.Named("PAWN"));
		}
		else
		{
			text = "MessageGoldenCubeWithdrawalIncreased".Translate(pawn.Named("PAWN"));
			if (!pawn.InMentalState && !pawn.Drafted && pawn.GetLord() == null && pawn.CurJobDef != JobDefOf.GoldenCubePlay && JobGiver_PlayWithGoldenCube.TryGetNearestCube(pawn, out var cube) && !pawn.Downed)
			{
				Job job = JobMaker.MakeJob(JobDefOf.GoldenCubePlay, cube);
				job.count = 1;
				pawn.jobs.StartJob(job, JobCondition.InterruptForced);
			}
		}
		Messages.Message(text, pawn, MessageTypeDefOf.NegativeHealthEvent);
	}
}
