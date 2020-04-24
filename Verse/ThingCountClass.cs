namespace Verse
{
	public sealed class ThingCountClass : IExposable
	{
		public Thing thing;

		private int countInt;

		public int Count
		{
			get
			{
				return countInt;
			}
			set
			{
				if (value < 0)
				{
					Log.Warning("Tried to set ThingCountClass stack count to " + value + ". thing=" + thing);
					countInt = 0;
				}
				else if (thing != null && value > thing.stackCount)
				{
					Log.Warning("Tried to set ThingCountClass stack count to " + value + ", but thing's stack count is only " + thing.stackCount + ". thing=" + thing);
					countInt = thing.stackCount;
				}
				else
				{
					countInt = value;
				}
			}
		}

		public ThingCountClass()
		{
		}

		public ThingCountClass(Thing thing, int count)
		{
			this.thing = thing;
			Count = count;
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref thing, "thing");
			Scribe_Values.Look(ref countInt, "count", 1);
		}

		public override string ToString()
		{
			return "(" + Count + "x " + ((thing != null) ? thing.LabelShort : "null") + ")";
		}

		public static implicit operator ThingCountClass(ThingCount t)
		{
			return new ThingCountClass(t.Thing, t.Count);
		}
	}
}
