using System.Collections.Generic;
using System.Xml;
using Verse;

namespace RimWorld;

public class RoomPart_GestationTankDef : RoomPartDef
{
	public enum State
	{
		Dormant,
		Proximity,
		Empty
	}

	public class TankOption
	{
		public State state;

		public float weight = 1f;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			XmlHelper.ParseElements(this, xmlRoot, "state", "weight");
		}
	}

	public List<TankOption> options = new List<TankOption>();

	public RoomPart_GestationTankDef()
	{
		workerClass = typeof(RoomPart_GestationTanks);
	}
}
