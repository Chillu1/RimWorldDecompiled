using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompCauseGameCondition_TemperatureOffset : CompCauseGameCondition
	{
		public float temperatureOffset;

		public new CompProperties_CausesGameCondition_ClimateAdjuster Props => (CompProperties_CausesGameCondition_ClimateAdjuster)props;

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			temperatureOffset = Props.temperatureOffsetRange.min;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref temperatureOffset, "temperatureOffset", 0f);
		}

		private string GetFloatStringWithSign(float val)
		{
			if (val < 0f)
			{
				return val.ToString("0");
			}
			return "+" + val.ToString("0");
		}

		public void SetTemperatureOffset(float offset)
		{
			temperatureOffset = Props.temperatureOffsetRange.ClampToRange(offset);
			ReSetupAllConditions();
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (Prefs.DevMode)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "-10";
				command_Action.action = (Action)Delegate.Combine(command_Action.action, (Action)delegate
				{
					SetTemperatureOffset(temperatureOffset - 10f);
				});
				command_Action.hotKey = KeyBindingDefOf.Misc1;
				yield return command_Action;
				Command_Action command_Action2 = new Command_Action();
				command_Action2.defaultLabel = "-1";
				command_Action2.action = (Action)Delegate.Combine(command_Action2.action, (Action)delegate
				{
					SetTemperatureOffset(temperatureOffset - 1f);
				});
				command_Action2.hotKey = KeyBindingDefOf.Misc2;
				yield return command_Action2;
				Command_Action command_Action3 = new Command_Action();
				command_Action3.defaultLabel = "+1";
				command_Action3.action = (Action)Delegate.Combine(command_Action3.action, (Action)delegate
				{
					SetTemperatureOffset(temperatureOffset + 1f);
				});
				command_Action3.hotKey = KeyBindingDefOf.Misc3;
				yield return command_Action3;
				Command_Action command_Action4 = new Command_Action();
				command_Action4.defaultLabel = "+10";
				command_Action4.action = (Action)Delegate.Combine(command_Action4.action, (Action)delegate
				{
					SetTemperatureOffset(temperatureOffset + 10f);
				});
				command_Action4.hotKey = KeyBindingDefOf.Misc4;
				yield return command_Action4;
			}
		}

		public override string CompInspectStringExtra()
		{
			string text = base.CompInspectStringExtra();
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			return text + ("Temperature".Translate() + ": " + GetFloatStringWithSign(temperatureOffset));
		}

		protected override void SetupCondition(GameCondition condition, Map map)
		{
			base.SetupCondition(condition, map);
			((GameCondition_TemperatureOffset)condition).tempOffset = temperatureOffset;
		}

		public override void RandomizeSettings()
		{
			temperatureOffset = (Rand.Bool ? Props.temperatureOffsetRange.min : Props.temperatureOffsetRange.max);
		}
	}
}
