using System;

namespace Verse
{
	public class SectionLayer_ThingsGeneral : SectionLayer_Things
	{
		public SectionLayer_ThingsGeneral(Section section)
			: base(section)
		{
			relevantChangeTypes = MapMeshFlag.Things;
			requireAddToMapMesh = true;
		}

		protected override void TakePrintFrom(Thing t)
		{
			try
			{
				t.Print(this);
			}
			catch (Exception ex)
			{
				Log.Error("Exception printing " + t + " at " + t.Position + ": " + ex.ToString());
			}
		}
	}
}
