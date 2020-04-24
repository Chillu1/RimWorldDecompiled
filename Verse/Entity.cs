using System;

namespace Verse
{
	public abstract class Entity
	{
		public abstract string LabelCap
		{
			get;
		}

		public abstract string Label
		{
			get;
		}

		public virtual string LabelShort => LabelCap;

		public virtual string LabelMouseover => LabelCap;

		public virtual string LabelShortCap => LabelShort.CapitalizeFirst();

		public abstract void SpawnSetup(Map map, bool respawningAfterLoad);

		public abstract void DeSpawn(DestroyMode mode = DestroyMode.Vanish);

		public virtual void Tick()
		{
			throw new NotImplementedException();
		}

		public virtual void TickRare()
		{
			throw new NotImplementedException();
		}

		public virtual void TickLong()
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return LabelCap;
		}
	}
}
