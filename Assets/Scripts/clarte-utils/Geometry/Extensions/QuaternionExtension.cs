using UnityEngine;

namespace CLARTE.Geometry.Extensions
{
	public static class QuaternionExtension
	{
		/// <summary>
		/// Check whether values of a quaternion are finite numbers
		/// </summary>
		/// <param name="quat"></param>
		/// <returns></returns>
		public static bool IsValid(this Quaternion quaternion)
		{
			if(float.IsNaN(quaternion.x) || float.IsNaN(quaternion.y) || float.IsNaN(quaternion.z) || float.IsNaN(quaternion.w))
				return false;
			else
				return true;
		}

		/// <summary>
		/// Compute rotation axis from a quaternion
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>
		static public Vector3 QuaternionAxis(this Quaternion quaternion)
		{
			float sin = 1;

			if(quaternion.w < 1)
			{
				sin = 1 / Mathf.Sqrt(1 - quaternion.w * quaternion.w);
			}
			else
			{
				Debug.LogWarning("One the rotations was identity. Computation may be wrong.");
			}

			return new Vector3(quaternion.x * sin, quaternion.y * sin, quaternion.z * sin);
		}
	}
}
