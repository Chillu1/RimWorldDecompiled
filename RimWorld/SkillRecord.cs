using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class SkillRecord : IExposable
	{
		private Pawn pawn;

		public SkillDef def;

		public int levelInt;

		public Passion passion;

		public float xpSinceLastLevel;

		public float xpSinceMidnight;

		private BoolUnknown cachedTotallyDisabled = BoolUnknown.Unknown;

		public const int IntervalTicks = 200;

		public const int MinLevel = 0;

		public const int MaxLevel = 20;

		public const int MaxFullRateXpPerDay = 4000;

		public const int MasterSkillThreshold = 14;

		public const float SaturatedLearningFactor = 0.2f;

		public const float LearnFactorPassionNone = 0.35f;

		public const float LearnFactorPassionMinor = 1f;

		public const float LearnFactorPassionMajor = 1.5f;

		public const float MinXPAmount = -1000f;

		private static readonly SimpleCurve XpForLevelUpCurve = new SimpleCurve
		{
			new CurvePoint(0f, 1000f),
			new CurvePoint(9f, 10000f),
			new CurvePoint(19f, 30000f)
		};

		public int Level
		{
			get
			{
				if (TotallyDisabled)
				{
					return 0;
				}
				return levelInt;
			}
			set
			{
				levelInt = Mathf.Clamp(value, 0, 20);
			}
		}

		public float XpRequiredForLevelUp => XpRequiredToLevelUpFrom(levelInt);

		public float XpProgressPercent => xpSinceLastLevel / XpRequiredForLevelUp;

		public float XpTotalEarned
		{
			get
			{
				float num = 0f;
				for (int i = 0; i < levelInt; i++)
				{
					num += XpRequiredToLevelUpFrom(i);
				}
				return num;
			}
		}

		public bool TotallyDisabled
		{
			get
			{
				if (cachedTotallyDisabled == BoolUnknown.Unknown)
				{
					cachedTotallyDisabled = ((!CalculateTotallyDisabled()) ? BoolUnknown.False : BoolUnknown.True);
				}
				return cachedTotallyDisabled == BoolUnknown.True;
			}
		}

		public string LevelDescriptor => levelInt switch
		{
			0 => "Skill0".Translate(), 
			1 => "Skill1".Translate(), 
			2 => "Skill2".Translate(), 
			3 => "Skill3".Translate(), 
			4 => "Skill4".Translate(), 
			5 => "Skill5".Translate(), 
			6 => "Skill6".Translate(), 
			7 => "Skill7".Translate(), 
			8 => "Skill8".Translate(), 
			9 => "Skill9".Translate(), 
			10 => "Skill10".Translate(), 
			11 => "Skill11".Translate(), 
			12 => "Skill12".Translate(), 
			13 => "Skill13".Translate(), 
			14 => "Skill14".Translate(), 
			15 => "Skill15".Translate(), 
			16 => "Skill16".Translate(), 
			17 => "Skill17".Translate(), 
			18 => "Skill18".Translate(), 
			19 => "Skill19".Translate(), 
			20 => "Skill20".Translate(), 
			_ => "Unknown", 
		};

		public bool LearningSaturatedToday => xpSinceMidnight > 4000f;

		public SkillRecord()
		{
		}

		public SkillRecord(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public SkillRecord(Pawn pawn, SkillDef def)
		{
			this.pawn = pawn;
			this.def = def;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref levelInt, "level", 0);
			Scribe_Values.Look(ref xpSinceLastLevel, "xpSinceLastLevel", 0f);
			Scribe_Values.Look(ref passion, "passion", Passion.None);
			Scribe_Values.Look(ref xpSinceMidnight, "xpSinceMidnight", 0f);
		}

		public void Interval()
		{
			float num = (pawn.story.traits.HasTrait(TraitDefOf.GreatMemory) ? 0.5f : 1f);
			switch (levelInt)
			{
			case 10:
				Learn(-0.1f * num);
				break;
			case 11:
				Learn(-0.2f * num);
				break;
			case 12:
				Learn(-0.4f * num);
				break;
			case 13:
				Learn(-0.6f * num);
				break;
			case 14:
				Learn(-1f * num);
				break;
			case 15:
				Learn(-1.8f * num);
				break;
			case 16:
				Learn(-2.8f * num);
				break;
			case 17:
				Learn(-4f * num);
				break;
			case 18:
				Learn(-6f * num);
				break;
			case 19:
				Learn(-8f * num);
				break;
			case 20:
				Learn(-12f * num);
				break;
			}
		}

		public static float XpRequiredToLevelUpFrom(int startingLevel)
		{
			return XpForLevelUpCurve.Evaluate(startingLevel);
		}

		public void Learn(float xp, bool direct = false)
		{
			if (TotallyDisabled || (xp < 0f && levelInt == 0))
			{
				return;
			}
			bool flag = false;
			if (xp > 0f)
			{
				xp *= LearnRateFactor(direct);
			}
			xpSinceLastLevel += xp;
			if (!direct)
			{
				xpSinceMidnight += xp;
			}
			if (levelInt == 20 && xpSinceLastLevel > XpRequiredForLevelUp - 1f)
			{
				xpSinceLastLevel = XpRequiredForLevelUp - 1f;
			}
			while (xpSinceLastLevel >= XpRequiredForLevelUp)
			{
				xpSinceLastLevel -= XpRequiredForLevelUp;
				levelInt++;
				flag = true;
				if (levelInt == 14)
				{
					if (passion == Passion.None)
					{
						TaleRecorder.RecordTale(TaleDefOf.GainedMasterSkillWithoutPassion, pawn, def);
					}
					else
					{
						TaleRecorder.RecordTale(TaleDefOf.GainedMasterSkillWithPassion, pawn, def);
					}
				}
				if (levelInt >= 20)
				{
					levelInt = 20;
					xpSinceLastLevel = Mathf.Clamp(xpSinceLastLevel, 0f, XpRequiredForLevelUp - 1f);
					break;
				}
			}
			while (xpSinceLastLevel <= -1000f)
			{
				levelInt--;
				xpSinceLastLevel += XpRequiredForLevelUp;
				if (levelInt <= 0)
				{
					levelInt = 0;
					xpSinceLastLevel = 0f;
					break;
				}
			}
			if (flag && pawn.IsColonist && pawn.Spawned)
			{
				MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, def.LabelCap + "\n" + "TextMote_SkillUp".Translate(levelInt));
			}
		}

		public float LearnRateFactor(bool direct = false)
		{
			if (DebugSettings.fastLearning)
			{
				return 200f;
			}
			float num = passion switch
			{
				Passion.None => 0.35f, 
				Passion.Minor => 1f, 
				Passion.Major => 1.5f, 
				_ => throw new NotImplementedException("Passion level " + passion), 
			};
			if (!direct)
			{
				num *= pawn.GetStatValue(StatDefOf.GlobalLearningFactor);
				if (LearningSaturatedToday)
				{
					num *= 0.2f;
				}
			}
			return num;
		}

		public void EnsureMinLevelWithMargin(int minLevel)
		{
			if (!TotallyDisabled && (Level < minLevel || (Level == minLevel && xpSinceLastLevel < XpRequiredForLevelUp / 2f)))
			{
				Level = minLevel;
				xpSinceLastLevel = XpRequiredForLevelUp / 2f;
			}
		}

		public void Notify_SkillDisablesChanged()
		{
			cachedTotallyDisabled = BoolUnknown.Unknown;
		}

		private bool CalculateTotallyDisabled()
		{
			return def.IsDisabled(pawn.story.DisabledWorkTagsBackstoryAndTraits, pawn.GetDisabledWorkTypes(permanentOnly: true));
		}

		public override string ToString()
		{
			return def.defName + ": " + levelInt + " (" + xpSinceLastLevel + "xp)";
		}
	}
}
