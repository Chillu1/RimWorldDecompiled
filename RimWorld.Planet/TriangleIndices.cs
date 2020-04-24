namespace RimWorld.Planet
{
	public struct TriangleIndices
	{
		public int v1;

		public int v2;

		public int v3;

		public TriangleIndices(int v1, int v2, int v3)
		{
			this.v1 = v1;
			this.v2 = v2;
			this.v3 = v3;
		}

		public bool SharesAnyVertexWith(TriangleIndices t, int otherThan)
		{
			if ((v1 == otherThan || (v1 != t.v1 && v1 != t.v2 && v1 != t.v3)) && (v2 == otherThan || (v2 != t.v1 && v2 != t.v2 && v2 != t.v3)))
			{
				if (v3 != otherThan)
				{
					if (v3 != t.v1 && v3 != t.v2)
					{
						return v3 == t.v3;
					}
					return true;
				}
				return false;
			}
			return true;
		}

		public int GetNextOrderedVertex(int root)
		{
			if (v1 == root)
			{
				return v2;
			}
			if (v2 == root)
			{
				return v3;
			}
			return v1;
		}
	}
}
