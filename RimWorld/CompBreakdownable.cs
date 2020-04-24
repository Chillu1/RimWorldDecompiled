using Verse;

namespace RimWorld
{
	public class CompBreakdownable : ThingComp
	{
		private bool brokenDownInt;

		private CompPowerTrader powerComp;

		private const int BreakdownMTBTicks = 13680000;

		public const string BreakdownSignal = "Breakdown";

		public bool BrokenDown => brokenDownInt;

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref brokenDownInt, "brokenDown", defaultValue: false);
		}

		public override void PostDraw()
		{
			if (brokenDownInt)
			{
				parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.BrokenDown);
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			powerComp = parent.GetComp<CompPowerTrader>();
			parent.Map.GetComponent<BreakdownManager>().Register(this);
		}

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			map.GetComponent<BreakdownManager>().Deregister(this);
		}

		public void CheckForBreakdown()
		{
			if (CanBreakdownNow() && Rand.MTBEventOccurs(1.368E+07f, 1f, 1041f))
			{
				DoBreakdown();
			}
		}

		protected bool CanBreakdownNow()
		{
			if (!BrokenDown)
			{
				if (powerComp != null)
				{
					return powerComp.PowerOn;
				}
				return true;
			}
			return false;
		}

		public void Notify_Repaired()
		{
			brokenDownInt = false;
			parent.Map.GetComponent<BreakdownManager>().Notify_Repaired(parent);
			if (parent is Building_PowerSwitch)
			{
				parent.Map.powerNetManager.Notfiy_TransmitterTransmitsPowerNowChanged(parent.GetComp<CompPower>());
			}
		}

		public void DoBreakdown()
		{
			brokenDownInt = true;
			parent.BroadcastCompSignal("Breakdown");
			parent.Map.GetComponent<BreakdownManager>().Notify_BrokenDown(parent);
		}

		public override string CompInspectStringExtra()
		{
			if (BrokenDown)
			{
				return "BrokenDown".Translate();
			}
			return null;
		}
	}
}
