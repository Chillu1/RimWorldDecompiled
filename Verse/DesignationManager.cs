using System.Collections.Generic;
using RimWorld;

namespace Verse
{
	public sealed class DesignationManager : IExposable
	{
		public Map map;

		public List<Designation> allDesignations = new List<Designation>();

		public DesignationManager(Map map)
		{
			this.map = map;
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref allDesignations, "allDesignations", LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				if (allDesignations.RemoveAll((Designation x) => x == null) != 0)
				{
					Log.Warning("Some designations were null after loading.");
				}
				if (allDesignations.RemoveAll((Designation x) => x.def == null) != 0)
				{
					Log.Warning("Some designations had null def after loading.");
				}
				for (int i = 0; i < allDesignations.Count; i++)
				{
					allDesignations[i].designationManager = this;
				}
			}
			if (Scribe.mode != LoadSaveMode.PostLoadInit)
			{
				return;
			}
			for (int num = allDesignations.Count - 1; num >= 0; num--)
			{
				switch (allDesignations[num].def.targetType)
				{
				case TargetType.Thing:
					if (!allDesignations[num].target.HasThing)
					{
						Log.Error(string.Concat("Thing-needing designation ", allDesignations[num], " had no thing target. Removing..."));
						allDesignations.RemoveAt(num);
					}
					break;
				case TargetType.Cell:
					if (!allDesignations[num].target.Cell.IsValid)
					{
						Log.Error(string.Concat("Cell-needing designation ", allDesignations[num], " had no cell target. Removing..."));
						allDesignations.RemoveAt(num);
					}
					break;
				}
			}
		}

		public void DrawDesignations()
		{
			for (int i = 0; i < allDesignations.Count; i++)
			{
				if (!allDesignations[i].target.HasThing || allDesignations[i].target.Thing.Map == map)
				{
					allDesignations[i].DesignationDraw();
				}
			}
		}

		public void AddDesignation(Designation newDes)
		{
			if (newDes.def.targetType == TargetType.Cell && DesignationAt(newDes.target.Cell, newDes.def) != null)
			{
				Log.Error("Tried to double-add designation at location " + newDes.target);
				return;
			}
			if (newDes.def.targetType == TargetType.Thing && DesignationOn(newDes.target.Thing, newDes.def) != null)
			{
				Log.Error("Tried to double-add designation on Thing " + newDes.target);
				return;
			}
			if (newDes.def.targetType == TargetType.Thing)
			{
				newDes.target.Thing.SetForbidden(value: false, warnOnFail: false);
			}
			allDesignations.Add(newDes);
			newDes.designationManager = this;
			newDes.Notify_Added();
			Map map = (newDes.target.HasThing ? newDes.target.Thing.Map : this.map);
			if (map != null)
			{
				MoteMaker.ThrowMetaPuffs(newDes.target.ToTargetInfo(map));
			}
		}

		public Designation DesignationOn(Thing t)
		{
			for (int i = 0; i < allDesignations.Count; i++)
			{
				Designation designation = allDesignations[i];
				if (designation.target.Thing == t)
				{
					return designation;
				}
			}
			return null;
		}

		public Designation DesignationOn(Thing t, DesignationDef def)
		{
			if (def.targetType == TargetType.Cell)
			{
				Log.Error("Designations of type " + def.defName + " are indexed by location only and you are trying to get one on a Thing.");
				return null;
			}
			for (int i = 0; i < allDesignations.Count; i++)
			{
				Designation designation = allDesignations[i];
				if (designation.target.Thing == t && designation.def == def)
				{
					return designation;
				}
			}
			return null;
		}

		public Designation DesignationAt(IntVec3 c, DesignationDef def)
		{
			if (def.targetType == TargetType.Thing)
			{
				Log.Error("Designations of type " + def.defName + " are indexed by Thing only and you are trying to get one on a location.");
				return null;
			}
			for (int i = 0; i < allDesignations.Count; i++)
			{
				Designation designation = allDesignations[i];
				if (designation.def == def && (!designation.target.HasThing || designation.target.Thing.Map == map) && designation.target.Cell == c)
				{
					return designation;
				}
			}
			return null;
		}

		public IEnumerable<Designation> AllDesignationsOn(Thing t)
		{
			int count = allDesignations.Count;
			for (int i = 0; i < count; i++)
			{
				if (allDesignations[i].target.Thing == t)
				{
					yield return allDesignations[i];
				}
			}
		}

		public IEnumerable<Designation> AllDesignationsAt(IntVec3 c)
		{
			int count = allDesignations.Count;
			for (int i = 0; i < count; i++)
			{
				Designation designation = allDesignations[i];
				if ((!designation.target.HasThing || designation.target.Thing.Map == map) && designation.target.Cell == c)
				{
					yield return designation;
				}
			}
		}

		public bool HasMapDesignationAt(IntVec3 c)
		{
			int count = allDesignations.Count;
			for (int i = 0; i < count; i++)
			{
				Designation designation = allDesignations[i];
				if (!designation.target.HasThing && designation.target.Cell == c)
				{
					return true;
				}
			}
			return false;
		}

		public IEnumerable<Designation> SpawnedDesignationsOfDef(DesignationDef def)
		{
			int count = allDesignations.Count;
			for (int i = 0; i < count; i++)
			{
				Designation designation = allDesignations[i];
				if (designation.def == def && (!designation.target.HasThing || designation.target.Thing.Map == map))
				{
					yield return designation;
				}
			}
		}

		public bool AnySpawnedDesignationOfDef(DesignationDef def)
		{
			int count = allDesignations.Count;
			for (int i = 0; i < count; i++)
			{
				Designation designation = allDesignations[i];
				if (designation.def == def && (!designation.target.HasThing || designation.target.Thing.Map == map))
				{
					return true;
				}
			}
			return false;
		}

		public void RemoveDesignation(Designation des)
		{
			des.Notify_Removing();
			allDesignations.Remove(des);
		}

		public void TryRemoveDesignation(IntVec3 c, DesignationDef def)
		{
			Designation designation = DesignationAt(c, def);
			if (designation != null)
			{
				RemoveDesignation(designation);
			}
		}

		public void RemoveAllDesignationsOn(Thing t, bool standardCanceling = false)
		{
			for (int i = 0; i < allDesignations.Count; i++)
			{
				Designation designation = allDesignations[i];
				if ((!standardCanceling || designation.def.designateCancelable) && designation.target.Thing == t)
				{
					designation.Notify_Removing();
				}
			}
			allDesignations.RemoveAll((Designation d) => (!standardCanceling || d.def.designateCancelable) && d.target.Thing == t);
		}

		public void TryRemoveDesignationOn(Thing t, DesignationDef def)
		{
			Designation designation = DesignationOn(t, def);
			if (designation != null)
			{
				RemoveDesignation(designation);
			}
		}

		public void RemoveAllDesignationsOfDef(DesignationDef def)
		{
			for (int num = allDesignations.Count - 1; num >= 0; num--)
			{
				if (allDesignations[num].def == def)
				{
					allDesignations[num].Notify_Removing();
					allDesignations.RemoveAt(num);
				}
			}
		}

		public void Notify_BuildingDespawned(Thing b)
		{
			CellRect cellRect = b.OccupiedRect();
			for (int num = allDesignations.Count - 1; num >= 0; num--)
			{
				Designation designation = allDesignations[num];
				if (cellRect.Contains(designation.target.Cell) && designation.def.removeIfBuildingDespawned)
				{
					RemoveDesignation(designation);
				}
			}
		}
	}
}
