namespace RimWorld
{
	public class ShipJob_WaitForever : ShipJob_Wait
	{
		protected override bool ShouldEnd
		{
			get
			{
				if (transportShip.shipThing != null)
				{
					return transportShip.shipThing.Destroyed;
				}
				return true;
			}
		}
	}
}
