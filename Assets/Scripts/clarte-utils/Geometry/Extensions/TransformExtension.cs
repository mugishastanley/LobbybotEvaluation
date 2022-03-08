using UnityEngine;

namespace CLARTE.Geometry.Extensions
{
	public static class TransformExtension
	{
		/// <summary>
		/// Set position of the Transform in a given referential
		/// </summary>
		/// <param name="transf"></param>
		/// <param name="position"></param>
		/// <param name="referential"></param>
		public static void SetPosition(this Transform transform, Vector3 position, Transform referential = null)
		{
			if(referential != null)
			{
				transform.position = referential.TransformPoint(position);
			}
			else
			{
				transform.position = position;
			}
		}

		/// <summary>
		/// Get the position of the Transform in a given referential
		/// </summary>
		/// <param name="transf"></param>
		/// <param name="referential"></param>
		/// <returns></returns>
		public static Vector3 GetPosition(this Transform transform, Transform referential = null)
		{
			if(referential != null)
			{
				return referential.InverseTransformPoint(transform.position);
			}
			else
			{
				return transform.position;
			}
		}

		/// <summary>
		/// Set the orientation of the Transform in a given referential
		/// </summary>
		/// <param name="transf"></param>
		/// <param name="orientation"></param>
		/// <param name="referential"></param>
		public static void SetOrientation(this Transform transform, Quaternion orientation, Transform referential = null)
		{
			if(referential != null)
			{
				transform.rotation = referential.rotation * orientation;
			}
			else
			{
				transform.rotation = orientation;
			}
		}

		/// <summary>
		/// Get the orientation of the Transform in a given referential
		/// </summary>
		/// <param name="transf"></param>
		/// <param name="referential"></param>
		/// <returns></returns>
		public static Quaternion GetOrientation(this Transform transform, Transform referential = null)
		{
			if(referential != null)
			{
				return Quaternion.Inverse(referential.rotation) * transform.rotation;
			}
			else
			{
				return transform.rotation;
			}
		}

		/// <summary>
		/// Returns the components of the Transform Forward (z) axis in a given referential
		/// </summary>
		/// <param name="transf"></param>
		/// <param name="referential"></param>
		/// <returns></returns>
		public static Vector3 Forward(this Transform transform, Transform referential = null)
		{
			if(referential != null)
			{
				return referential.InverseTransformVector(transform.forward);
			}
			else
			{
				return transform.forward;
			}
		}

		/// <summary>
		/// Returns the components of the Transform Up (y) axis in a given referential
		/// </summary>
		/// <param name="transf"></param>
		/// <param name="referential"></param>
		/// <returns></returns>
		public static Vector3 Up(this Transform transform, Transform referential = null)
		{
			if(referential != null)
			{
				return referential.InverseTransformVector(transform.up);
			}
			else
			{
				return transform.up;
			}
		}

		/// <summary>
		/// Returns the components of the Transform Right (x) axis in a given referential
		/// </summary>
		/// <param name="transf"></param>
		/// <param name="referential"></param>
		/// <returns></returns>
		public static Vector3 Right(this Transform transform, Transform referential = null)
		{
			if(referential != null)
			{
				return referential.InverseTransformVector(transform.right);
			}
			else
			{
				return transform.right;
			}
		}

		/// <summary>
		/// Show every object descending from the Transform
		/// </summary>
		/// <param name="transf"></param>
		/// <param name="state"></param>
		public static void ShowHierarchy(this Transform transform, bool state)
		{
			Renderer[] renderers = transform.gameObject.GetComponentsInChildren<Renderer>();

			if(renderers != null)
			{
				foreach(Renderer renderer in renderers)
				{
					renderer.enabled = state;
				}
			}
		}

		/// <summary>
		/// Set the local matrix of the Transform
		/// </summary>
		/// <param name="transf"></param>
		/// <param name="mat"></param>
		public static void SetLocalMatrix(this Transform transform, Matrix4x4 matrix)
		{
			transform.localPosition = matrix.ExtractTranslation();
			transform.localRotation = matrix.ExtractRotationQuaternion();
			transform.localScale = matrix.ExtractScale();
		}

		/// <summary>
		/// Returns the local matrix of the Transform
		/// </summary>
		/// <param name="transf"></param>
		/// <returns></returns>
		public static Matrix4x4 GetLocalMatrix(this Transform transform)
		{
			Matrix4x4 mat = new Matrix4x4();

			mat.SetTRS(transform.localPosition, transform.localRotation, transform.localScale);

			return mat;
		}

		/// <summary>
		/// Get the world matrix of the Transform
		/// (a wrapper to localToWorldMatrix)
		/// </summary>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static Matrix4x4 GetWorldMatrix(this Transform transform)
		{
			return transform.localToWorldMatrix;
		}

		/// <summary>
		/// Set the global matrix of the Transform
		/// </summary>
		/// <param name="transf"></param>
		/// <param name="mat"></param>
		public static void SetWorldMatrix(this Transform transform, Matrix4x4 matrix)
		{
			Matrix4x4 parentMatrix = transform.parent == null ? Matrix4x4.identity : transform.parent.localToWorldMatrix;

			transform.SetLocalMatrix(parentMatrix.inverse * matrix);
		}

		/// <summary>
		/// Get the matrix of this transform in another referential
		/// </summary>
		/// <param name="transf"></param>
		/// <param name="referential"></param>
		/// <returns></returns>
		static public Matrix4x4 GetMatrix(this Transform transf, Transform referential)
		{
			return (referential != null ? referential.worldToLocalMatrix : Matrix4x4.identity) * transf.localToWorldMatrix;
		}

		/// <summary>
		/// Set the matrix of this transform in another referential
		/// </summary>
		/// <param name="transf"></param>
		/// <param name="mat"></param>
		/// <param name="referential"></param>
		static public void SetMatrix(this Transform transf, Matrix4x4 mat, Transform referential)
		{
			Matrix4x4 world_matrix = (referential != null ? referential.localToWorldMatrix : Matrix4x4.identity) * mat;

			transf.SetWorldMatrix(world_matrix);
		}

#if UNITY_EDITOR || UNITY_EDITOR_32
		/// <summary>
		/// Add ability to hide children objects to a GameObject contextual menu
		/// (right click on a GO in the Hierarchy view)
		/// </summary>
		[UnityEditor.MenuItem("GameObject/Show-hide hierarchy/Hide children", false, 0)]
		static void HideChildren()
		{
			UnityEditor.Selection.activeTransform.ShowHierarchy(false);
		}

		[UnityEditor.MenuItem("GameObject/Show-hide hierarchy/Hide children", true, 0)]
		static bool ValidateHideChildren()
		{
			return UnityEditor.Selection.activeTransform != null;
		}

		/// <summary>
		/// Add ability to show children objects to a GameObject contextual menu
		/// (right click on a GO in the Hierarchy view)
		/// </summary>
		[UnityEditor.MenuItem("GameObject/Show-hide hierarchy/Show children", false, 0)]
		static void ShowChildren()
		{
			UnityEditor.Selection.activeTransform.ShowHierarchy(true);
		}

		[UnityEditor.MenuItem("GameObject/Show-hide hierarchy/Show children", true, 0)]
		static bool ValidateShowChildren()
		{
			return UnityEditor.Selection.activeTransform != null;
		}

		/// <summary>
		/// Add ability to parent to scene root to a GameObject contextual menu
		/// (right click on a GO in the Hierarchy view)
		/// </summary>
		[UnityEditor.MenuItem("GameObject/Move to root", false, 0)]
		static void MoveToRoot()
		{
			UnityEditor.Selection.activeTransform.parent = null; ;
		}

		[UnityEditor.MenuItem("GameObject/Move to root", true, 0)]
		static bool ValidateMoveToRoot()
		{
			return UnityEditor.Selection.activeTransform != null;
		}
#endif
	}
}
