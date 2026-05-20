using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RiverNode
{
	public List<RiverNode> childNodes = new List<RiverNode>();

	public Vector3 start;

	public Vector3 end;

	public float width;

	public int seed = Rand.Int;
}
