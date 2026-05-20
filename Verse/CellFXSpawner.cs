using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class CellFXSpawner
{
	private float fxPerTilePerTick;

	private Action<Vector3> emitterFunc;

	private float queue;

	public List<IntVec3> Cells { get; private set; }

	public CellFXSpawner(float fxPerTilePerSec, Action<Vector3> emitterFunc)
	{
		Cells = new List<IntVec3>();
		this.emitterFunc = emitterFunc;
		fxPerTilePerTick = fxPerTilePerSec / 60f;
	}

	public void Tick()
	{
		queue += fxPerTilePerTick * (float)Cells.Count;
		while (queue >= 1f)
		{
			emitterFunc(Cells.RandomElement().ToVector3Shifted());
			queue -= 1f;
		}
	}
}
