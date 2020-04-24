using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Transferable : IExposable
	{
		private string editBuffer = "0";

		public abstract Thing AnyThing
		{
			get;
		}

		public abstract ThingDef ThingDef
		{
			get;
		}

		public abstract bool Interactive
		{
			get;
		}

		public abstract bool HasAnyThing
		{
			get;
		}

		public virtual bool IsThing => true;

		public abstract string Label
		{
			get;
		}

		public string LabelCap => Label.CapitalizeFirst(ThingDef);

		public abstract string TipDescription
		{
			get;
		}

		public abstract TransferablePositiveCountDirection PositiveCountDirection
		{
			get;
		}

		public abstract int CountToTransfer
		{
			get;
			protected set;
		}

		public int CountToTransferToSource
		{
			get
			{
				if (PositiveCountDirection != 0)
				{
					return -CountToTransfer;
				}
				return CountToTransfer;
			}
		}

		public int CountToTransferToDestination
		{
			get
			{
				if (PositiveCountDirection != 0)
				{
					return CountToTransfer;
				}
				return -CountToTransfer;
			}
		}

		public string EditBuffer
		{
			get
			{
				return editBuffer;
			}
			set
			{
				editBuffer = value;
			}
		}

		public abstract int GetMinimumToTransfer();

		public abstract int GetMaximumToTransfer();

		public int GetRange()
		{
			return GetMaximumToTransfer() - GetMinimumToTransfer();
		}

		public int ClampAmount(int amount)
		{
			return Mathf.Clamp(amount, GetMinimumToTransfer(), GetMaximumToTransfer());
		}

		public AcceptanceReport CanAdjustBy(int adjustment)
		{
			return CanAdjustTo(CountToTransfer + adjustment);
		}

		public AcceptanceReport CanAdjustTo(int destination)
		{
			if (destination == CountToTransfer)
			{
				return AcceptanceReport.WasAccepted;
			}
			if (ClampAmount(destination) != CountToTransfer)
			{
				return AcceptanceReport.WasAccepted;
			}
			if (destination < CountToTransfer)
			{
				return UnderflowReport();
			}
			return OverflowReport();
		}

		public void AdjustBy(int adjustment)
		{
			AdjustTo(CountToTransfer + adjustment);
		}

		public void AdjustTo(int destination)
		{
			if (!CanAdjustTo(destination).Accepted)
			{
				Log.Error("Failed to adjust transferable counts");
			}
			else
			{
				CountToTransfer = ClampAmount(destination);
			}
		}

		public void ForceTo(int value)
		{
			CountToTransfer = value;
		}

		public void ForceToSource(int value)
		{
			if (PositiveCountDirection == TransferablePositiveCountDirection.Source)
			{
				ForceTo(value);
			}
			else
			{
				ForceTo(-value);
			}
		}

		public void ForceToDestination(int value)
		{
			if (PositiveCountDirection == TransferablePositiveCountDirection.Source)
			{
				ForceTo(-value);
			}
			else
			{
				ForceTo(value);
			}
		}

		public virtual void DrawIcon(Rect iconRect)
		{
		}

		public virtual AcceptanceReport UnderflowReport()
		{
			return false;
		}

		public virtual AcceptanceReport OverflowReport()
		{
			return false;
		}

		public virtual void ExposeData()
		{
		}
	}
}
