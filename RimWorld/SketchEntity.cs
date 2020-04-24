using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class SketchEntity : IExposable
	{
		public IntVec3 pos;

		public abstract string Label
		{
			get;
		}

		public abstract CellRect OccupiedRect
		{
			get;
		}

		public abstract float SpawnOrder
		{
			get;
		}

		public virtual bool LostImportantReferences => false;

		public abstract void DrawGhost(IntVec3 at, Color color);

		public abstract bool IsSameSpawned(IntVec3 at, Map map);

		public abstract bool IsSameSpawnedOrBlueprintOrFrame(IntVec3 at, Map map);

		public abstract bool IsSpawningBlocked(IntVec3 at, Map map, Thing thingToIgnore = null, bool wipeIfCollides = false);

		public abstract bool IsSpawningBlockedPermanently(IntVec3 at, Map map, Thing thingToIgnore = null, bool wipeIfCollides = false);

		public abstract bool Spawn(IntVec3 at, Map map, Faction faction, Sketch.SpawnMode spawnMode = Sketch.SpawnMode.Normal, bool wipeIfCollides = false, List<Thing> spawnedThings = null, bool dormant = false);

		public abstract bool SameForSubtracting(SketchEntity other);

		public bool SpawnNear(IntVec3 near, Map map, float radius, Faction faction, Sketch.SpawnMode spawnMode = Sketch.SpawnMode.Normal, bool wipeIfCollides = false, List<Thing> spawnedThings = null, bool dormant = false)
		{
			int num = GenRadial.NumCellsInRadius(radius);
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = near + GenRadial.RadialPattern[i];
				if (intVec.InBounds(map) && Spawn(intVec, map, faction, spawnMode, wipeIfCollides, spawnedThings, dormant))
				{
					return true;
				}
			}
			return false;
		}

		public virtual SketchEntity DeepCopy()
		{
			SketchEntity obj = (SketchEntity)Activator.CreateInstance(GetType());
			obj.pos = pos;
			return obj;
		}

		public virtual void ExposeData()
		{
			Scribe_Values.Look(ref pos, "pos");
		}
	}
}
