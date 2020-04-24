using System;
using Verse;

namespace RimWorld
{
	public class TransferableSorterDef : Def
	{
		public Type comparerClass;

		[Unsaved(false)]
		private TransferableComparer comparerInt;

		public TransferableComparer Comparer
		{
			get
			{
				if (comparerInt == null)
				{
					comparerInt = (TransferableComparer)Activator.CreateInstance(comparerClass);
				}
				return comparerInt;
			}
		}
	}
}
