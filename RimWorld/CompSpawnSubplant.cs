using System;
using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompSpawnSubplant : ThingComp
	{
		private float progressToNextSubplant;

		private List<Thing> subplants = new List<Thing>();

		public Action onGrassGrown;

		public CompProperties_SpawnSubplant Props => (CompProperties_SpawnSubplant)props;

		public List<Thing> SubplantsForReading
		{
			get
			{
				Cleanup();
				return subplants;
			}
		}

		public void AddProgress(float progress)
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Subplant spawners are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 43254);
				return;
			}
			progressToNextSubplant += progress;
			TryGrowSubplants();
		}

		public void Cleanup()
		{
			subplants.RemoveAll((Thing p) => !p.Spawned);
		}

		public override string CompInspectStringExtra()
		{
			return (string)(Props.subplant.LabelCap + ": ") + SubplantsForReading.Count + "\n" + "ProgressToNextSubplant".Translate(Props.subplant.label, progressToNextSubplant.ToStringPercent());
		}

		private void TryGrowSubplants()
		{
			while (progressToNextSubplant >= 1f)
			{
				DoGrowSubplant();
				progressToNextSubplant -= 1f;
			}
		}

		private void DoGrowSubplant()
		{
			IntVec3 position = parent.Position;
			int num = 0;
			IntVec3 intVec;
			List<Thing> thingList;
			while (true)
			{
				if (num >= 1000)
				{
					return;
				}
				intVec = position + GenRadial.RadialPattern[num];
				if (intVec.InBounds(parent.Map))
				{
					bool flag = false;
					thingList = intVec.GetThingList(parent.Map);
					foreach (Thing item in thingList)
					{
						if (item.def == Props.subplant)
						{
							flag = true;
							break;
						}
					}
					if (!flag && Props.subplant.CanEverPlantAt_NewTemp(intVec, parent.Map, canWipePlantsExceptTree: true))
					{
						break;
					}
				}
				num++;
			}
			for (int num2 = thingList.Count - 1; num2 >= 0; num2--)
			{
				if (thingList[num2].def.category == ThingCategory.Plant)
				{
					thingList[num2].Destroy();
				}
			}
			subplants.Add(GenSpawn.Spawn(Props.subplant, intVec, parent.Map));
			if (Props.spawnSound != null)
			{
				Props.spawnSound.PlayOneShot(new TargetInfo(parent));
			}
			onGrassGrown?.Invoke();
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (Prefs.DevMode)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "DEV: Add 100% progress";
				command_Action.action = delegate
				{
					AddProgress(1f);
				};
				yield return command_Action;
			}
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref progressToNextSubplant, "progressToNextSubplant", 0f);
			Scribe_Collections.Look(ref subplants, "subplants", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				subplants.RemoveAll((Thing x) => x == null);
			}
		}
	}
}
