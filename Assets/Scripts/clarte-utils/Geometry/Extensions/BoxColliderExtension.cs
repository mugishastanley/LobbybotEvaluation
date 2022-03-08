using UnityEngine;

namespace CLARTE.Geometry.Extensions
{
	public static class BoxColliderExtension
	{
		/// <summary>
		/// Get the corners of the BoxCollider.
		/// </summary>
		/// <param name="box">The BoxCollider to get the corners form.</param>
		/// <param name="space">Defines if the result corners are defined in the local or world referential.</param>
		/// <returns>An array of 8 vectors, one for each of the box corners.</returns>
		public static Vector3[] GetCorners(this BoxCollider box, Space space = Space.Self)
		{
			Vector3 min = box.center - 0.5f * box.size;
			Vector3 max = box.center + 0.5f * box.size;

			Vector3[] corners = new Vector3[] {
				new Vector3(min.x, min.y, min.z),
				new Vector3(min.x, min.y, max.z),
				new Vector3(min.x, max.y, min.z),
				new Vector3(min.x, max.y, max.z),
				new Vector3(max.x, min.y, min.z),
				new Vector3(max.x, min.y, max.z),
				new Vector3(max.x, max.y, min.z),
				new Vector3(max.x, max.y, max.z)
			};

			if(space == Space.World)
			{
				Transform t = box.transform;

				int count = corners.Length;

				for(int i = 0; i < count; i++)
				{
					corners[i] = t.TransformPoint(corners[i]);
				}
			}

			return corners;
		}

		/// <summary>
		/// Computes intersection between two OBBs using the Separating Axis Theorem (SAT) algorithm.
		/// </summary>
		/// <param name="a">The first box.</param>
		/// <param name="b">The second box.</param>
		/// <returns>True if the two boxes are colliding, false otherwise.</returns>
		public static bool Collision(this BoxCollider a, BoxCollider b)
		{
			return Geometry.BoxBoxIntersection(a, b);
		}
	}
}
