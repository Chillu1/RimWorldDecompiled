namespace Verse
{
	public abstract class MapComponent : IExposable
	{
		public Map map;

		public MapComponent(Map map)
		{
			this.map = map;
		}

		public virtual void MapComponentUpdate()
		{
		}

		public virtual void MapComponentTick()
		{
		}

		public virtual void MapComponentOnGUI()
		{
		}

		public virtual void ExposeData()
		{
		}

		public virtual void FinalizeInit()
		{
		}

		public virtual void MapGenerated()
		{
		}

		public virtual void MapRemoved()
		{
		}
	}
}
