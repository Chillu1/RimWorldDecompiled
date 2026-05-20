using UnityEngine;

namespace Verse
{
	public class AsymmetricLinkData
	{
		public class BorderData
		{
			public Color color = Color.black;

			public Vector2 size;

			public Vector3 offset;

			private Material colorMat;

			public Material Mat
			{
				get
				{
					if (colorMat == null)
					{
						colorMat = SolidColorMaterials.SimpleSolidColorMaterial(color);
					}
					return colorMat;
				}
			}
		}

		public LinkFlags linkFlags;

		public bool linkToDoors;

		public BorderData drawDoorBorderEast;

		public BorderData drawDoorBorderWest;
	}
}
