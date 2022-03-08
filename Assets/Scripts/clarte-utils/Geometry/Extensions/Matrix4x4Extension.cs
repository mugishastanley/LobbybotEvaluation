using UnityEngine;

namespace CLARTE.Geometry.Extensions
{
	public static class Matrix4x4Extension
	{
		/// <summary>
		/// Extract the rotation part of a Matrix4x4 and returns it as a quaternion
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		public static Quaternion ExtractRotationQuaternion(this Matrix4x4 matrix)
		{
			if(!IsOrthogonal(matrix))
			{
				Debug.LogWarning("Matrix is not orthogonal: conversion to quaternion doesn't make sense");
			}

			return Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
		}

		/// <summary>
		/// Extract the rotation part of a Matrix4x4 and returns it as a Matrix3x3
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		public static Matrix3x3 ExtractRotationMatrix(this Matrix4x4 matrix)
		{
			Matrix3x3 rot = new Matrix3x3();

			for(int i = 0; i < 3; i++)
			{
				for(int j = 0; j < 3; j++)
				{
					rot[i, j] = matrix[i, j];
				}
			}

			return rot;
		}

		/// <summary>
		/// Extract the rotation part of a Matrix4x4 and returns it as a quaternion
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		public static Vector3 ExtractTranslation(this Matrix4x4 matrix)
		{
			return new Vector3(matrix.m03, matrix.m13, matrix.m23);
		}

		/// <summary>
		/// Extract the scale part of a Matrix4x4
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		public static Vector3 ExtractScale(this Matrix4x4 matrix)
		{
			Vector3 ret = new Vector3();

			ret.x = matrix.GetColumn(0).magnitude;
			ret.y = matrix.GetColumn(1).magnitude;
			ret.z = matrix.GetColumn(2).magnitude;

			return ret;
		}

		/// <summary>
		/// Checks whether a Matrix4x4 is orthogonal
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		public static bool IsOrthogonal(this Matrix4x4 matrix)
		{
			bool ret = false;

			Vector4[] c = new Vector4[3];

			for(int i = 0; i < 3; i++)
			{
				c[i] = matrix.GetColumn(i);
			}

			float dot0 = Mathf.Abs(Vector4.Dot(c[0], c[1]));
			float dot1 = Mathf.Abs(Vector4.Dot(c[1], c[2]));
			float dot2 = Mathf.Abs(Vector4.Dot(c[0], c[2]));

			if(dot0 < Vector3.kEpsilon && dot1 < Vector3.kEpsilon && dot2 < Vector3.kEpsilon)
			{
				ret = true;
			}

			return ret;
		}
	}
}
