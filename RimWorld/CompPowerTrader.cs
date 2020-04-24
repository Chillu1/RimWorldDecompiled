using System;
using System.Text;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompPowerTrader : CompPower
	{
		public Action powerStartedAction;

		public Action powerStoppedAction;

		private bool powerOnInt;

		public float powerOutputInt;

		private bool powerLastOutputted;

		private Sustainer sustainerPowered;

		protected CompFlickable flickableComp;

		public const string PowerTurnedOnSignal = "PowerTurnedOn";

		public const string PowerTurnedOffSignal = "PowerTurnedOff";

		public float PowerOutput
		{
			get
			{
				return powerOutputInt;
			}
			set
			{
				powerOutputInt = value;
				if (powerOutputInt > 0f)
				{
					powerLastOutputted = true;
				}
				if (powerOutputInt < 0f)
				{
					powerLastOutputted = false;
				}
			}
		}

		public float EnergyOutputPerTick => PowerOutput * CompPower.WattsToWattDaysPerTick;

		public bool PowerOn
		{
			get
			{
				return powerOnInt;
			}
			set
			{
				if (powerOnInt == value)
				{
					return;
				}
				powerOnInt = value;
				if (powerOnInt)
				{
					if (!FlickUtility.WantsToBeOn(parent))
					{
						Log.Warning("Tried to power on " + parent + " which did not desire it.");
						return;
					}
					if (parent.IsBrokenDown())
					{
						Log.Warning("Tried to power on " + parent + " which is broken down.");
						return;
					}
					if (powerStartedAction != null)
					{
						powerStartedAction();
					}
					parent.BroadcastCompSignal("PowerTurnedOn");
					SoundDef soundDef = ((CompProperties_Power)parent.def.CompDefForAssignableFrom<CompPowerTrader>()).soundPowerOn;
					if (soundDef.NullOrUndefined())
					{
						soundDef = SoundDefOf.Power_OnSmall;
					}
					soundDef.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
					StartSustainerPoweredIfInactive();
				}
				else
				{
					if (powerStoppedAction != null)
					{
						powerStoppedAction();
					}
					parent.BroadcastCompSignal("PowerTurnedOff");
					SoundDef soundDef2 = ((CompProperties_Power)parent.def.CompDefForAssignableFrom<CompPowerTrader>()).soundPowerOff;
					if (soundDef2.NullOrUndefined())
					{
						soundDef2 = SoundDefOf.Power_OffSmall;
					}
					if (parent.Spawned)
					{
						soundDef2.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
					}
					EndSustainerPoweredIfActive();
				}
			}
		}

		public string DebugString
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(parent.LabelCap + " CompPower:");
				stringBuilder.AppendLine("   PowerOn: " + PowerOn.ToString());
				stringBuilder.AppendLine("   energyProduction: " + PowerOutput);
				return stringBuilder.ToString();
			}
		}

		public override void ReceiveCompSignal(string signal)
		{
			if (signal == "FlickedOff" || signal == "ScheduledOff" || signal == "Breakdown")
			{
				PowerOn = false;
			}
			if (signal == "RanOutOfFuel" && powerLastOutputted)
			{
				PowerOn = false;
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			flickableComp = parent.GetComp<CompFlickable>();
		}

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			EndSustainerPoweredIfActive();
			powerOutputInt = 0f;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref powerOnInt, "powerOn", defaultValue: true);
		}

		public override void PostDraw()
		{
			base.PostDraw();
			if (!parent.IsBrokenDown())
			{
				if (flickableComp != null && !flickableComp.SwitchIsOn)
				{
					parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.PowerOff);
				}
				else if (FlickUtility.WantsToBeOn(parent) && !PowerOn)
				{
					parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.NeedsPower);
				}
			}
		}

		public override void SetUpPowerVars()
		{
			base.SetUpPowerVars();
			CompProperties_Power props = base.Props;
			PowerOutput = -1f * props.basePowerConsumption;
			powerLastOutputted = (props.basePowerConsumption <= 0f);
		}

		public override void ResetPowerVars()
		{
			base.ResetPowerVars();
			powerOnInt = false;
			powerOutputInt = 0f;
			powerLastOutputted = false;
			sustainerPowered = null;
			if (flickableComp != null)
			{
				flickableComp.ResetToOn();
			}
		}

		public override void LostConnectParent()
		{
			base.LostConnectParent();
			PowerOn = false;
		}

		public override string CompInspectStringExtra()
		{
			string str = (!powerLastOutputted) ? ((string)("PowerNeeded".Translate() + ": " + (0f - PowerOutput).ToString("#####0") + " W")) : ((string)("PowerOutput".Translate() + ": " + PowerOutput.ToString("#####0") + " W"));
			return str + "\n" + base.CompInspectStringExtra();
		}

		private void StartSustainerPoweredIfInactive()
		{
			CompProperties_Power props = base.Props;
			if (!props.soundAmbientPowered.NullOrUndefined() && sustainerPowered == null)
			{
				SoundInfo info = SoundInfo.InMap(parent);
				sustainerPowered = props.soundAmbientPowered.TrySpawnSustainer(info);
			}
		}

		private void EndSustainerPoweredIfActive()
		{
			if (sustainerPowered != null)
			{
				sustainerPowered.End();
				sustainerPowered = null;
			}
		}
	}
}
