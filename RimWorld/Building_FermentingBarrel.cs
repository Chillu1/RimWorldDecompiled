using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Building_FermentingBarrel : Building
	{
		private int wortCount;

		private float progressInt;

		private Material barFilledCachedMat;

		public const int MaxCapacity = 25;

		private const int BaseFermentationDuration = 360000;

		public const float MinIdealTemperature = 7f;

		private static readonly Vector2 BarSize = new Vector2(0.55f, 0.1f);

		private static readonly Color BarZeroProgressColor = new Color(0.4f, 0.27f, 0.22f);

		private static readonly Color BarFermentedColor = new Color(0.9f, 0.85f, 0.2f);

		private static readonly Material BarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

		public float Progress
		{
			get
			{
				return progressInt;
			}
			set
			{
				if (value != progressInt)
				{
					progressInt = value;
					barFilledCachedMat = null;
				}
			}
		}

		private Material BarFilledMat
		{
			get
			{
				if (barFilledCachedMat == null)
				{
					barFilledCachedMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.Lerp(BarZeroProgressColor, BarFermentedColor, Progress));
				}
				return barFilledCachedMat;
			}
		}

		public int SpaceLeftForWort
		{
			get
			{
				if (Fermented)
				{
					return 0;
				}
				return 25 - wortCount;
			}
		}

		private bool Empty => wortCount <= 0;

		public bool Fermented
		{
			get
			{
				if (!Empty)
				{
					return Progress >= 1f;
				}
				return false;
			}
		}

		private float CurrentTempProgressSpeedFactor
		{
			get
			{
				CompProperties_TemperatureRuinable compProperties = def.GetCompProperties<CompProperties_TemperatureRuinable>();
				float ambientTemperature = base.AmbientTemperature;
				if (ambientTemperature < compProperties.minSafeTemperature)
				{
					return 0.1f;
				}
				if (ambientTemperature < 7f)
				{
					return GenMath.LerpDouble(compProperties.minSafeTemperature, 7f, 0.1f, 1f, ambientTemperature);
				}
				return 1f;
			}
		}

		private float ProgressPerTickAtCurrentTemp => 2.77777781E-06f * CurrentTempProgressSpeedFactor;

		private int EstimatedTicksLeft => Mathf.Max(Mathf.RoundToInt((1f - Progress) / ProgressPerTickAtCurrentTemp), 0);

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref wortCount, "wortCount", 0);
			Scribe_Values.Look(ref progressInt, "progress", 0f);
		}

		public override void TickRare()
		{
			base.TickRare();
			if (!Empty)
			{
				Progress = Mathf.Min(Progress + 250f * ProgressPerTickAtCurrentTemp, 1f);
			}
		}

		public void AddWort(int count)
		{
			GetComp<CompTemperatureRuinable>().Reset();
			if (Fermented)
			{
				Log.Warning("Tried to add wort to a barrel full of beer. Colonists should take the beer first.");
				return;
			}
			int num = Mathf.Min(count, 25 - wortCount);
			if (num > 0)
			{
				Progress = GenMath.WeightedAverage(0f, num, Progress, wortCount);
				wortCount += num;
			}
		}

		protected override void ReceiveCompSignal(string signal)
		{
			if (signal == "RuinedByTemperature")
			{
				Reset();
			}
		}

		private void Reset()
		{
			wortCount = 0;
			Progress = 0f;
		}

		public void AddWort(Thing wort)
		{
			int num = Mathf.Min(wort.stackCount, 25 - wortCount);
			if (num > 0)
			{
				AddWort(num);
				wort.SplitOff(num).Destroy();
			}
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			if (stringBuilder.Length != 0)
			{
				stringBuilder.AppendLine();
			}
			CompTemperatureRuinable comp = GetComp<CompTemperatureRuinable>();
			if (!Empty && !comp.Ruined)
			{
				if (Fermented)
				{
					stringBuilder.AppendLine("ContainsBeer".Translate(wortCount, 25));
				}
				else
				{
					stringBuilder.AppendLine("ContainsWort".Translate(wortCount, 25));
				}
			}
			if (!Empty)
			{
				if (Fermented)
				{
					stringBuilder.AppendLine("Fermented".Translate());
				}
				else
				{
					stringBuilder.AppendLine("FermentationProgress".Translate(Progress.ToStringPercent(), EstimatedTicksLeft.ToStringTicksToPeriod()));
					if (CurrentTempProgressSpeedFactor != 1f)
					{
						stringBuilder.AppendLine("FermentationBarrelOutOfIdealTemperature".Translate(CurrentTempProgressSpeedFactor.ToStringPercent()));
					}
				}
			}
			stringBuilder.AppendLine("Temperature".Translate() + ": " + base.AmbientTemperature.ToStringTemperature("F0"));
			stringBuilder.AppendLine("IdealFermentingTemperature".Translate() + ": " + 7f.ToStringTemperature("F0") + " ~ " + comp.Props.maxSafeTemperature.ToStringTemperature("F0"));
			return stringBuilder.ToString().TrimEndNewlines();
		}

		public Thing TakeOutBeer()
		{
			if (!Fermented)
			{
				Log.Warning("Tried to get beer but it's not yet fermented.");
				return null;
			}
			Thing thing = ThingMaker.MakeThing(ThingDefOf.Beer);
			thing.stackCount = wortCount;
			Reset();
			return thing;
		}

		public override void Draw()
		{
			base.Draw();
			if (!Empty)
			{
				Vector3 drawPos = DrawPos;
				drawPos.y += 0.0454545468f;
				drawPos.z += 0.25f;
				GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
				r.center = drawPos;
				r.size = BarSize;
				r.fillPercent = (float)wortCount / 25f;
				r.filledMat = BarFilledMat;
				r.unfilledMat = BarUnfilledMat;
				r.margin = 0.1f;
				r.rotation = Rot4.North;
				GenDraw.DrawFillableBar(r);
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (Prefs.DevMode && !Empty)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "Debug: Set progress to 1";
				command_Action.action = delegate
				{
					Progress = 1f;
				};
				yield return command_Action;
			}
		}
	}
}
