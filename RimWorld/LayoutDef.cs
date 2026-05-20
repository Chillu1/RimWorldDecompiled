using System;
using System.Collections.Generic;
using System.Xml;
using Verse;

namespace RimWorld;

public class LayoutDef : Def
{
	public class LayoutRoomWeight
	{
		public LayoutRoomDef def;

		public float weight = 1f;

		public IntRange countRange = new IntRange(0, int.MaxValue);

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			XmlHelper.ParseElements(this, xmlRoot, "def", "weight");
		}
	}

	public List<LayoutRoomWeight> roomDefs;

	public Type workerClass;

	public bool clearRoomsEntirely;

	public bool canHaveMultipleLayoutsInRoom;

	public float multipleLayoutRoomChance = 0.15f;

	public bool shouldDamage;

	public bool noRoof;

	public float adjacentRoomMergeChance = 0.65f;

	public float borderDoorRemoveChance = 0.8f;

	public int minRoomWidth = 6;

	public int minRoomHeight = 6;

	public List<LayoutScatterParms> junkScaterrers = new List<LayoutScatterParms>();

	public List<LayoutScatterTerrainParms> scatterTerrain = new List<LayoutScatterTerrainParms>();

	public List<LayoutFillEdgesParms> fillEdges = new List<LayoutFillEdgesParms>();

	public List<LayoutFillInteriorParms> fillInterior = new List<LayoutFillInteriorParms>();

	public List<LayoutWallAttatchmentParms> wallAttachments = new List<LayoutWallAttatchmentParms>();

	public List<LayoutPrefabParms> prefabs = new List<LayoutPrefabParms>();

	public List<LayoutPartParms> parts = new List<LayoutPartParms>();

	private LayoutWorker layoutWorker;

	public LayoutWorker Worker => layoutWorker ?? (layoutWorker = (LayoutWorker)Activator.CreateInstance(workerClass, this));
}
