using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Pawn_WorkSettings : IExposable
	{
		private Pawn pawn;

		private DefMap<WorkTypeDef, int> priorities;

		private bool workGiversDirty = true;

		private List<WorkGiver> workGiversInOrderEmerg = new List<WorkGiver>();

		private List<WorkGiver> workGiversInOrderNormal = new List<WorkGiver>();

		public const int LowestPriority = 4;

		public const int DefaultPriority = 3;

		private const int MaxInitialActiveWorks = 6;

		private static List<WorkTypeDef> wtsByPrio = new List<WorkTypeDef>();

		public bool EverWork => priorities != null;

		public List<WorkGiver> WorkGiversInOrderNormal
		{
			get
			{
				if (workGiversDirty)
				{
					CacheWorkGiversInOrder();
				}
				return workGiversInOrderNormal;
			}
		}

		public List<WorkGiver> WorkGiversInOrderEmergency
		{
			get
			{
				if (workGiversDirty)
				{
					CacheWorkGiversInOrder();
				}
				return workGiversInOrderEmerg;
			}
		}

		public Pawn_WorkSettings()
		{
		}

		public Pawn_WorkSettings(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Deep.Look(ref priorities, "priorities");
			if (Scribe.mode == LoadSaveMode.PostLoadInit && priorities != null)
			{
				List<WorkTypeDef> disabledWorkTypes = pawn.GetDisabledWorkTypes();
				for (int i = 0; i < disabledWorkTypes.Count; i++)
				{
					Disable(disabledWorkTypes[i]);
				}
			}
		}

		public void EnableAndInitializeIfNotAlreadyInitialized()
		{
			if (priorities == null)
			{
				EnableAndInitialize();
			}
		}

		public void EnableAndInitialize()
		{
			if (priorities == null)
			{
				priorities = new DefMap<WorkTypeDef, int>();
			}
			priorities.SetAll(0);
			int num = 0;
			foreach (WorkTypeDef item in from w in DefDatabase<WorkTypeDef>.AllDefs
				where !w.alwaysStartActive && !pawn.WorkTypeIsDisabled(w)
				orderby pawn.skills.AverageOfRelevantSkillsFor(w) descending
				select w)
			{
				SetPriority(item, 3);
				num++;
				if (num >= 6)
				{
					break;
				}
			}
			foreach (WorkTypeDef item2 in DefDatabase<WorkTypeDef>.AllDefs.Where((WorkTypeDef w) => w.alwaysStartActive))
			{
				if (!pawn.WorkTypeIsDisabled(item2))
				{
					SetPriority(item2, 3);
				}
			}
			List<WorkTypeDef> disabledWorkTypes = pawn.GetDisabledWorkTypes();
			for (int i = 0; i < disabledWorkTypes.Count; i++)
			{
				Disable(disabledWorkTypes[i]);
			}
		}

		private void ConfirmInitializedDebug()
		{
			if (priorities == null)
			{
				Log.Error(pawn + " did not have work settings initialized.");
				EnableAndInitialize();
			}
		}

		public void SetPriority(WorkTypeDef w, int priority)
		{
			ConfirmInitializedDebug();
			if (priority != 0 && pawn.WorkTypeIsDisabled(w))
			{
				Log.Error("Tried to change priority on disabled worktype " + w + " for pawn " + pawn);
				return;
			}
			if (priority < 0 || priority > 4)
			{
				Log.Message("Trying to set work to invalid priority " + priority);
			}
			priorities[w] = priority;
			if (priority == 0 && pawn.jobs != null)
			{
				pawn.jobs.Notify_WorkTypeDisabled(w);
			}
			workGiversDirty = true;
		}

		public int GetPriority(WorkTypeDef w)
		{
			ConfirmInitializedDebug();
			int num = priorities[w];
			if (num > 0 && !Find.PlaySettings.useWorkPriorities)
			{
				return 3;
			}
			return num;
		}

		public bool WorkIsActive(WorkTypeDef w)
		{
			ConfirmInitializedDebug();
			return GetPriority(w) > 0;
		}

		public void Disable(WorkTypeDef w)
		{
			ConfirmInitializedDebug();
			SetPriority(w, 0);
		}

		public void DisableAll()
		{
			ConfirmInitializedDebug();
			priorities.SetAll(0);
			workGiversDirty = true;
		}

		public void Notify_UseWorkPrioritiesChanged()
		{
			workGiversDirty = true;
		}

		public void Notify_DisabledWorkTypesChanged()
		{
			if (priorities != null)
			{
				List<WorkTypeDef> disabledWorkTypes = pawn.GetDisabledWorkTypes();
				for (int i = 0; i < disabledWorkTypes.Count; i++)
				{
					Disable(disabledWorkTypes[i]);
				}
			}
		}

		private void CacheWorkGiversInOrder()
		{
			wtsByPrio.Clear();
			List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
			int num = 999;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				WorkTypeDef workTypeDef = allDefsListForReading[i];
				int priority = GetPriority(workTypeDef);
				if (priority > 0)
				{
					if (priority < num && workTypeDef.workGiversByPriority.Any((WorkGiverDef wg) => !wg.emergency))
					{
						num = priority;
					}
					wtsByPrio.Add(workTypeDef);
				}
			}
			wtsByPrio.InsertionSort(delegate(WorkTypeDef a, WorkTypeDef b)
			{
				float value = a.naturalPriority + (4 - GetPriority(a)) * 100000;
				return ((float)(b.naturalPriority + (4 - GetPriority(b)) * 100000)).CompareTo(value);
			});
			workGiversInOrderEmerg.Clear();
			for (int j = 0; j < wtsByPrio.Count; j++)
			{
				WorkTypeDef workTypeDef2 = wtsByPrio[j];
				for (int k = 0; k < workTypeDef2.workGiversByPriority.Count; k++)
				{
					WorkGiver worker = workTypeDef2.workGiversByPriority[k].Worker;
					if (worker.def.emergency && GetPriority(worker.def.workType) <= num)
					{
						workGiversInOrderEmerg.Add(worker);
					}
				}
			}
			workGiversInOrderNormal.Clear();
			for (int l = 0; l < wtsByPrio.Count; l++)
			{
				WorkTypeDef workTypeDef3 = wtsByPrio[l];
				for (int m = 0; m < workTypeDef3.workGiversByPriority.Count; m++)
				{
					WorkGiver worker2 = workTypeDef3.workGiversByPriority[m].Worker;
					if (!worker2.def.emergency || GetPriority(worker2.def.workType) > num)
					{
						workGiversInOrderNormal.Add(worker2);
					}
				}
			}
			workGiversDirty = false;
		}

		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("WorkSettings for " + pawn);
			stringBuilder.AppendLine("Cached emergency WorkGivers in order:");
			for (int i = 0; i < WorkGiversInOrderEmergency.Count; i++)
			{
				stringBuilder.AppendLine("   " + i + ": " + DebugStringFor(WorkGiversInOrderEmergency[i].def));
			}
			stringBuilder.AppendLine("Cached normal WorkGivers in order:");
			for (int j = 0; j < WorkGiversInOrderNormal.Count; j++)
			{
				stringBuilder.AppendLine("   " + j + ": " + DebugStringFor(WorkGiversInOrderNormal[j].def));
			}
			return stringBuilder.ToString();
		}

		private string DebugStringFor(WorkGiverDef wg)
		{
			return "[" + GetPriority(wg.workType) + " " + wg.workType.defName + "] - " + wg.defName + " (" + wg.priorityInType + ")";
		}
	}
}
