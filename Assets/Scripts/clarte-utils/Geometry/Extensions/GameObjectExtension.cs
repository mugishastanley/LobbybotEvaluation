using UnityEngine;

namespace CLARTE.Geometry.Extensions
{
	public static class GameObjectExtension
	{
		/// <summary>
		/// Return the component of Type T if the game object has one attached, or add it and return it otherwise
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="go"></param>
		/// <returns></returns>
		public static T GetOrAddComponent<T>(this GameObject go) where T : Component
		{
			return (T)go.GetOrAddComponent(typeof(T));
		}

		/// <summary>
		/// Return the component of Type type if the game object has one attached, or add it and return it otherwise
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="go"></param>
		/// <returns></returns>
		public static Component GetOrAddComponent(this GameObject go, System.Type type)
		{
			Component result = go.GetComponent(type);

			if(result == null)
			{
				result = go.AddComponent(type);
			}

			return result;
		}

		/// <summary>
		/// Get the size of a GameObject
		/// </summary>
		/// <param name="go"></param>
		/// <returns></returns>
		public static Vector3 GetSize(this GameObject go)
		{
			MeshFilter mesh_filter = go.GetComponent<MeshFilter>();

			if(!mesh_filter)
			{
				return Vector3.zero;
			}

			Mesh mesh = mesh_filter.sharedMesh;

			if(!mesh)
			{
				return Vector3.zero;
			}

			Vector3 object_size = mesh.bounds.size;

			object_size.Scale(go.transform.lossyScale);

			return object_size;
		}

		/// <summary>
		/// Change pivot point of a mesh
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="newReferential">Location of the new pivot point</param>
		public static void ChangeReferential(this GameObject go, Transform new_referential)
		{
			Matrix4x4 matrix;

			if(new_referential)
			{
				matrix = new_referential.localToWorldMatrix;
			}
			else
			{
				matrix = Matrix4x4.identity;
			}

			MeshFilter mesh_filter = go.GetComponent<MeshFilter>();

			if(mesh_filter)
			{
				Mesh mesh = mesh_filter.mesh;

				if(mesh)
				{
					Vector3[] vertices = mesh.vertices;
					Vector3[] normals = mesh.normals;
					Vector4[] tangents = mesh.tangents;

					for(int i = 0; i < vertices.Length; i++)
					{
						vertices[i] = go.transform.TransformPoint(vertices[i]);

						if(new_referential)
						{
							vertices[i] = new_referential.InverseTransformPoint(vertices[i]);
						}
					}

					for(int i = 0; i < normals.Length; i++)
					{
						normals[i] = go.transform.TransformDirection(normals[i]);

						if(new_referential)
						{
							normals[i] = new_referential.InverseTransformDirection(normals[i]);
						}
					}

					for(int i = 0; i < tangents.Length; i++)
					{
						Vector3 tangent_l = new Vector3(tangents[i].x, tangents[i].y, tangents[i].z);
						Vector3 tangent_w = go.transform.TransformDirection(tangent_l);

						tangents[i] = new Vector4(tangent_w.x, tangent_w.y, tangent_w.z, tangents[i].w);

						if(new_referential)
						{
							tangent_w = new Vector3(tangents[i].x, tangents[i].y, tangents[i].z);
							tangent_l = go.transform.InverseTransformDirection(tangent_l);

							tangents[i] = new Vector4(tangent_l.x, tangent_l.y, tangent_l.z, tangents[i].w);
						}
					}

					mesh.vertices = vertices;
					mesh.normals = normals;
					mesh.tangents = tangents;

					go.transform.SetWorldMatrix(matrix);

					mesh.RecalculateBounds();

					Collider collider = go.GetComponent<Collider>();

					if(collider)
					{
						System.Type type = collider.GetType();

						Object.Destroy(collider);

						go.AddComponent(type);
					}
				}
				else
				{
					Debug.LogErrorFormat("ChangeReferential failed: no mesh assigned to '{0}' MeshFilter", go.name);
				}
			}
			else
			{
				Debug.LogErrorFormat("ChangeReferential failed: MeshFilter component not found on '{0}'", go.name);
			}
		}
	}
}
