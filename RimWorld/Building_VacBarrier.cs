using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Building_VacBarrier : Building_SupportedDoor
	{
		private CompPowerTrader intPowerTrader;

		private VacuumComponent intVacuum;

		private CompPowerTrader PowerTrader => intPowerTrader ?? (intPowerTrader = GetComp<CompPowerTrader>());

		private VacuumComponent Vacuum => intVacuum ?? (intVacuum = base.MapHeld?.GetComponent<VacuumComponent>());

		private bool BarrierOn => PowerTrader.PowerOn;

		public override bool FreePassage => true;

		public override bool ExchangeVacuum => !PowerTrader.PowerOn;

		protected override float TempEqualizeRate
		{
			get
			{
				if (!BarrierOn)
				{
					return 14f;
				}
				return 0f;
			}
		}

		protected override bool AlwaysOpen => true;

		protected override float OpenPct => 1f;

		protected override bool CanDrawMovers => false;

		protected override void ReceiveCompSignal(string signal)
		{
			if (signal == "PowerTurnedOn" || signal == "PowerTurnedOff")
			{
				Vacuum?.Dirty();
			}
		}

		protected override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			base.DrawAt(drawLoc, flip);
			if (BarrierOn)
			{
				Graphic.Draw(drawLoc, flip ? base.Rotation.Opposite : base.Rotation, this);
			}
		}
	}
}
