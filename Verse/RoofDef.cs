namespace Verse
{
	public class RoofDef : Def
	{
		public bool isNatural;

		public bool isThickRoof;

		public ThingDef collapseLeavingThingDef;

		public ThingDef filthLeaving;

		public SoundDef soundPunchThrough;

		public bool VanishOnCollapse => !isThickRoof;
	}
}
