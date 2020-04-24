using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompTemperatureRuinable : ThingComp
	{
		protected float ruinedPercent;

		public const string RuinedSignal = "RuinedByTemperature";

		public CompProperties_TemperatureRuinable Props => (CompProperties_TemperatureRuinable)props;

		public bool Ruined => ruinedPercent >= 1f;

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref ruinedPercent, "ruinedPercent", 0f);
		}

		public void Reset()
		{
			ruinedPercent = 0f;
		}

		public override void CompTick()
		{
			DoTicks(1);
		}

		public override void CompTickRare()
		{
			DoTicks(250);
		}

		private void DoTicks(int ticks)
		{
			if (!Ruined)
			{
				float ambientTemperature = parent.AmbientTemperature;
				if (ambientTemperature > Props.maxSafeTemperature)
				{
					ruinedPercent += (ambientTemperature - Props.maxSafeTemperature) * Props.progressPerDegreePerTick * (float)ticks;
				}
				else if (ambientTemperature < Props.minSafeTemperature)
				{
					ruinedPercent -= (ambientTemperature - Props.minSafeTemperature) * Props.progressPerDegreePerTick * (float)ticks;
				}
				if (ruinedPercent >= 1f)
				{
					ruinedPercent = 1f;
					parent.BroadcastCompSignal("RuinedByTemperature");
				}
				else if (ruinedPercent < 0f)
				{
					ruinedPercent = 0f;
				}
			}
		}

		public override void PreAbsorbStack(Thing otherStack, int count)
		{
			float t = (float)count / (float)(parent.stackCount + count);
			CompTemperatureRuinable comp = ((ThingWithComps)otherStack).GetComp<CompTemperatureRuinable>();
			ruinedPercent = Mathf.Lerp(ruinedPercent, comp.ruinedPercent, t);
		}

		public override bool AllowStackWith(Thing other)
		{
			CompTemperatureRuinable comp = ((ThingWithComps)other).GetComp<CompTemperatureRuinable>();
			return Ruined == comp.Ruined;
		}

		public override void PostSplitOff(Thing piece)
		{
			((ThingWithComps)piece).GetComp<CompTemperatureRuinable>().ruinedPercent = ruinedPercent;
		}

		public override string CompInspectStringExtra()
		{
			if (Ruined)
			{
				return "RuinedByTemperature".Translate();
			}
			if (ruinedPercent > 0f)
			{
				float ambientTemperature = parent.AmbientTemperature;
				string str;
				if (ambientTemperature > Props.maxSafeTemperature)
				{
					str = "Overheating".Translate();
				}
				else
				{
					if (!(ambientTemperature < Props.minSafeTemperature))
					{
						return null;
					}
					str = "Freezing".Translate();
				}
				return str + ": " + ruinedPercent.ToStringPercent();
			}
			return null;
		}
	}
}
