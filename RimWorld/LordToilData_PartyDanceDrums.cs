using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class LordToilData_PartyDanceDrums : LordToilData_Gathering
	{
		public Dictionary<Pawn, Building> playedInstruments = new Dictionary<Pawn, Building>();

		private List<Pawn> tmpPawns;

		private List<Building> tmpUsedInstruments;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref playedInstruments, "playedInstruments", LookMode.Reference, LookMode.Reference, ref tmpPawns, ref tmpUsedInstruments);
		}
	}
}
