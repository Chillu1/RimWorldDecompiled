using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class CompSpawnSubplant : ThingComp
	{
		private float progressToNextSubplant;

		private List<Thing> subplants = new List<Thing>();

		private int meditationTicksToday;

		public Action onGrassGrown;

		private static readonly List<Pair<int, float>> TicksToProgressMultipliers = new List<Pair<int, float>>
		{
			new Pair<int, float>(30000, 1f),
			new Pair<int, float>(60000, 0.5f),
			new Pair<int, float>(120000, 0.25f),
			new Pair<int, float>(240000, 0.15f)
		};

		public CompProperties_SpawnSubplant Props => (CompProperties_SpawnSubplant)props;

		public List<Thing> SubplantsForReading
		{
			get
			{
				Cleanup();
				return subplants;
			}
		}

		private float ProgressMultiplier
		{
			get
			{
				foreach (Pair<int, float> ticksToProgressMultiplier in TicksToProgressMultipliers)
				{
					if (meditationTicksToday < ticksToProgressMultiplier.First)
					{
						return ticksToProgressMultiplier.Second;
					}
				}
				return TicksToProgressMultipliers.Last().Second;
			}
		}

		[Obsolete("Only used for mod compatibility.")]
		public void AddProgress(float progress)
		{
			AddProgress_NewTmp(progress);
		}

		public void AddProgress_NewTmp(float progress, bool ignoreMultiplier = false)
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Subplant spawners are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 43254);
				return;
			}
			if (!ignoreMultiplier)
			{
				progress *= ProgressMultiplier;
			}
			progressToNextSubplant += progress;
			meditationTicksToday++;
			TryGrowSubplants();
		}

		public override void CompTickLong()
		{
			if (GenLocalDate.DayTick(parent.Map) < 2000)
			{
				meditationTicksToday = 0;
			}
		}

		public void Cleanup()
		{
			subplants.RemoveAll((Thing p) => !p.Spawned);
		}

		public override string CompInspectStringExtra()
		{
			return (string)("TotalMeditationToday".Translate((meditationTicksToday / 2500).ToString() + "LetterHour".Translate(), ProgressMultiplier.ToStringPercent()) + "\n" + Props.subplant.LabelCap + ": ") + SubplantsForReading.Count + " (" + "ProgressToNextSubplant".Translate(progressToNextSubplant.ToStringPercent()) + ")";
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
			for (int i = 0; i < 1000; i++)
			{
				IntVec3 intVec = position + GenRadial.RadialPattern[i];
				if (!intVec.InBounds(parent.Map) || !WanderUtility.InSameRoom(position, intVec, parent.Map))
				{
					continue;
				}
				bool flag = false;
				List<Thing> thingList = intVec.GetThingList(parent.Map);
				foreach (Thing item in thingList)
				{
					if (item.def == Props.subplant)
					{
						flag = true;
						break;
					}
				}
				if (flag || !Props.subplant.CanEverPlantAt_NewTemp(intVec, parent.Map, canWipePlantsExceptTree: true))
				{
					continue;
				}
				for (int num = thingList.Count - 1; num >= 0; num--)
				{
					if (thingList[num].def.category == ThingCategory.Plant)
					{
						thingList[num].Destroy();
					}
				}
				subplants.Add(GenSpawn.Spawn(Props.subplant, intVec, parent.Map));
				if (Props.spawnSound != null)
				{
					Props.spawnSound.PlayOneShot(new TargetInfo(parent));
				}
				onGrassGrown?.Invoke();
				break;
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (Prefs.DevMode)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "DEV: Add 100% progress";
				command_Action.action = delegate
				{
					AddProgress_NewTmp(1f, ignoreMultiplier: true);
				};
				yield return command_Action;
			}
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref progressToNextSubplant, "progressToNextSubplant", 0f);
			Scribe_Collections.Look(ref subplants, "subplants", LookMode.Reference);
			Scribe_Values.Look(ref meditationTicksToday, "meditationTicksToday", 0);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				subplants.RemoveAll((Thing x) => x == null);
			}
		}
	}
}
