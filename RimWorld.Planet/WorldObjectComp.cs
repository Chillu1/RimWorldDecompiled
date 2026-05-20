using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet;

public abstract class WorldObjectComp
{
	public WorldObject parent;

	public WorldObjectCompProperties props;

	public IThingHolder ParentHolder => parent.ParentHolder;

	public bool ParentHasMap
	{
		get
		{
			if (parent is MapParent mapParent)
			{
				return mapParent.HasMap;
			}
			return false;
		}
	}

	public virtual void Initialize(WorldObjectCompProperties props)
	{
		this.props = props;
	}

	public virtual void CompTick()
	{
	}

	public virtual void CompTickInterval(int delta)
	{
	}

	public virtual IEnumerable<Gizmo> GetGizmos()
	{
		return Enumerable.Empty<Gizmo>();
	}

	public virtual IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
	{
		return Enumerable.Empty<FloatMenuOption>();
	}

	public virtual IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
	{
		return Enumerable.Empty<Gizmo>();
	}

	public virtual IEnumerable<IncidentTargetTagDef> IncidentTargetTags()
	{
		return Enumerable.Empty<IncidentTargetTagDef>();
	}

	public virtual void PostDrawExtraSelectionOverlays()
	{
	}

	public virtual string CompInspectStringExtra()
	{
		return null;
	}

	public virtual string GetDescriptionPart()
	{
		return null;
	}

	public virtual void PostPostRemove()
	{
	}

	public virtual void PostDestroy()
	{
	}

	public virtual void PostMyMapRemoved()
	{
	}

	public void PostMyMapSettled()
	{
	}

	public virtual void PostMapGenerate()
	{
	}

	public virtual void PostCaravanFormed(Caravan caravan)
	{
	}

	public virtual void PostExposeData()
	{
	}

	public override string ToString()
	{
		return $"{GetType().Name}(parent={parent} at={((parent != null) ? parent.Tile.tileId : (-1))})";
	}
}
