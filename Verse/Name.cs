namespace Verse
{
	public abstract class Name : IExposable
	{
		public abstract string ToStringFull
		{
			get;
		}

		public abstract string ToStringShort
		{
			get;
		}

		public abstract bool IsValid
		{
			get;
		}

		public bool UsedThisGame
		{
			get
			{
				foreach (Name item in NameUseChecker.AllPawnsNamesEverUsed)
				{
					if (item.ConfusinglySimilarTo(this))
					{
						return true;
					}
				}
				return false;
			}
		}

		public abstract bool Numerical
		{
			get;
		}

		public abstract bool ConfusinglySimilarTo(Name other);

		public abstract void ExposeData();
	}
}
