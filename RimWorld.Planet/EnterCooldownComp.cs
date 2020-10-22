using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class EnterCooldownComp : WorldObjectComp
	{
		private int ticksLeft;

		public WorldObjectCompProperties_EnterCooldown Props => (WorldObjectCompProperties_EnterCooldown)props;

		public bool Active => ticksLeft > 0;

		public bool BlocksEntering
		{
			get
			{
				if (Active)
				{
					return !base.ParentHasMap;
				}
				return false;
			}
		}

		public int TicksLeft
		{
			get
			{
				if (!Active)
				{
					return 0;
				}
				return ticksLeft;
			}
		}

		public float DaysLeft => (float)TicksLeft / 60000f;

		public void Start(float? durationDays = null)
		{
			float num = durationDays ?? Props.durationDays;
			ticksLeft = Mathf.RoundToInt(num * 60000f);
		}

		public void Stop()
		{
			ticksLeft = 0;
		}

		public override void CompTick()
		{
			base.CompTick();
			if (Active)
			{
				ticksLeft--;
			}
		}

		public override void PostMapGenerate()
		{
			base.PostMapGenerate();
			if (Active)
			{
				Stop();
			}
		}

		public override void PostMyMapRemoved()
		{
			base.PostMyMapRemoved();
			if (Props.autoStartOnMapRemoved)
			{
				Start();
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref ticksLeft, "ticksLeft", 0);
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			if (Prefs.DevMode)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "Dev: Set enter cooldown to 1 hour";
				command_Action.action = delegate
				{
					ticksLeft = 2500;
				};
				yield return command_Action;
				Command_Action command_Action2 = new Command_Action();
				command_Action2.defaultLabel = "Dev: Reset enter cooldown";
				command_Action2.action = delegate
				{
					ticksLeft = 0;
				};
				yield return command_Action2;
			}
		}
	}
}
