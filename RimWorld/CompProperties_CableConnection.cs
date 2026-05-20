using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompProperties_CableConnection : CompProperties
{
	public Color color = Color.white;

	public List<List<Vector3>> offsets = new List<List<Vector3>>();

	public bool drawMote;

	public ThingDef moteDef;

	public CompProperties_CableConnection()
	{
		compClass = typeof(CompCableConnection);
	}
}
