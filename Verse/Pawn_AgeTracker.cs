using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public class Pawn_AgeTracker : IExposable
	{
		private Pawn pawn;

		private long ageBiologicalTicksInt = -1L;

		private long birthAbsTicksInt = -1L;

		private int cachedLifeStageIndex = -1;

		private long nextLifeStageChangeTick = -1L;

		private const float BornAtLongitude = 0f;

		public long BirthAbsTicks
		{
			get
			{
				return birthAbsTicksInt;
			}
			set
			{
				birthAbsTicksInt = value;
			}
		}

		public int AgeBiologicalYears => (int)(ageBiologicalTicksInt / 3600000);

		public float AgeBiologicalYearsFloat => (float)ageBiologicalTicksInt / 3600000f;

		public long AgeBiologicalTicks
		{
			get
			{
				return ageBiologicalTicksInt;
			}
			set
			{
				ageBiologicalTicksInt = value;
				cachedLifeStageIndex = -1;
			}
		}

		public long AgeChronologicalTicks
		{
			get
			{
				return GenTicks.TicksAbs - birthAbsTicksInt;
			}
			set
			{
				BirthAbsTicks = GenTicks.TicksAbs - value;
			}
		}

		public int AgeChronologicalYears => (int)(AgeChronologicalTicks / 3600000);

		public float AgeChronologicalYearsFloat => (float)AgeChronologicalTicks / 3600000f;

		public int BirthYear => GenDate.Year(birthAbsTicksInt, 0f);

		public int BirthDayOfSeasonZeroBased => GenDate.DayOfSeason(birthAbsTicksInt, 0f);

		public int BirthDayOfYear => GenDate.DayOfYear(birthAbsTicksInt, 0f);

		public Quadrum BirthQuadrum => GenDate.Quadrum(birthAbsTicksInt, 0f);

		public string AgeNumberString
		{
			get
			{
				string text = AgeBiologicalYearsFloat.ToStringApproxAge();
				if (AgeChronologicalYears != AgeBiologicalYears)
				{
					text = text + " (" + AgeChronologicalYears + ")";
				}
				return text;
			}
		}

		public string AgeTooltipString
		{
			get
			{
				ageBiologicalTicksInt.TicksToPeriod(out var years, out var quadrums, out var days, out var hoursFloat);
				(GenTicks.TicksAbs - birthAbsTicksInt).TicksToPeriod(out var years2, out var quadrums2, out var days2, out hoursFloat);
				string value = "FullDate".Translate(Find.ActiveLanguageWorker.OrdinalNumber(BirthDayOfSeasonZeroBased + 1), BirthQuadrum.Label(), BirthYear);
				string text = "Born".Translate(value) + "\n" + "AgeChronological".Translate(years2, quadrums2, days2) + "\n" + "AgeBiological".Translate(years, quadrums, days);
				if (Prefs.DevMode)
				{
					text += "\n\nDev mode info:";
					text = text + "\nageBiologicalTicksInt: " + ageBiologicalTicksInt;
					text = text + "\nbirthAbsTicksInt: " + birthAbsTicksInt;
					text = text + "\nnextLifeStageChangeTick: " + nextLifeStageChangeTick;
				}
				return text;
			}
		}

		public int CurLifeStageIndex
		{
			get
			{
				if (cachedLifeStageIndex < 0)
				{
					RecalculateLifeStageIndex();
				}
				return cachedLifeStageIndex;
			}
		}

		public LifeStageDef CurLifeStage => CurLifeStageRace.def;

		public LifeStageAge CurLifeStageRace => pawn.RaceProps.lifeStageAges[CurLifeStageIndex];

		public PawnKindLifeStage CurKindLifeStage
		{
			get
			{
				if (pawn.RaceProps.Humanlike)
				{
					Log.ErrorOnce("Tried to get CurKindLifeStage from humanlike pawn " + pawn, 8888811);
					return null;
				}
				return pawn.kindDef.lifeStages[CurLifeStageIndex];
			}
		}

		public Pawn_AgeTracker(Pawn newPawn)
		{
			pawn = newPawn;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref ageBiologicalTicksInt, "ageBiologicalTicks", 0L);
			Scribe_Values.Look(ref birthAbsTicksInt, "birthAbsTicks", 0L);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				cachedLifeStageIndex = -1;
			}
		}

		public void AgeTick()
		{
			ageBiologicalTicksInt++;
			if (Find.TickManager.TicksGame >= nextLifeStageChangeTick)
			{
				RecalculateLifeStageIndex();
			}
			if (ageBiologicalTicksInt % 3600000 == 0L)
			{
				BirthdayBiological();
			}
		}

		public void AgeTickMothballed(int interval)
		{
			long num = ageBiologicalTicksInt;
			ageBiologicalTicksInt += interval;
			while (Find.TickManager.TicksGame >= nextLifeStageChangeTick)
			{
				RecalculateLifeStageIndex();
			}
			for (int i = (int)(num / 3600000); i < ageBiologicalTicksInt / 3600000; i += 3600000)
			{
				BirthdayBiological();
			}
		}

		private void RecalculateLifeStageIndex()
		{
			int num = -1;
			List<LifeStageAge> lifeStageAges = pawn.RaceProps.lifeStageAges;
			for (int num2 = lifeStageAges.Count - 1; num2 >= 0; num2--)
			{
				if (lifeStageAges[num2].minAge <= AgeBiologicalYearsFloat + 1E-06f)
				{
					num = num2;
					break;
				}
			}
			if (num == -1)
			{
				num = 0;
			}
			bool num3 = cachedLifeStageIndex != num;
			cachedLifeStageIndex = num;
			if (num3 && !pawn.RaceProps.Humanlike)
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					pawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
				});
				CheckChangePawnKindName();
			}
			if (cachedLifeStageIndex < lifeStageAges.Count - 1)
			{
				float num4 = lifeStageAges[cachedLifeStageIndex + 1].minAge - AgeBiologicalYearsFloat;
				int num5 = ((Current.ProgramState == ProgramState.Playing) ? Find.TickManager.TicksGame : 0);
				nextLifeStageChangeTick = num5 + (long)Mathf.Ceil(num4 * 3600000f);
			}
			else
			{
				nextLifeStageChangeTick = long.MaxValue;
			}
		}

		private void BirthdayBiological()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (HediffGiver_Birthday item in AgeInjuryUtility.RandomHediffsToGainOnBirthday(pawn, AgeBiologicalYears))
			{
				if (item.TryApply(pawn))
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.Append("    - " + item.hediff.LabelCap);
				}
			}
			if (pawn.RaceProps.Humanlike && PawnUtility.ShouldSendNotificationAbout(pawn) && stringBuilder.Length > 0)
			{
				string str = "BirthdayBiologicalAgeInjuries".Translate(pawn, AgeBiologicalYears, stringBuilder).AdjustedFor(pawn);
				Find.LetterStack.ReceiveLetter("LetterLabelBirthday".Translate(), str, LetterDefOf.NegativeEvent, (TargetInfo)pawn);
			}
		}

		public void DebugForceBirthdayBiological()
		{
			BirthdayBiological();
		}

		private void CheckChangePawnKindName()
		{
			NameSingle nameSingle = pawn.Name as NameSingle;
			if (nameSingle == null || !nameSingle.Numerical)
			{
				return;
			}
			string kindLabel = pawn.KindLabel;
			if (!(nameSingle.NameWithoutNumber == kindLabel))
			{
				int number = nameSingle.Number;
				string text = pawn.KindLabel + " " + number;
				if (!NameUseChecker.NameSingleIsUsed(text))
				{
					pawn.Name = new NameSingle(text, numerical: true);
				}
				else
				{
					pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn, NameStyle.Numeric);
				}
			}
		}

		public void DebugMake1YearOlder()
		{
			ageBiologicalTicksInt += 3600000L;
			birthAbsTicksInt -= 3600000L;
			RecalculateLifeStageIndex();
		}
	}
}
