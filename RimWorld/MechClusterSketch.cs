using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class MechClusterSketch : IExposable
	{
		public struct Mech : IExposable
		{
			public PawnKindDef kindDef;

			public IntVec3 position;

			public Mech(PawnKindDef kindDef)
			{
				this.kindDef = kindDef;
				position = IntVec3.Invalid;
			}

			public void ExposeData()
			{
				Scribe_Defs.Look(ref kindDef, "kindDef");
				Scribe_Values.Look(ref position, "position");
			}
		}

		public Sketch buildingsSketch;

		public List<Mech> pawns;

		public bool startDormant;

		public MechClusterSketch()
		{
		}

		public MechClusterSketch(Sketch buildingsSketch, List<Mech> pawns, bool startDormant)
		{
			this.buildingsSketch = buildingsSketch;
			this.pawns = pawns;
			this.startDormant = startDormant;
		}

		public void ExposeData()
		{
			Scribe_Deep.Look(ref buildingsSketch, "buildingsSketch");
			Scribe_Collections.Look(ref pawns, "pawns", LookMode.Deep);
			Scribe_Values.Look(ref startDormant, "startDormant", defaultValue: false);
		}
	}
}
