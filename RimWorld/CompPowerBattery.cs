using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompPowerBattery : CompPower
	{
		private float storedEnergy;

		private const float SelfDischargingWatts = 5f;

		public float AmountCanAccept
		{
			get
			{
				if (parent.IsBrokenDown())
				{
					return 0f;
				}
				CompProperties_Battery props = Props;
				return (props.storedEnergyMax - storedEnergy) / props.efficiency;
			}
		}

		public float StoredEnergy => storedEnergy;

		public float StoredEnergyPct => storedEnergy / Props.storedEnergyMax;

		public new CompProperties_Battery Props => (CompProperties_Battery)props;

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref storedEnergy, "storedPower", 0f);
			CompProperties_Battery props = Props;
			if (storedEnergy > props.storedEnergyMax)
			{
				storedEnergy = props.storedEnergyMax;
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			DrawPower(Mathf.Min(5f * CompPower.WattsToWattDaysPerTick, storedEnergy));
		}

		public void AddEnergy(float amount)
		{
			if (amount < 0f)
			{
				Log.Error("Cannot add negative energy " + amount);
				return;
			}
			if (amount > AmountCanAccept)
			{
				amount = AmountCanAccept;
			}
			amount *= Props.efficiency;
			storedEnergy += amount;
		}

		public void DrawPower(float amount)
		{
			storedEnergy -= amount;
			if (storedEnergy < 0f)
			{
				Log.Error("Drawing power we don't have from " + parent);
				storedEnergy = 0f;
			}
		}

		public void SetStoredEnergyPct(float pct)
		{
			pct = Mathf.Clamp01(pct);
			storedEnergy = Props.storedEnergyMax * pct;
		}

		public override void ReceiveCompSignal(string signal)
		{
			if (signal == "Breakdown")
			{
				DrawPower(StoredEnergy);
			}
		}

		public override string CompInspectStringExtra()
		{
			CompProperties_Battery props = Props;
			string t = "PowerBatteryStored".Translate() + ": " + storedEnergy.ToString("F0") + " / " + props.storedEnergyMax.ToString("F0") + " Wd";
			t += "\n" + "PowerBatteryEfficiency".Translate() + ": " + (props.efficiency * 100f).ToString("F0") + "%";
			if (storedEnergy > 0f)
			{
				t += "\n" + "SelfDischarging".Translate() + ": " + 5f.ToString("F0") + " W";
			}
			return t + "\n" + base.CompInspectStringExtra();
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
			if (Prefs.DevMode)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "DEBUG: Fill";
				command_Action.action = delegate
				{
					SetStoredEnergyPct(1f);
				};
				yield return command_Action;
				Command_Action command_Action2 = new Command_Action();
				command_Action2.defaultLabel = "DEBUG: Empty";
				command_Action2.action = delegate
				{
					SetStoredEnergyPct(0f);
				};
				yield return command_Action2;
			}
		}
	}
}
