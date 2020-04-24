using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class JoyToleranceSet : IExposable
	{
		private DefMap<JoyKindDef, float> tolerances = new DefMap<JoyKindDef, float>();

		private DefMap<JoyKindDef, bool> bored = new DefMap<JoyKindDef, bool>();

		public float this[JoyKindDef d] => tolerances[d];

		public void ExposeData()
		{
			Scribe_Deep.Look(ref tolerances, "tolerances");
			Scribe_Deep.Look(ref bored, "bored");
			if (bored == null)
			{
				bored = new DefMap<JoyKindDef, bool>();
			}
		}

		public bool BoredOf(JoyKindDef def)
		{
			return bored[def];
		}

		public void Notify_JoyGained(float amount, JoyKindDef joyKind)
		{
			float num = Mathf.Min(tolerances[joyKind] + amount * 0.65f, 1f);
			tolerances[joyKind] = num;
			if (num > 0.5f)
			{
				bored[joyKind] = true;
			}
		}

		public float JoyFactorFromTolerance(JoyKindDef joyKind)
		{
			return 1f - tolerances[joyKind];
		}

		public void NeedInterval(Pawn pawn)
		{
			float num = ExpectationsUtility.CurrentExpectationFor(pawn).joyToleranceDropPerDay * 150f / 60000f;
			for (int i = 0; i < tolerances.Count; i++)
			{
				float num2 = tolerances[i];
				num2 -= num;
				if (num2 < 0f)
				{
					num2 = 0f;
				}
				tolerances[i] = num2;
				if (bored[i] && num2 < 0.3f)
				{
					bored[i] = false;
				}
			}
		}

		public string TolerancesString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			List<JoyKindDef> allDefsListForReading = DefDatabase<JoyKindDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				JoyKindDef joyKindDef = allDefsListForReading[i];
				float num = tolerances[joyKindDef];
				if (num > 0.01f)
				{
					if (stringBuilder.Length == 0)
					{
						stringBuilder.AppendLine("JoyTolerances".Translate() + ":");
					}
					string text = "   " + joyKindDef.LabelCap + ": " + num.ToStringPercent();
					if (bored[joyKindDef])
					{
						text += " (" + "bored".Translate() + ")";
					}
					stringBuilder.AppendLine(text);
				}
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}

		public bool BoredOfAllAvailableJoyKinds(Pawn pawn)
		{
			List<JoyKindDef> list = JoyUtility.JoyKindsOnMapTempList(pawn.MapHeld);
			bool result = true;
			for (int i = 0; i < list.Count; i++)
			{
				if (!bored[list[i]])
				{
					result = false;
					break;
				}
			}
			list.Clear();
			return result;
		}
	}
}
