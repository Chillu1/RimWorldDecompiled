using RimWorld;
using UnityEngine;

namespace Verse;

public class Graphic_ClusterTight : Graphic_Cluster
{
	protected override float PositionVariance => 0.2f;

	protected override float SizeVariance => 0.15f;

	protected override int ScatterCount(Thing thing)
	{
		if (thing is Filth filth && !filth.drawInstances.NullOrEmpty())
		{
			return filth.drawInstances.Count;
		}
		return 1;
	}

	protected override Vector3 GetCenter(Thing thing, int index)
	{
		if (thing is Filth { drawInstances: not null } filth && filth.drawInstances.Count > index)
		{
			return filth.drawInstances[index].drawPos;
		}
		return thing.DrawPos;
	}
}
