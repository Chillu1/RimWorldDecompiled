using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using UnityEngine.Rendering;

namespace Verse;

public sealed class DesignationManager : IExposable
{
	public readonly Map map;

	public readonly DefMap<DesignationDef, List<Designation>> designationsByDef = new DefMap<DesignationDef, List<Designation>>();

	private readonly DefMap<DesignationDef, List<Matrix4x4[]>> cellDesignationDrawMatricies = new DefMap<DesignationDef, List<Matrix4x4[]>>();

	private readonly DefMap<DesignationDef, bool> designationMatriciesDirty = new DefMap<DesignationDef, bool>();

	private readonly List<Designation>[] designationsAtCell;

	private readonly Dictionary<Thing, List<Designation>> thingDesignations = new Dictionary<Thing, List<Designation>>();

	private MaterialPropertyBlock tmpPropertyBlock;

	private List<Designation> saveDesignations = new List<Designation>();

	private static readonly List<Designation> EmptyList = new List<Designation>();

	private static readonly List<Designation> tmpDesignations = new List<Designation>();

	public List<Designation> AllDesignations
	{
		get
		{
			tmpDesignations.Clear();
			foreach (KeyValuePair<DesignationDef, List<Designation>> item in designationsByDef)
			{
				tmpDesignations.AddRange(item.Value);
			}
			return tmpDesignations;
		}
	}

	public DesignationManager(Map map)
	{
		this.map = map;
		designationsAtCell = new List<Designation>[map.cellIndices.NumGridCells];
	}

	public void ExposeData()
	{
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			saveDesignations.Clear();
			saveDesignations.AddRange(AllDesignations);
		}
		Scribe_Collections.Look(ref saveDesignations, "allDesignations", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			if (saveDesignations.RemoveAll((Designation x) => x == null) != 0)
			{
				Log.Warning("Some designations were null after loading.");
			}
			if (saveDesignations.RemoveAll((Designation x) => x.def == null) != 0)
			{
				Log.Warning("Some designations had null def after loading.");
			}
			foreach (Designation saveDesignation in saveDesignations)
			{
				saveDesignation.designationManager = this;
			}
		}
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		Plan plan = null;
		for (int num = saveDesignations.Count - 1; num >= 0; num--)
		{
			switch (saveDesignations[num].def.targetType)
			{
			case TargetType.Thing:
				if (!saveDesignations[num].target.HasThing)
				{
					Log.Error($"Thing-needing designation {saveDesignations[num]} had no thing target. Removing...");
					continue;
				}
				break;
			case TargetType.Cell:
				if (!saveDesignations[num].target.Cell.IsValid)
				{
					Log.Error($"Cell-needing designation {saveDesignations[num]} had no cell target. Removing...");
					continue;
				}
				break;
			}
			if (saveDesignations[num].def == DesignationDefOf.Plan)
			{
				Map map = this.map;
				if (map.planManager == null)
				{
					map.planManager = new PlanManager(this.map);
				}
				if (plan == null)
				{
					plan = new Plan(ColorDefOf.PlanGray, this.map.planManager);
				}
				plan.AddCell(saveDesignations[num].target.Cell);
				saveDesignations.RemoveAt(num);
			}
			else
			{
				IndexDesignation(saveDesignations[num]);
			}
		}
		plan?.CheckContiguous();
		saveDesignations.Clear();
	}

	public void DrawDesignations()
	{
		CellRect cellRect = Find.CameraDriver.CurrentViewRect.ExpandedBy(3);
		if (tmpPropertyBlock == null)
		{
			tmpPropertyBlock = new MaterialPropertyBlock();
		}
		foreach (DesignationDef item in DefDatabase<DesignationDef>.AllDefsListForReading)
		{
			if (item.targetType == TargetType.Cell && item.shouldBatchDraw && SystemInfo.supportsInstancing)
			{
				int count = designationsByDef[item].Count;
				if (designationsByDef[item].Count != 0)
				{
					CalculateCellDesignationDrawMatricies(item);
					List<Matrix4x4[]> list = cellDesignationDrawMatricies[item];
					item.iconMat.enableInstancing = true;
					int num = count / 1023;
					for (int i = 0; i < num; i++)
					{
						Graphics.DrawMeshInstanced(MeshPool.plane10, 0, item.iconMat, list[i], 1023, tmpPropertyBlock, ShadowCastingMode.Off, receiveShadows: true, 0);
					}
					int num2 = count % 1023;
					if (num2 > 0)
					{
						Graphics.DrawMeshInstanced(MeshPool.plane10, 0, item.iconMat, list[num], num2, tmpPropertyBlock, ShadowCastingMode.Off, receiveShadows: true, 0);
					}
				}
				continue;
			}
			foreach (Designation item2 in designationsByDef[item])
			{
				if ((!item2.target.HasThing || item2.target.Thing.Map == map) && cellRect.Contains(item2.target.Cell))
				{
					item2.DesignationDraw();
				}
			}
		}
	}

	private void DirtyCellDesignationsCache(DesignationDef def)
	{
		if (def.targetType == TargetType.Cell)
		{
			designationMatriciesDirty[def] = true;
		}
	}

	public void AddDesignation(Designation newDes)
	{
		if (newDes.def.targetType == TargetType.Cell && DesignationAt(newDes.target.Cell, newDes.def) != null)
		{
			Log.Error($"Tried to double-add designation at location {newDes.target}");
			return;
		}
		if (newDes.def.targetType == TargetType.Thing && DesignationOn(newDes.target.Thing, newDes.def) != null)
		{
			Log.Error($"Tried to double-add designation on Thing {newDes.target}");
			return;
		}
		if (newDes.def.targetType == TargetType.Thing)
		{
			newDes.target.Thing.SetForbidden(value: false, warnOnFail: false);
		}
		IndexDesignation(newDes);
		newDes.designationManager = this;
		newDes.Notify_Added();
		Map map = (newDes.target.HasThing ? newDes.target.Thing.Map : this.map);
		if (map != null)
		{
			FleckMaker.ThrowMetaPuffs(newDes.target.ToTargetInfo(map));
		}
	}

	private void IndexDesignation(Designation designation)
	{
		designationsByDef[designation.def].Add(designation);
		DirtyCellDesignationsCache(designation.def);
		if (designation.def.targetType == TargetType.Thing)
		{
			Thing thing = designation.target.Thing;
			if (!thingDesignations.ContainsKey(thing))
			{
				thingDesignations[thing] = new List<Designation>();
			}
			thingDesignations[thing].Add(designation);
		}
		else if (designation.def.targetType == TargetType.Cell)
		{
			if (TryGetCellDesignations(designation.target.Cell, out var foundDesignations, initializeIfNull: true))
			{
				foundDesignations.Add(designation);
			}
			else
			{
				Log.Error($"Tried to create cell target designation at invalid cell: {designation.target.Cell}");
			}
		}
		else
		{
			Log.Error($"Tried to index unexpected designation type: {designation.def.targetType}");
		}
	}

	private void CalculateCellDesignationDrawMatricies(DesignationDef def)
	{
		if (def.targetType == TargetType.Cell && designationMatriciesDirty[def])
		{
			List<Designation> list = designationsByDef[def];
			int count = list.Count;
			while (cellDesignationDrawMatricies[def].Count * 1023 < count)
			{
				cellDesignationDrawMatricies[def].Add(new Matrix4x4[1023]);
			}
			for (int i = 0; i < count; i++)
			{
				Designation designation = list[i];
				int index = i / 1023;
				cellDesignationDrawMatricies[def][index][i % 1023] = Matrix4x4.TRS(designation.DrawLoc(), Quaternion.identity, Vector3.one);
			}
			designationMatriciesDirty[def] = false;
		}
	}

	public Designation DesignationOn(Thing t)
	{
		thingDesignations.TryGetValue(t, out var value);
		if (value.NullOrEmpty())
		{
			return null;
		}
		return value[0];
	}

	public Designation DesignationOn(Thing t, DesignationDef def)
	{
		if (def.targetType == TargetType.Cell)
		{
			Log.Error("Designations of type " + def.defName + " are indexed by location only and you are trying to get one on a Thing.");
			return null;
		}
		thingDesignations.TryGetValue(t, out var value);
		if (value.NullOrEmpty())
		{
			return null;
		}
		foreach (Designation item in value)
		{
			if (item.def == def)
			{
				return item;
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
		if (TryGetCellDesignations(c, out var foundDesignations))
		{
			for (int i = 0; i < foundDesignations.Count; i++)
			{
				Designation designation = foundDesignations[i];
				if (designation.def == def)
				{
					return designation;
				}
			}
		}
		return null;
	}

	public List<Designation> AllDesignationsOn(Thing t)
	{
		if (!thingDesignations.ContainsKey(t))
		{
			return EmptyList;
		}
		return thingDesignations[t];
	}

	public List<Designation> AllDesignationsAt(IntVec3 c)
	{
		tmpDesignations.Clear();
		if (TryGetCellDesignations(c, out var foundDesignations))
		{
			tmpDesignations.AddRange(foundDesignations);
		}
		foreach (Thing item in map.thingGrid.ThingsListAt(c))
		{
			tmpDesignations.AddRange(AllDesignationsOn(item));
		}
		return tmpDesignations;
	}

	public bool HasMapDesignationAt(IntVec3 c)
	{
		if (TryGetCellDesignations(c, out var foundDesignations))
		{
			return !foundDesignations.NullOrEmpty();
		}
		return false;
	}

	public bool HasMapDesignationOn(Thing t)
	{
		return DesignationOn(t) != null;
	}

	public IEnumerable<Designation> SpawnedDesignationsOfDef(DesignationDef def)
	{
		foreach (Designation item in designationsByDef[def])
		{
			if (item.def == def && (!item.target.HasThing || item.target.Thing.Map == map))
			{
				yield return item;
			}
		}
	}

	public bool AnySpawnedDesignationOfDef(DesignationDef def)
	{
		foreach (Designation item in designationsByDef[def])
		{
			if (item.def == def && (!item.target.HasThing || item.target.Thing.Map == map))
			{
				return true;
			}
		}
		return false;
	}

	private bool TryGetCellDesignations(IntVec3 cell, out List<Designation> foundDesignations, bool initializeIfNull = false)
	{
		int num = map.cellIndices.CellToIndex(cell);
		if (num < 0 || num >= designationsAtCell.Length)
		{
			foundDesignations = null;
			return false;
		}
		foundDesignations = designationsAtCell[num];
		if (foundDesignations == null)
		{
			if (initializeIfNull)
			{
				foundDesignations = new List<Designation>();
				designationsAtCell[num] = foundDesignations;
			}
			else
			{
				foundDesignations = EmptyList;
			}
		}
		return true;
	}

	public void RemoveDesignation(Designation des)
	{
		des.Notify_Removing();
		if (des.def.targetType == TargetType.Cell)
		{
			if (TryGetCellDesignations(des.target.Cell, out var foundDesignations))
			{
				foundDesignations.Remove(des);
			}
			else
			{
				Log.Warning($"Tried to remove designation with target cell that couldn't be found in index: {des.target.Cell}");
			}
		}
		else if (des.def.targetType == TargetType.Thing)
		{
			Thing thing = des.target.Thing;
			if (thingDesignations.ContainsKey(thing))
			{
				List<Designation> list = thingDesignations[thing];
				list.Remove(des);
				if (list.Count == 0)
				{
					thingDesignations.Remove(des.target.Thing);
				}
			}
			else
			{
				Log.Warning("Tried to remove thing designation that wasn't indexed");
			}
		}
		else
		{
			Log.Error($"Tried to remove designation with unexpected type: {des.def.targetType}");
		}
		designationsByDef[des.def].Remove(des);
		DirtyCellDesignationsCache(des.def);
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
		List<Designation> list = AllDesignationsOn(t);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			Designation designation = list[num];
			if (!standardCanceling || designation.def.designateCancelable)
			{
				RemoveDesignation(list[num]);
			}
		}
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
		List<Designation> list = designationsByDef[def];
		for (int num = list.Count - 1; num >= 0; num--)
		{
			RemoveDesignation(list[num]);
		}
	}

	public void Notify_BuildingDespawned(Thing b)
	{
		CellRect cellRect = b.OccupiedRect();
		foreach (KeyValuePair<DesignationDef, List<Designation>> item in designationsByDef)
		{
			if (!item.Key.removeIfBuildingDespawned)
			{
				continue;
			}
			List<Designation> value = item.Value;
			for (int num = value.Count - 1; num >= 0; num--)
			{
				if (cellRect.Contains(value[num].target.Cell))
				{
					RemoveDesignation(value[num]);
				}
			}
		}
	}
}
