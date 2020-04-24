using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class TransferableOneWay : Transferable
	{
		public List<Thing> things = new List<Thing>();

		private int countToTransfer;

		public override Thing AnyThing
		{
			get
			{
				if (!HasAnyThing)
				{
					return null;
				}
				return things[0];
			}
		}

		public override ThingDef ThingDef
		{
			get
			{
				if (!HasAnyThing)
				{
					return null;
				}
				return AnyThing.def;
			}
		}

		public override bool HasAnyThing => things.Count != 0;

		public override string Label => AnyThing.LabelNoCount;

		public override bool Interactive => true;

		public override TransferablePositiveCountDirection PositiveCountDirection => TransferablePositiveCountDirection.Destination;

		public override string TipDescription
		{
			get
			{
				if (!HasAnyThing)
				{
					return "";
				}
				return AnyThing.DescriptionDetailed;
			}
		}

		public override int CountToTransfer
		{
			get
			{
				return countToTransfer;
			}
			protected set
			{
				countToTransfer = value;
				base.EditBuffer = value.ToStringCached();
			}
		}

		public int MaxCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < things.Count; i++)
				{
					num += things[i].stackCount;
				}
				return num;
			}
		}

		public override int GetMinimumToTransfer()
		{
			return 0;
		}

		public override int GetMaximumToTransfer()
		{
			return MaxCount;
		}

		public override AcceptanceReport OverflowReport()
		{
			return new AcceptanceReport("ColonyHasNoMore".Translate());
		}

		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				things.RemoveAll((Thing x) => x.Destroyed);
			}
			Scribe_Values.Look(ref countToTransfer, "countToTransfer", 0);
			Scribe_Collections.Look(ref things, "things", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				base.EditBuffer = countToTransfer.ToStringCached();
			}
			if (Scribe.mode == LoadSaveMode.PostLoadInit && things.RemoveAll((Thing x) => x == null) != 0)
			{
				Log.Warning("Some of the things were null after loading.");
			}
		}
	}
}
