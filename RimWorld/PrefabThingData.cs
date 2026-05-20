using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Verse;

namespace RimWorld;

public class PrefabThingData
{
	public ThingDef def;

	public ThingDef stuff;

	public ColorDef colorDef;

	public Color color;

	public IntRange stackCountRange = IntRange.One;

	public int hp;

	public float chance = 1f;

	public bool canOverrideData = true;

	public QualityCategory? quality;

	public IntVec3 position;

	public List<CellRect> rects;

	public List<IntVec3> positions;

	public RotationDirection relativeRotation;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		XmlHelper.ParseElements(this, xmlRoot, "def");
	}
}
