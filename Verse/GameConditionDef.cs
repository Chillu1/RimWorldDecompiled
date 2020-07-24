using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class GameConditionDef : Def
	{
		public Type conditionClass = typeof(GameCondition);

		private List<GameConditionDef> exclusiveConditions;

		[MustTranslate]
		public string endMessage;

		[MustTranslate]
		public string letterText;

		public List<ThingDef> letterHyperlinks;

		public LetterDef letterDef;

		public bool canBePermanent;

		[MustTranslate]
		public string descriptionFuture;

		[NoTranslate]
		public string jumpToSourceKey = "ClickToJumpToSource";

		public PsychicDroneLevel defaultDroneLevel = PsychicDroneLevel.BadMedium;

		public bool preventRain;

		public WeatherDef weatherDef;

		public float temperatureOffset = -10f;

		public bool CanCoexistWith(GameConditionDef other)
		{
			if (this == other)
			{
				return false;
			}
			if (exclusiveConditions != null)
			{
				return !exclusiveConditions.Contains(other);
			}
			return true;
		}

		public static GameConditionDef Named(string defName)
		{
			return DefDatabase<GameConditionDef>.GetNamed(defName);
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (conditionClass == null)
			{
				yield return "conditionClass is null";
			}
		}
	}
}
