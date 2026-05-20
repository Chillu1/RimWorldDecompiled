namespace RimWorld
{
	public static class PassionExtension
	{
		public static Passion IncrementPassion(this Passion passion)
		{
			return passion switch
			{
				Passion.None => Passion.Minor, 
				Passion.Minor => Passion.Major, 
				_ => passion, 
			};
		}
	}
}
