using System;
using UnityEngine;

namespace CLARTE.Geometry
{
	public partial struct Matrix3x3
	{
		#region Members
		public const int size = 3;

		// mXY where X = row and Y = column
		public float m00;
		public float m10;
		public float m20;
		public float m01;
		public float m11;
		public float m21;
		public float m02;
		public float m12;
		public float m22;
		#endregion

		#region Equals override
		public override int GetHashCode()
		{
			return Backport.Tuple.CombineHashCodes(GetColumn(0).GetHashCode(), GetColumn(1).GetHashCode(), GetColumn(2).GetHashCode());
		}

		public override bool Equals(object other)
		{
			bool result = false;

			if(other is Matrix3x3)
			{
				Matrix3x3 matrix3x3 = (Matrix3x3) other;

				result = true;

				for(int i = 0; result && i < size; i++)
				{
					result = GetColumn(i).Equals(matrix3x3.GetColumn(i));
				}
			}

			return result;
		}
		#endregion

		#region Getter / Setter
		public static Matrix3x3 Zero
		{
			get
			{
				return new Matrix3x3
				{
					m00 = 0f,
					m01 = 0f,
					m02 = 0f,
					m10 = 0f,
					m11 = 0f,
					m12 = 0f,
					m20 = 0f,
					m21 = 0f,
					m22 = 0f
				};
			}
		}

		public static Matrix3x3 Identity
		{
			get
			{
				return new Matrix3x3
				{
					m00 = 1f,
					m01 = 0f,
					m02 = 0f,
					m10 = 0f,
					m11 = 1f,
					m12 = 0f,
					m20 = 0f,
					m21 = 0f,
					m22 = 1f
				};
			}
		}

		public Matrix3x3 Transposed
		{
			get
			{
				Matrix3x3 transposed = new Matrix3x3();

				transposed.m00 = m00;
				transposed.m10 = m01;
				transposed.m20 = m02;
				transposed.m01 = m10;
				transposed.m11 = m11;
				transposed.m21 = m12;
				transposed.m02 = m20;
				transposed.m12 = m21;
				transposed.m22 = m22;

				return transposed;
			}
		}

		public Vector3 GetColumn(int i)
		{
			return new Vector3(this[0, i], this[1, i], this[2, i]);
		}

		public Vector3 GetRow(int i)
		{
			return new Vector3(this[i, 0], this[i, 1], this[i, 2]);
		}

		public void SetColumn(int i, Vector3 v)
		{
			this[0, i] = v.x;
			this[1, i] = v.y;
			this[2, i] = v.z;
		}

		public void SetRow(int i, Vector3 v)
		{
			this[i, 0] = v.x;
			this[i, 1] = v.y;
			this[i, 2] = v.z;
		}
		#endregion

		#region Index accessors
		public float this[int row, int column]
		{
			get
			{
				return this[column * size + row];
			}

			set
			{
				this[column * size + row] = value;
			}
		}

		public float this[int index]
		{
			get
			{
				float result;

				switch(index)
				{
					case 0:
						result = m00;
						break;
					case 1:
						result = m10;
						break;
					case 2:
						result = m20;
						break;
					case 3:
						result = m01;
						break;
					case 4:
						result = m11;
						break;
					case 5:
						result = m21;
						break;
					case 6:
						result = m02;
						break;
					case 7:
						result = m12;
						break;
					case 8:
						result = m22;
						break;
					default:
						throw new IndexOutOfRangeException(string.Format("Invalid matrix index '{0}'", index));
				}

				return result;
			}

			set
			{
				switch(index)
				{
					case 0:
						m00 = value;
						break;
					case 1:
						m10 = value;
						break;
					case 2:
						m20 = value;
						break;
					case 3:
						m01 = value;
						break;
					case 4:
						m11 = value;
						break;
					case 5:
						m21 = value;
						break;
					case 6:
						m02 = value;
						break;
					case 7:
						m12 = value;
						break;
					case 8:
						m22 = value;
						break;
					default:
						throw new IndexOutOfRangeException(string.Format("Invalid matrix index '{0}'", index));
				}
			}
		}
		#endregion

		#region Operators overloads
		public static Matrix3x3 operator *(Matrix3x3 lhs, Matrix3x3 rhs)
		{
			Matrix3x3 res = new Matrix3x3();

			res.m00 = lhs.m00 * rhs.m00 + lhs.m01 * rhs.m10 + lhs.m02 * rhs.m20;
			res.m01 = lhs.m00 * rhs.m01 + lhs.m01 * rhs.m11 + lhs.m02 * rhs.m21;
			res.m02 = lhs.m00 * rhs.m02 + lhs.m01 * rhs.m12 + lhs.m02 * rhs.m22;
			res.m10 = lhs.m10 * rhs.m00 + lhs.m11 * rhs.m10 + lhs.m12 * rhs.m20;
			res.m11 = lhs.m10 * rhs.m01 + lhs.m11 * rhs.m11 + lhs.m12 * rhs.m21;
			res.m12 = lhs.m10 * rhs.m02 + lhs.m11 * rhs.m12 + lhs.m12 * rhs.m22;
			res.m20 = lhs.m20 * rhs.m00 + lhs.m21 * rhs.m10 + lhs.m22 * rhs.m20;
			res.m21 = lhs.m20 * rhs.m01 + lhs.m21 * rhs.m11 + lhs.m22 * rhs.m21;
			res.m22 = lhs.m20 * rhs.m02 + lhs.m21 * rhs.m12 + lhs.m22 * rhs.m22;

			return res;
		}

		public static Vector3 operator *(Matrix3x3 lhs, Vector3 v)
		{
			Vector3 result;

			result.x = lhs.m00 * v.x + lhs.m01 * v.y + lhs.m02 * v.z;
			result.y = lhs.m10 * v.x + lhs.m11 * v.y + lhs.m12 * v.z;
			result.z = lhs.m20 * v.x + lhs.m21 * v.y + lhs.m22 * v.z;

			return result;
		}

		public static bool operator ==(Matrix3x3 lhs, Matrix3x3 rhs)
		{
			bool result = true;

			for(int i = 0; result && i < size; i++)
			{
				result = lhs.GetColumn(i) == rhs.GetColumn(i);
			}

			return result;
		}

		public static bool operator !=(Matrix3x3 lhs, Matrix3x3 rhs)
		{
			return !(lhs == rhs);
		}
		#endregion
	}
}