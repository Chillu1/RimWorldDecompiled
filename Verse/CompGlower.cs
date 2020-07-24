using RimWorld;

namespace Verse
{
	public class CompGlower : ThingComp
	{
		private bool glowOnInt;

		public CompProperties_Glower Props => (CompProperties_Glower)props;

		private bool ShouldBeLitNow
		{
			get
			{
				if (!parent.Spawned)
				{
					return false;
				}
				if (!FlickUtility.WantsToBeOn(parent))
				{
					return false;
				}
				CompPowerTrader compPowerTrader = parent.TryGetComp<CompPowerTrader>();
				if (compPowerTrader != null && !compPowerTrader.PowerOn)
				{
					return false;
				}
				CompRefuelable compRefuelable = parent.TryGetComp<CompRefuelable>();
				if (compRefuelable != null && !compRefuelable.HasFuel)
				{
					return false;
				}
				CompSendSignalOnCountdown compSendSignalOnCountdown = parent.TryGetComp<CompSendSignalOnCountdown>();
				if (compSendSignalOnCountdown != null && compSendSignalOnCountdown.ticksLeft <= 0)
				{
					return false;
				}
				CompSendSignalOnPawnProximity compSendSignalOnPawnProximity = parent.TryGetComp<CompSendSignalOnPawnProximity>();
				if (compSendSignalOnPawnProximity != null && compSendSignalOnPawnProximity.Sent)
				{
					return false;
				}
				return true;
			}
		}

		public bool Glows => glowOnInt;

		public void UpdateLit(Map map)
		{
			bool shouldBeLitNow = ShouldBeLitNow;
			if (glowOnInt != shouldBeLitNow)
			{
				glowOnInt = shouldBeLitNow;
				if (!glowOnInt)
				{
					map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
					map.glowGrid.DeRegisterGlower(this);
				}
				else
				{
					map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
					map.glowGrid.RegisterGlower(this);
				}
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (ShouldBeLitNow)
			{
				UpdateLit(parent.Map);
				parent.Map.glowGrid.RegisterGlower(this);
			}
			else
			{
				UpdateLit(parent.Map);
			}
		}

		public override void ReceiveCompSignal(string signal)
		{
			switch (signal)
			{
			case "PowerTurnedOn":
			case "PowerTurnedOff":
			case "FlickedOn":
			case "FlickedOff":
			case "Refueled":
			case "RanOutOfFuel":
			case "ScheduledOn":
			case "ScheduledOff":
			case "MechClusterDefeated":
				UpdateLit(parent.Map);
				break;
			}
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref glowOnInt, "glowOn", defaultValue: false);
		}

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			UpdateLit(map);
		}
	}
}
