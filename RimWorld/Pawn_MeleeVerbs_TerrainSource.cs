using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Pawn_MeleeVerbs_TerrainSource : IExposable, IVerbOwner
	{
		public Pawn_MeleeVerbs parent;

		public TerrainDef def;

		public VerbTracker tracker;

		public VerbTracker VerbTracker => tracker;

		public List<VerbProperties> VerbProperties => null;

		public List<Tool> Tools => def.tools;

		Thing IVerbOwner.ConstantCaster => parent.Pawn;

		ImplementOwnerTypeDef IVerbOwner.ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.Terrain;

		public static Pawn_MeleeVerbs_TerrainSource Create(Pawn_MeleeVerbs parent, TerrainDef terrainDef)
		{
			Pawn_MeleeVerbs_TerrainSource obj = new Pawn_MeleeVerbs_TerrainSource
			{
				parent = parent,
				def = terrainDef
			};
			obj.tracker = new VerbTracker(obj);
			return obj;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Deep.Look(ref tracker, "tracker", this);
		}

		string IVerbOwner.UniqueVerbOwnerID()
		{
			return "TerrainVerbs_" + parent.Pawn.ThingID;
		}

		bool IVerbOwner.VerbsStillUsableBy(Pawn p)
		{
			if (p != parent.Pawn)
			{
				return false;
			}
			if (!p.Spawned || def != p.Position.GetTerrain(p.Map))
			{
				return false;
			}
			if (Find.TickManager.TicksGame < parent.lastTerrainBasedVerbUseTick + 1200)
			{
				return false;
			}
			return true;
		}
	}
}
