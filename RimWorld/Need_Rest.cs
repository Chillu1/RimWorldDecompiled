using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class Need_Rest : Need
	{
		private int lastRestTick = -999;

		private float lastRestEffectiveness = 1f;

		private int ticksAtZero;

		private const float FullSleepHours = 10.5f;

		public const float BaseRestGainPerTick = 3.809524E-05f;

		private const float BaseRestFallPerTick = 1.58333332E-05f;

		public const float ThreshTired = 0.28f;

		public const float ThreshVeryTired = 0.14f;

		public const float DefaultFallAsleepMaxLevel = 0.75f;

		public const float DefaultNaturalWakeThreshold = 1f;

		public const float CanWakeThreshold = 0.2f;

		private const float BaseInvoluntarySleepMTBDays = 0.25f;

		public RestCategory CurCategory
		{
			get
			{
				if (CurLevel < 0.01f)
				{
					return RestCategory.Exhausted;
				}
				if (CurLevel < 0.14f)
				{
					return RestCategory.VeryTired;
				}
				if (CurLevel < 0.28f)
				{
					return RestCategory.Tired;
				}
				return RestCategory.Rested;
			}
		}

		public float RestFallPerTick
		{
			get
			{
				switch (CurCategory)
				{
				case RestCategory.Rested:
					return 1.58333332E-05f * RestFallFactor;
				case RestCategory.Tired:
					return 1.58333332E-05f * RestFallFactor * 0.7f;
				case RestCategory.VeryTired:
					return 1.58333332E-05f * RestFallFactor * 0.3f;
				case RestCategory.Exhausted:
					return 1.58333332E-05f * RestFallFactor * 0.6f;
				default:
					return 999f;
				}
			}
		}

		private float RestFallFactor => pawn.health.hediffSet.RestFallFactor;

		public override int GUIChangeArrow
		{
			get
			{
				if (Resting)
				{
					return 1;
				}
				return -1;
			}
		}

		public int TicksAtZero => ticksAtZero;

		private bool Resting => Find.TickManager.TicksGame < lastRestTick + 2;

		public Need_Rest(Pawn pawn)
			: base(pawn)
		{
			threshPercents = new List<float>();
			threshPercents.Add(0.28f);
			threshPercents.Add(0.14f);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref ticksAtZero, "ticksAtZero", 0);
		}

		public override void SetInitialLevel()
		{
			CurLevel = Rand.Range(0.9f, 1f);
		}

		public override void NeedInterval()
		{
			if (!IsFrozen)
			{
				if (Resting)
				{
					float num = lastRestEffectiveness;
					num *= pawn.GetStatValue(StatDefOf.RestRateMultiplier);
					if (num > 0f)
					{
						CurLevel += 0.005714286f * num;
					}
				}
				else
				{
					CurLevel -= RestFallPerTick * 150f;
				}
			}
			if (CurLevel < 0.0001f)
			{
				ticksAtZero += 150;
			}
			else
			{
				ticksAtZero = 0;
			}
			if (ticksAtZero <= 1000 || !pawn.Spawned)
			{
				return;
			}
			float mtb = (ticksAtZero < 15000) ? 0.25f : ((ticksAtZero < 30000) ? 0.125f : ((ticksAtZero >= 45000) ? 0.0625f : 0.0833333358f));
			if (Rand.MTBEventOccurs(mtb, 60000f, 150f) && (pawn.CurJob == null || pawn.CurJob.def != JobDefOf.LayDown))
			{
				pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.LayDown, pawn.Position), JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, JobTag.SatisfyingNeeds);
				if (pawn.InMentalState)
				{
					pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
				}
				if (PawnUtility.ShouldSendNotificationAbout(pawn))
				{
					Messages.Message("MessageInvoluntarySleep".Translate(pawn.LabelShort, pawn), pawn, MessageTypeDefOf.NegativeEvent);
				}
				TaleRecorder.RecordTale(TaleDefOf.Exhausted, pawn);
			}
		}

		public void TickResting(float restEffectiveness)
		{
			if (!(restEffectiveness <= 0f))
			{
				lastRestTick = Find.TickManager.TicksGame;
				lastRestEffectiveness = restEffectiveness;
			}
		}
	}
}
