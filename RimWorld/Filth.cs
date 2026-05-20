using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Filth : Thing
{
	public struct FilthInstance : IExposable
	{
		public float rotation;

		public Vector3 drawPos;

		public void ExposeData()
		{
			Scribe_Values.Look(ref rotation, "rotation", 0f);
			Scribe_Values.Look(ref drawPos, "drawPos");
		}
	}

	public int thickness = 1;

	public List<string> sources;

	private int growTick;

	private int disappearAfterTicks;

	public List<FilthInstance> drawInstances;

	private const int MaxThickness = 5;

	private const int MinAgeToPickUp = 400;

	private const int MaxNumSources = 3;

	public bool CanFilthAttachNow
	{
		get
		{
			if (def.filth.canFilthAttach && thickness > 1)
			{
				return Find.TickManager.TicksGame - growTick > 400;
			}
			return false;
		}
	}

	public bool CanBeThickened => thickness < 5;

	public int TicksSinceThickened => Find.TickManager.TicksGame - growTick;

	public int DisappearAfterTicks => disappearAfterTicks;

	public override string Label
	{
		get
		{
			if (sources.NullOrEmpty())
			{
				return "FilthLabel".Translate(base.Label, thickness.ToString());
			}
			return "FilthLabelWithSource".Translate(base.Label, sources.ToCommaList(useAnd: true), thickness.ToString());
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref growTick, "growTick", 0);
		Scribe_Values.Look(ref thickness, "thickness", 1);
		Scribe_Values.Look(ref disappearAfterTicks, "disappearAfterTicks", 0);
		if (Scribe.mode != LoadSaveMode.Saving || drawInstances != null)
		{
			Scribe_Collections.Look(ref drawInstances, "drawInstances", LookMode.Deep);
		}
		if (Scribe.mode != LoadSaveMode.Saving || sources != null)
		{
			Scribe_Collections.Look(ref sources, "sources", LookMode.Value);
		}
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (Current.ProgramState == ProgramState.Playing)
		{
			base.Map.listerFilthInHomeArea.Notify_FilthSpawned(this);
		}
		if (!respawningAfterLoad && !base.BeingTransportedOnGravship)
		{
			growTick = Find.TickManager.TicksGame;
			disappearAfterTicks = (int)(def.filth.disappearsInDays.RandomInRange * 60000f);
		}
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		Map map = base.Map;
		base.DeSpawn(mode);
		if (Current.ProgramState == ProgramState.Playing)
		{
			map.listerFilthInHomeArea.Notify_FilthDespawned(this);
		}
	}

	public void AddSource(string newSource)
	{
		if (sources == null)
		{
			sources = new List<string>();
		}
		for (int i = 0; i < sources.Count; i++)
		{
			if (sources[i] == newSource)
			{
				return;
			}
		}
		while (sources.Count > 3)
		{
			sources.RemoveAt(0);
		}
		sources.Add(newSource);
	}

	public void AddSources(IEnumerable<string> sources)
	{
		if (sources == null)
		{
			return;
		}
		foreach (string source in sources)
		{
			AddSource(source);
		}
	}

	public virtual void ThickenFilth()
	{
		growTick = Find.TickManager.TicksGame;
		if (thickness < def.filth.maxThickness)
		{
			thickness++;
			UpdateMesh();
		}
	}

	public void ThinFilth()
	{
		thickness--;
		if (base.Spawned)
		{
			if (thickness == 0)
			{
				Destroy();
			}
			else
			{
				UpdateMesh();
			}
		}
	}

	private void UpdateMesh()
	{
		if (base.Spawned)
		{
			base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things);
		}
	}

	public void SetOverrideDrawPositionAndRotation(Vector3 drawPos, float rotation)
	{
		if (drawInstances == null)
		{
			drawInstances = new List<FilthInstance>();
		}
		drawInstances.Add(new FilthInstance
		{
			drawPos = drawPos,
			rotation = rotation
		});
		UpdateMesh();
	}

	public bool CanDropAt(IntVec3 c, Map map, FilthSourceFlags additionalFlags = FilthSourceFlags.None)
	{
		return FilthMaker.CanMakeFilth(c, map, def, additionalFlags);
	}
}
