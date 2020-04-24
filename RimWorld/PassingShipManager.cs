using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public sealed class PassingShipManager : IExposable
	{
		public Map map;

		public List<PassingShip> passingShips = new List<PassingShip>();

		private static List<PassingShip> tmpPassingShips = new List<PassingShip>();

		public PassingShipManager(Map map)
		{
			this.map = map;
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref passingShips, "passingShips", LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				for (int i = 0; i < passingShips.Count; i++)
				{
					passingShips[i].passingShipManager = this;
				}
			}
		}

		public void AddShip(PassingShip vis)
		{
			passingShips.Add(vis);
			vis.passingShipManager = this;
		}

		public void RemoveShip(PassingShip vis)
		{
			passingShips.Remove(vis);
			vis.passingShipManager = null;
		}

		public void PassingShipManagerTick()
		{
			for (int num = passingShips.Count - 1; num >= 0; num--)
			{
				passingShips[num].PassingShipTick();
			}
		}

		public void RemoveAllShipsOfFaction(Faction faction)
		{
			for (int num = passingShips.Count - 1; num >= 0; num--)
			{
				if (passingShips[num].Faction == faction)
				{
					passingShips[num].Depart();
				}
			}
		}

		internal void DebugSendAllShipsAway()
		{
			tmpPassingShips.Clear();
			tmpPassingShips.AddRange(passingShips);
			for (int i = 0; i < tmpPassingShips.Count; i++)
			{
				tmpPassingShips[i].Depart();
			}
			Messages.Message("All passing ships sent away.", MessageTypeDefOf.TaskCompletion, historical: false);
		}
	}
}
