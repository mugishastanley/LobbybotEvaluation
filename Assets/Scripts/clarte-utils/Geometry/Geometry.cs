using UnityEngine;
using CLARTE.Geometry.Extensions;

namespace CLARTE.Geometry
{
	/// <summary>
	/// Describes a finite line segment
	/// </summary>
	public struct Segment
	{
		/// <summary>
		/// First extremity of the segment
		/// </summary>
		public Vector3 p1;

		/// <summary>
		/// Second extremity of the segment
		/// </summary>
		public Vector3 p2;

		public Segment(Vector3 _p1, Vector3 _p2)
		{
			p1 = _p1;
			p2 = _p2;
		}
	}

	/// <summary>
	/// Describes an infinite line
	/// </summary>
	public struct Line
	{
		/// <summary>
		/// Point the line is passing through
		/// </summary>
		public Vector3 p;

		/// <summary>
		/// Direction of the line
		/// </summary>
		public Vector3 u;

		public Line(Vector3 _p, Vector3 _u)
		{
			p = _p;
			u = _u;
		}

		public Line(Segment s)
		{
			p = s.p1;
			u = s.p2 - s.p1;
		}
	}

	/// <summary>
	/// Describes a triangle
	/// (3 vertices)
	/// </summary>
	public struct Triangle
	{
		/// <summary>
		/// 1st point
		/// </summary>
		public Vector3 a;

		/// <summary>
		/// 2nd point
		/// </summary>
		public Vector3 b;

		/// <summary>
		/// 3rd point
		/// </summary>
		public Vector3 c;

		public Triangle(Vector3 _a, Vector3 _b, Vector3 _c)
		{
			a = _a;
			b = _b;
			c = _c;
		}
	}

	/// <summary>
	/// Describes an infinite plane
	/// </summary>
	public struct Plane
	{
		/// <summary>
		/// Point the plane is passing through
		/// </summary>
		public Vector3 p;

		/// <summary>
		/// Direction of a normal to the plane
		/// </summary>
		public Vector3 n;

		public Plane(Vector3 _p, Vector3 _n)
		{
			p = _p;
			n = _n;
		}

		public Plane(Triangle t)
		{
			p = t.a;

			n = Vector3.Cross(t.b - t.a, t.c - t.a);

			n.Normalize();
		}
	}

	/// <summary>
	/// Describes a sphere
	/// </summary>
	public struct Sphere
	{
		/// <summary>
		/// Center of the sphere
		/// </summary>
		public Vector3 c;

		/// <summary>
		/// Radius of the sphere
		/// </summary>
		public float r;

		public Sphere(Vector3 _c, float _r)
		{
			c = _c;
			r = _r;
		}
	}

	/// <summary>
	/// Describes a capsule
	/// </summary>
	public struct Capsule
	{
		/// <summary>
		/// 1st foyer of the capsule
		/// </summary>
		public Vector3 f1;

		/// <summary>
		/// 2nd foyer of the capsule
		/// </summary>
		public Vector3 f2;

		/// <summary>
		/// Radius of the capsule
		/// </summary>
		public float r;

		public Capsule(Vector3 _f1, Vector3 _f2, float _r)
		{
			f1 = _f1;
			f2 = _f2;
			r = _r;
		}
	}

	/// <summary>
	/// Geometry utilities for Unity
	/// </summary>
	static public class Geometry
	{
		private const float epsilon = 1E-05F;

		#region Intersections
		/// <summary>
		/// Computes the intersection between two 3D lines
		/// </summary>
		/// <param name="line1">1st line </param>
		/// <param name="line2">2nd line</param>
		/// <param name="intersection_point">Intersection point, if it exists</param>
		/// <returns>True if intersection point exists, false otherwise</returns>
		static public bool LineLineIntersection(Line line1, Line line2, out Vector3 intersection_point)
		{
			Vector3 line1_point1 = line1.p;
			Vector3 line1_point2 = line1.p + line1.u;

			Vector3 line2_point1 = line2.p;
			Vector3 line2_point2 = line2.p + line2.u;

			Vector3 v1, v2;

			intersection_point = Vector3.zero;

			Line l1 = new Line(new Segment(line1_point1, line1_point2));
			Line l2 = new Line(new Segment(line2_point1, line2_point2));

			if(LineLineClosestPoints(l1, l2, out v1, out v2))
			{
				if((v2 - v1).sqrMagnitude < epsilon)
				{
					intersection_point = v1;

					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Computes intersection between 2 segments
		/// </summary>
		/// <param name="segment1">1st segment</param>
		/// <param name="segment2">2nd segment</param>
		/// <param name="intersection_point">intersection point</param>
		/// <returns></returns>
		static public bool SegmentSegmentIntersection(Segment segment1, Segment segment2, out Vector3 intersection_point)
		{
			intersection_point = Vector3.zero;

			if(LineLineIntersection(new Line(segment1), new Line(segment2), out intersection_point))
			{
				float s1 = Vector3.Dot(segment1.p1 - intersection_point, segment1.p2 - intersection_point);
				float s2 = Vector3.Dot(segment2.p1 - intersection_point, segment2.p2 - intersection_point);

				if(s1 < 0.0f && s2 < 0.0f)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Compute intersection between a line and a plane
		/// </summary>
		/// <param name="plane">Plane</param>
		/// <param name="line">Line</param>
		/// <param name="intersection_point">Intersection point, if it exists</param>
		/// <returns>True if line intersects plane, false otherwise</returns>
		static public bool PlaneLineIntersection(Plane plane, Line line, out Vector3 intersection_point)
		{
			intersection_point = new Vector3();

			line.u.Normalize();

			float d = -plane.n.x * plane.p.x - plane.n.y * plane.p.y - plane.n.z * plane.p.z;

			float denom = plane.n.x * line.u.x + plane.n.y * line.u.y + plane.n.z * line.u.z;

			if(denom != 0.0f)
			{
				float num = -(plane.n.x * line.p.x + plane.n.y * line.p.y + plane.n.z * line.p.z + d);

				float t = num / denom;

				intersection_point = line.p + line.u * t;

				return true;
			}

			return false;
		}

		/// <summary>
		/// Compute intersection between a plane and a segment
		/// </summary>
		/// <param name="plane">Plane normal</param>
		/// <param name="segment">Segment</param>
		/// <param name="intersection_point">Intersection point, if it exists</param>
		/// <returns>True if segment intersects plane, false otherwise</returns>
		static public bool PlaneSegmentIntersection(Plane plane, Segment segment, out Vector3 intersection_point)
		{
			intersection_point = new Vector3();

			if(Vector3.Dot(plane.n, segment.p1 - plane.p) * Vector3.Dot(plane.n, segment.p2 - plane.p) >= 0)
			{
				return false;
			}

			Line line = new Line(segment.p1, segment.p2 - segment.p1);

			// Compute intersection
			return PlaneLineIntersection(plane, line, out intersection_point);
		}

		/// <summary>
		/// Compute intersection between a triangle and a segment
		/// </summary>
		/// <param name="triangle">Triangle</param>
		/// <param name="segment">Segment</param>
		/// <param name="intersection_point">Intersection point, if it exists</param>
		/// <returns>True if segment intersects triangle, false otherwise</returns>
		static public bool TriangleSegmentIntersection(Triangle triangle, Segment segment, out Vector3 intersection_point)
		{
			Plane p = new Plane(triangle);

			if(PlaneSegmentIntersection(p, segment, out intersection_point))
			{
				if(IsPointInTriangle(triangle, intersection_point))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Compute triangle vs triangle intersection
		/// </summary>
		/// <param name="triangle1">1st triangle</param>
		/// <param name="triangle2">2nd triangle</param>
		/// <returns>true if triangles intersect, false otherwise</returns>
		static public bool TriangleTriangleIntersection(Triangle triangle1, Triangle triangle2)
		{
			Vector3 intersect;

			Plane plane1 = new Plane(triangle1);
			Plane plane2 = new Plane(triangle2);

			Segment s11 = new Segment(triangle1.a, triangle1.b);
			Segment s12 = new Segment(triangle1.b, triangle1.c);
			Segment s13 = new Segment(triangle1.a, triangle1.c);

			Segment s21 = new Segment(triangle2.a, triangle2.b);
			Segment s22 = new Segment(triangle2.b, triangle2.c);
			Segment s23 = new Segment(triangle2.a, triangle2.c);

			if(ArePlanesCoPlanar(plane1, plane2))
			{
				// Intersection occurs as soon as two segments intersect.

				// 1st triangle 1st edge vs 2nd triangle 1st edge
				if(SegmentSegmentIntersection(s11, s21, out intersect))
				{
					return true;
				}

				// 1st triangle 1st edge vs 2nd triangle 2nd edge
				if(SegmentSegmentIntersection(s11, s22, out intersect))
				{
					return true;
				}

				// 1st triangle 1st edge vs 2nd triangle 3rd edge
				if(SegmentSegmentIntersection(s11, s23, out intersect))
				{
					return true;
				}


				// 1st triangle 2nd edge vs 2nd triangle 1st edge
				if(SegmentSegmentIntersection(s12, s21, out intersect))
				{
					return true;
				}

				// 1st triangle 2nd edge vs 2nd triangle 2nd edge
				if(SegmentSegmentIntersection(s12, s22, out intersect))
				{
					return true;
				}

				// 1st triangle 2nd edge vs 2nd triangle 3rd edge
				if(SegmentSegmentIntersection(s12, s23, out intersect))
				{
					return true;
				}


				// 1st triangle 3rd edge vs 2nd triangle 1st edge
				if(SegmentSegmentIntersection(s13, s21, out intersect))
				{
					return true;
				}

				// 1st triangle 3rd edge vs 2nd triangle 2nd edge
				if(SegmentSegmentIntersection(s13, s22, out intersect))
				{
					return true;
				}

				// 1st triangle 3rd edge vs 2nd triangle 3rd edge
				if(SegmentSegmentIntersection(s13, s23, out intersect))
				{
					return true;
				}


				return false;
			}
			else
			{
				// Intersection occurs as soon as we find one edge intersecting one triangle. No need to test for a second edge.

				// 1st triangle vs 2nd triangle 1st edge
				if(TriangleSegmentIntersection(triangle1, s11, out intersect))
				{
					return true;
				}

				// 1st triangle vs 2nd triangle 2nd edge
				if(TriangleSegmentIntersection(triangle1, s12, out intersect))
				{
					return true;
				}

				// 1st triangle vs 2nd triangle 3rd edge
				if(TriangleSegmentIntersection(triangle1, s13, out intersect))
				{
					return true;
				}


				// 2nd triangle vs 1st triangle 1st edge
				if(TriangleSegmentIntersection(triangle2, s21, out intersect))
				{
					return true;
				}

				// 2nd triangle vs 1st triangle 2nd edge
				if(TriangleSegmentIntersection(triangle2, s22, out intersect))
				{
					return true;
				}

				// 2nd triangle vs 1st triangle 3rd edge
				if(TriangleSegmentIntersection(triangle2, s23, out intersect))
				{
					return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Get intersection between a sphere and a line
		/// </summary>
		/// <param name="line">Line</param>
		/// <param name="sphere">Sphere</param>
		/// <param name="intersection_point1">1st intersection</param>
		/// <param name="intersection_point2">2nd intersection</param>
		/// <returns>True if intersection exists, false otherwise</returns>
		static public bool SphereLineIntersection(Line line, Sphere sphere, out Vector3 intersection_point1, out Vector3 intersection_point2)
		{
			intersection_point1 = new Vector3();
			intersection_point2 = new Vector3();

			Vector3 p1 = line.p;
			Vector3 p2 = line.p + line.u;

			float a = (p2.x - p1.x) * (p2.x - p1.x) + (p2.y - p1.y) * (p2.y - p1.y) + (p2.z - p1.z) * (p2.z - p1.z);
			float b = 2 * ((p2.x - p1.x) * (p1.x - sphere.c.x) + (p2.y - p1.y) * (p1.y - sphere.c.y) + (p2.z - p1.z) * (p1.z - sphere.c.z));
			float c = sphere.c.x * sphere.c.x + sphere.c.y * sphere.c.y + sphere.c.z * sphere.c.z + p1.x * p1.x + p1.y * p1.y + p1.z * p1.z - 2 * (sphere.c.x * p1.x + sphere.c.y * p1.y + sphere.c.z * p1.z) - sphere.r * sphere.r;

			float delta = b * b - 4 * a * c;

			if(delta < 0.0f)
			{
				return false;
			}

			float u1 = (-b - (float)Mathf.Sqrt((float)delta)) / (2 * a);
			float u2 = (-b + (float)Mathf.Sqrt((float)delta)) / (2 * a);


			intersection_point1 = p1 + u1 * (p2 - p1);
			intersection_point2 = p1 + u2 * (p2 - p1);


			return true;
		}
		
		static public Capsule CapsuleCollider2Capsule(CapsuleCollider collider)
        {
			Vector3 capsule_direction_local;
			float capsule_scale_1;
			float capsule_scale_2;

			switch(collider.direction)
			{
				case 0:
					capsule_direction_local = Vector3.right;
					capsule_scale_1 = collider.gameObject.transform.lossyScale.y;
					capsule_scale_2 = collider.gameObject.transform.lossyScale.z;
					break;
				case 1:
					capsule_direction_local = Vector3.up;
					capsule_scale_1 = collider.gameObject.transform.lossyScale.x;
					capsule_scale_2 = collider.gameObject.transform.lossyScale.z;
					break;
				case 2:
					capsule_direction_local = Vector3.forward;
					capsule_scale_1 = collider.gameObject.transform.lossyScale.x;
					capsule_scale_2 = collider.gameObject.transform.lossyScale.y;
					break;
				default:					
					Debug.LogError("Direction id should be 0, 1 or 2");
					return default(Capsule);
			}

			float radius = collider.radius * Mathf.Max(capsule_scale_1, capsule_scale_2);

			Vector3 capsule_ext1_world = collider.gameObject.transform.TransformPoint(collider.center + 0.5f * capsule_direction_local * collider.height);
			Vector3 capsule_ext2_world = collider.gameObject.transform.TransformPoint(collider.center - 0.5f * capsule_direction_local * collider.height);

			Vector3 capsule_direction_world = collider.gameObject.transform.TransformDirection(capsule_direction_local);

			Vector3 capsule_f1_world = capsule_ext1_world - capsule_direction_world * radius;
			Vector3 capsule_f2_world = capsule_ext2_world + capsule_direction_world * radius;

			return new Capsule(capsule_f1_world, capsule_f2_world, radius);
		}

		/// <summary>
		/// Checks whether two capsules are intersecting.
		/// Two capsules intersect <=> radius 1 + radius 2 >= dist(axis 1, axis2)
		/// </summary>
		/// <param name="capsule1">1st capsule</param>
		/// <param name="capsule2">2nd capsule</param>
		/// <returns>True if capsules intersect, false otherwise</returns>
		static public bool CapsuleCapsuleIntersection(CapsuleCollider capsule1, CapsuleCollider capsule2)
        {
			return CapsuleCapsuleIntersection(CapsuleCollider2Capsule(capsule1), CapsuleCollider2Capsule(capsule2));
		}

		/// <summary>
		/// Checks whether two capsules are intersecting.
		/// Two capsules intersect <=> radius 1 + radius 2 >= dist(axis 1, axis2)
		/// </summary>
		/// <param name="capsule1">1st capsule</param>
		/// <param name="capsule2">2nd capsule</param>
		/// <returns>True if capsules intersect, false otherwise</returns>
		static public bool CapsuleCapsuleIntersection(Capsule capsule1, Capsule capsule2)
		{
			float dist = CapsuleCapsuleDistance(capsule1, capsule2);

			// If the distance is negative or nil, they collide
			return dist <= 0;
		}

		/// <summary>
		/// Computes the intersection between a box and a segment
		/// </summary>
		/// <param name="box">Box</param>
		/// <param name="segment">Segment</param>
		/// <returns></returns>
		static public bool BoxSegmentIntersection(BoxCollider box, Segment segment)
		{
			Vector3 p1_box = box.transform.InverseTransformPoint(segment.p1);

			Vector3 p2_box = box.transform.InverseTransformPoint(segment.p2);

			Segment segment_box = new Segment(p1_box, p2_box);

			Bounds bounds = new Bounds(box.center, box.size);

			return BoxSegmentIntersection(bounds, segment_box);
		}

		/// <summary>
		/// Computes intersection between a segment and a box.
		/// </summary>
		/// <param name="box">Box</param>
		/// <param name="segment">Segment (coords in box referential)</param>
		/// <returns>True if segment intersects box, false otherwise</returns>
		static public bool BoxSegmentIntersection(Bounds box, Segment segment)
		{
			Vector3 segment_dir = segment.p2 - segment.p1;

			Vector3 inv_segment_dir = new Vector3();

			inv_segment_dir.x = 1.0f / segment_dir.x;
			inv_segment_dir.y = 1.0f / segment_dir.y;
			inv_segment_dir.z = 1.0f / segment_dir.z;

			Vector3 t0 = new Vector3();
			t0.x = (box.min - segment.p1).x * inv_segment_dir.x;
			t0.y = (box.min - segment.p1).y * inv_segment_dir.y;
			t0.z = (box.min - segment.p1).z * inv_segment_dir.z;

			Vector3 t1 = new Vector3();
			t1.x = (box.max - segment.p1).x * inv_segment_dir.x;
			t1.y = (box.max - segment.p1).y * inv_segment_dir.y;
			t1.z = (box.max - segment.p1).z * inv_segment_dir.z;

			Vector3 tmin = new Vector3();
			tmin.x = Mathf.Min(t0.x, t1.x);
			tmin.y = Mathf.Min(t0.y, t1.y);
			tmin.z = Mathf.Min(t0.z, t1.z);

			Vector3 tmax = new Vector3();
			tmax.x = Mathf.Max(t0.x, t1.x);
			tmax.y = Mathf.Max(t0.y, t1.y);
			tmax.z = Mathf.Max(t0.z, t1.z);

			float max_of_mins = Mathf.Max(Mathf.Max(tmin.x, tmin.y), tmin.z);

			float min_of_maxs = Mathf.Min(Mathf.Min(tmax.x, tmax.y), tmax.z);

			bool line_intersect = max_of_mins <= min_of_maxs;

			bool max_of_mins_is_between_0_and_1 = max_of_mins > 0.0f && max_of_mins < 1.0f;
			bool min_of_maxs_is_between_0_and_1 = min_of_maxs > 0.0f && min_of_maxs < 1.0f;

			return line_intersect && (max_of_mins_is_between_0_and_1 || min_of_maxs_is_between_0_and_1);
		}

		/// <summary>
		/// Computes box - plane intersection
		/// Box centered on origin
		/// </summary>
		/// <param name=plane">Plane</param>
		/// <param name="box_extent">Half-size of the box</param>
		/// <returns></returns>
		static private bool BoxPlaneIntersection(Plane plane, Vector3 box_extent)
		{
			Vector3 vmin = new Vector3();
			Vector3 vmax = new Vector3();

			if(plane.n.x > 0.0f)
			{
				vmin.x = -box_extent.x - plane.p.x;

				vmax.x = box_extent.x - plane.p.x;
			}
			else
			{
				vmin.x = box_extent.x - plane.p.x;

				vmax.x = -box_extent.x - plane.p.x;
			}

			if(plane.n.y > 0.0f)
			{
				vmin.y = -box_extent.y - plane.p.y;

				vmax.y = box_extent.y - plane.p.y;
			}
			else
			{
				vmin.y = box_extent.y - plane.p.y;

				vmax.y = -box_extent.y - plane.p.y;
			}

			if(plane.n.z > 0.0f)
			{
				vmin.z = -box_extent.z - plane.p.z;

				vmax.z = box_extent.z - plane.p.z;
			}
			else
			{
				vmin.z = box_extent.z - plane.p.z;

				vmax.z = -box_extent.z - plane.p.z;
			}


			if(Vector3.Dot(plane.n, vmin) > 0.0f)
			{
				return false;
			}

			if(Vector3.Dot(plane.n, vmax) >= 0.0f)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Computes box - triangle intersection
		///
		/// AABB-triangle overlap test code
		/// by Tomas Akenine-Möller
		///
		/// History:
		///   2001-03-05: released the code in its first version
		///   2001-06-18: changed the order of the tests, faster
		///
		/// Acknowledgement: Many thanks to Pierre Terdiman for
		/// suggestions and discussions on how to optimize code.
		/// Thanks to David Hunt for finding a ">="-bug!  
		/// </summary>
		/// <param name="box">Box</param>
		/// <param name="triangle">Triangle (coords in box referential)</param>
		/// <returns>True if triangle intersects box, false otherwise</returns>
		static public bool BoxTriangleIntersection(Bounds box, Triangle triangle)
		{
			// Use separating axis theorem to test overlap between triangle and box
			// need to test for overlap in these directions:
			//    1) the {x,y,z}-directions (actually, since we use the AABB of the triangle
			//       we do not even need to test these)
			//    2) normal of the triangle
			//    3) crossproduct(edge from tri, {x,y,z}-direction)
			//       this gives 3x3=9 more tests

			Vector3 v0, v1, v2;

			float min, max, p0, p1, p2, rad, fex, fey, fez;

			Vector3 normal, e0, e1, e2;

			// This is the fastest branch on Sun
			// move everything so that the boxcenter is in (0,0,0)

			v0 = triangle.a - box.center;

			v1 = triangle.b - box.center;

			v2 = triangle.c - box.center;

			// compute triangle edges

			e0 = v1 - v0;      // tri edge 0

			e1 = v2 - v1;      // tri edge 1

			e2 = v0 - v2;      // tri edge 2

			// Bullet 3:
			//  test the 9 tests first (this was faster)

			fex = Mathf.Abs(e0.x);

			fey = Mathf.Abs(e0.y);

			fez = Mathf.Abs(e0.z);

			p0 = e0.z * v0.y - e0.y * v0.z;

			p2 = e0.z * v2.y - e0.y * v2.z;

			if(p0 < p2)
			{
				min = p0;
				max = p2;
			}
			else
			{
				min = p2;
				max = p0;
			}

			rad = fez * box.extents.y + fey * box.extents.z;

			if(min > rad || max < -rad)
			{
				return false;
			}

			p0 = -e0.z * v0.x + e0.x * v0.z;

			p2 = -e0.z * v2.x + e0.x * v2.z;

			if(p0 < p2)
			{
				min = p0;
				max = p2;
			}
			else
			{
				min = p2;
				max = p0;
			}

			rad = fez * box.extents.x + fex * box.extents.z;

			if(min > rad || max < -rad)
			{
				return false;
			}

			p1 = e0.y * v1.x - e0.x * v1.y;

			p2 = e0.y * v2.x - e0.x * v2.y;

			if(p2 < p1)
			{
				min = p2;
				max = p1;
			}
			else
			{
				min = p1;
				max = p2;
			}

			rad = fey * box.extents.x + fex * box.extents.y;

			if(min > rad || max < -rad)
			{
				return false;
			}

			fex = Mathf.Abs(e1.x);

			fey = Mathf.Abs(e1.y);

			fez = Mathf.Abs(e1.z);

			p0 = e1.z * v0.y - e1.y * v0.z;

			p2 = e1.z * v2.y - e1.y * v2.z;

			if(p0 < p2)
			{
				min = p0;
				max = p2;
			}
			else
			{
				min = p2;
				max = p0;
			}

			rad = fez * box.extents.y + fey * box.extents.z;

			if(min > rad || max < -rad)
			{
				return false;
			}

			p0 = -e1.z * v0.x + e1.x * v0.z;

			p2 = -e1.z * v2.x + e1.x * v2.z;

			if(p0 < p2)
			{
				min = p0;
				max = p2;
			}
			else
			{
				min = p2;
				max = p0;
			}

			rad = fez * box.extents.x + fex * box.extents.z;

			if(min > rad || max < -rad)
			{
				return false;
			}

			p0 = e1.y * v0.x - e1.x * v0.y;

			p1 = e1.y * v1.x - e1.x * v1.y;

			if(p0 < p1)
			{
				min = p0;
				max = p1;
			}
			else
			{
				min = p1;
				max = p0;
			}

			rad = fey * box.extents.x + fex * box.extents.y;

			if(min > rad || max < -rad)
			{
				return false;
			}

			fex = Mathf.Abs(e2.x);

			fey = Mathf.Abs(e2.y);

			fez = Mathf.Abs(e2.z);

			p0 = e2.z * v0.y - e2.y * v0.z;

			p1 = e2.z * v1.y - e2.y * v1.z;

			if(p0 < p1)
			{
				min = p0;
				max = p1;
			}
			else
			{
				min = p1;
				max = p0;
			}

			rad = fez * box.extents.y + fey * box.extents.z;

			if(min > rad || max < -rad)
			{
				return false;
			}

			p0 = -e2.z * v0.x + e2.x * v0.z;

			p1 = -e2.z * v1.x + e2.x * v1.z;

			if(p0 < p1)
			{
				min = p0;
				max = p1;
			}
			else
			{
				min = p1;
				max = p0;
			}

			rad = fez * box.extents.x + fex * box.extents.z;

			if(min > rad || max < -rad)
			{
				return false;
			}

			p1 = e2.y * v1.x - e2.x * v1.y;

			p2 = e2.y * v2.x - e2.x * v2.y;

			if(p2 < p1)
			{
				min = p2;
				max = p1;
			}
			else
			{
				min = p1; max = p2;
			}

			rad = fey * box.extents.x + fex * box.extents.y;

			if(min > rad || max < -rad) return false;


			// Bullet 1:
			//  first test overlap in the {x,y,z}-directions
			//  find min, max of the triangle each direction, and test for overlap in
			//  that direction -- this is equivalent to testing a minimal AABB around
			//  the triangle against the AABB

			// test in X-direction

			min = Mathf.Min(v0.x, Mathf.Min(v1.x, v2.x));
			max = Mathf.Max(v0.x, Mathf.Max(v1.x, v2.x));

			if(min > box.extents.x || max < -box.extents.x)
			{
				return false;
			}

			// test in Y-direction

			min = Mathf.Min(v0.y, Mathf.Min(v1.y, v2.y));
			max = Mathf.Max(v0.y, Mathf.Max(v1.y, v2.y));

			if(min > box.extents.y || max < -box.extents.y)
			{
				return false;
			}

			// test in Z-direction

			min = Mathf.Min(v0.z, Mathf.Min(v1.z, v2.z));
			max = Mathf.Max(v0.z, Mathf.Max(v1.z, v2.z));

			if(min > box.extents.z || max < -box.extents.z)
			{
				return false;
			}

			// Bullet 2:
			//  test if the box intersects the plane of the triangle
			//  compute plane equation of triangle: normal*x+d=0

			normal = Vector3.Cross(e0, e1);

			Plane plane = new Plane(v0, normal);

			if(!BoxPlaneIntersection(plane, box.extents))
			{
				return false;
			}

			return true;   // box and triangle overlaps
		}

		/// <summary>
		/// Computes box-box intersection using the Separating Axis Test
		/// </summary>
		/// <param name="box1">The first box.</param>
		/// <param name="box2">The second box.</param>
		/// <returns>True if the two boxes are colliding, false otherwise.</returns>
		static public bool BoxBoxIntersection(BoxCollider box1, BoxCollider box2)
		{
			Transform b1_transform = box1.transform;
			Transform b2_transform = box2.transform;

			Vector3 b1_axis_x = b1_transform.right;
			Vector3 b1_axis_y = b1_transform.up;
			Vector3 b1_axis_z = b1_transform.forward;
			Vector3 b2_axis_x = b2_transform.right;
			Vector3 b2_axis_y = b2_transform.up;
			Vector3 b2_axis_z = b2_transform.forward;

			// Get all the 15 test axes to be used for Box-Box collision test using SAT
			Vector3[] axes = new Vector3[]
			{
				b1_axis_x,
				b1_axis_y,
				b1_axis_z,
				b2_axis_x,
				b2_axis_y,
				b2_axis_z,
				Vector3.Cross(b1_axis_x, b2_axis_x),
				Vector3.Cross(b1_axis_x, b2_axis_y),
				Vector3.Cross(b1_axis_x, b2_axis_z),
				Vector3.Cross(b1_axis_y, b2_axis_x),
				Vector3.Cross(b1_axis_y, b2_axis_y),
				Vector3.Cross(b1_axis_y, b2_axis_z),
				Vector3.Cross(b1_axis_z, b2_axis_x),
				Vector3.Cross(b1_axis_z, b2_axis_y),
				Vector3.Cross(b1_axis_z, b2_axis_z)
			};

			// Get the corners of each box
			Vector3[] b1_corners = box1.GetCorners(Space.World);
			Vector3[] b2_corners = box2.GetCorners(Space.World);

			int nb_axes = axes.Length;
			int nb_corners = b1_corners.Length;

			// Test overlap on each axis
			for(int i = 0; i < nb_axes; i++)
			{
				Vector3 axis = axes[i];

				// Cross product = (0, 0, 0) => colinear base vectors
				// i.e. box aligned on some axis: we can safely skip the test on this degenerated axis
				if(axis != Vector3.zero)
				{
					float b1_proj_min = float.MaxValue;
					float b1_proj_max = float.MinValue;
					float b2_proj_min = float.MaxValue;
					float b2_proj_max = float.MinValue;

					// Get min and max value of projected corners onto current axis
					for(int j = 0; j < nb_corners; j++)
					{
						float b1_proj = Vector3.Dot(b1_corners[j], axis);
						float b2_proj = Vector3.Dot(b2_corners[j], axis);

						if(b1_proj < b1_proj_min)
						{
							b1_proj_min = b1_proj;
						}

						if(b1_proj > b1_proj_max)
						{
							b1_proj_max = b1_proj;
						}

						if(b2_proj < b2_proj_min)
						{
							b2_proj_min = b2_proj;
						}

						if(b2_proj > b2_proj_max)
						{
							b2_proj_max = b2_proj;
						}
					}

					bool overlap = false;

					// Test if projections are overlaping on the current axis
					if(b1_proj_min == b1_proj_max)
					{
						overlap = true;
					}
					else if(b1_proj_min < b2_proj_min)
					{
						if(b1_proj_max >= b2_proj_min)
						{
							overlap = true;
						}
					}
					else
					{
						if(b2_proj_max >= b1_proj_min)
						{
							overlap = true;
						}
					}

					// No collision if boxes does not overlap on at least one axis
					if(!overlap)
					{
						return false;
					}
				}
			}

			// All tested axes overlap, therefore the boxes collide
			return true;
		}
		#endregion

		#region Closest points
		/// <summary>
		/// Computes shortest segment between two lines
		/// </summary>
		/// <param name="line1">1st line</param>
		/// <param name="line2">2nd line</param>
		/// <param name="closest_point1">Closest point on line 1</param>
		/// <param name="closest_point2">Closest point on line 2</param>
		/// <returns>True if segment exists, false otherwise</returns>
		static public bool LineLineClosestPoints(Line line1, Line line2, out Vector3 closest_point1, out Vector3 closest_point2)
		{
			// Algorithm is ported from the C algorithm of 
			// Paul Bourke at http://local.wasp.uwa.edu.au/~pbourke/geometry/lineline3d/
			closest_point1 = Vector3.zero;
			closest_point2 = Vector3.zero;

			Vector3 p1 = line1.p;
			Vector3 p2 = line1.p + line1.u;
			Vector3 p3 = line2.p;
			Vector3 p4 = line2.p + line2.u;
			Vector3 p13 = p1 - p3;
			Vector3 p43 = p4 - p3;

			if(p43.sqrMagnitude < epsilon)
			{
				return false;
			}

			Vector3 p21 = p2 - p1;

			if(p21.sqrMagnitude < epsilon)
			{
				return false;
			}

			float d1343 = p13.x * p43.x + p13.y * p43.y + p13.z * p43.z;
			float d4321 = p43.x * p21.x + p43.y * p21.y + p43.z * p21.z;
			float d1321 = p13.x * p21.x + p13.y * p21.y + p13.z * p21.z;
			float d4343 = p43.x * p43.x + p43.y * p43.y + p43.z * p43.z;
			float d2121 = p21.x * p21.x + p21.y * p21.y + p21.z * p21.z;

			float denom = d2121 * d4343 - d4321 * d4321;

			if(Mathf.Abs(denom) < epsilon)
			{
				return false;
			}

			float numer = d1343 * d4321 - d1321 * d4343;

			float mua = numer / denom;
			float mub = (d1343 + d4321 * (mua)) / d4343;

			closest_point1.x = (p1.x + mua * p21.x);
			closest_point1.y = (p1.y + mua * p21.y);
			closest_point1.z = (p1.z + mua * p21.z);
			closest_point2.x = (p3.x + mub * p43.x);
			closest_point2.y = (p3.y + mub * p43.y);
			closest_point2.z = (p3.z + mub * p43.z);

			return true;
		}

		/// <summary>
		/// Computes closest points belonging to two segments
		/// </summary>
		/// <param name="segment1">Segment 1</param>
		/// <param name="segment2">Segment 2</param>
		/// <param name="closest_point1">Closest point on segment 1</param>
		/// <param name="closest_point2">Closest point on segment 2</param>
		/// <returns>True</returns>
		static public bool SegmentSegmentClosestPoints(Segment segment1, Segment segment2, out Vector3 closest_point1, out Vector3 closest_point2)
		{
			float s, t;

			Vector3 d1 = segment1.p2 - segment1.p1; // Direction vector of segment S1
			Vector3 d2 = segment2.p2 - segment2.p1; // Direction vector of segment S2
			Vector3 r = segment1.p1 - segment2.p1;
			float a = Vector3.Dot(d1, d1); // Squared length of segment S1, always nonnegative
			float e = Vector3.Dot(d2, d2); // Squared length of segment S2, always nonnegative
			float f = Vector3.Dot(d2, r);

			// Check if either or both segments degenerate into points
			if(a <= epsilon && e <= epsilon)
			{
				// Both segments degenerate into points
				s = t = 0.0f;
				closest_point1 = segment1.p1;
				closest_point2 = segment2.p1;
				return true;
			}
			if(a <= epsilon)
			{
				// First segment degenerates into a point
				s = 0.0f;
				t = f / e; //s=0=>t=(b*s+f)/e=f/e
				t = Mathf.Clamp01(t);
			}
			else
			{
				float c = Vector3.Dot(d1, r);
				if(e <= epsilon)
				{
					// Second segment degenerates into a point
					t = 0.0f;
					s = Mathf.Clamp01(-c / a); //t=0=>s=(b*t-c)/a=-c/a
				}
				else
				{
					// The general nondegenerate case starts here
					float b = Vector3.Dot(d1, d2);
					float denom = a * e - b * b; // Always nonnegative

					// If segments not parallel, compute closest point on L1 to L2 and
					// clamp to segment S1. Else pick arbitrary s (here 0)
					if(denom != 0.0f)
					{
						s = Mathf.Clamp01((b * f - c * e) / denom);
					}
					else
					{
						s = 0.0f;
					}

					// Compute point on L2 closest to S1(s) using
					// t = Dot((P1 + D1*s) - P2,D2) / Dot(D2,D2) = (b*s + f) / e
					t = (b * s + f) / e;

					// If t in [0,1] done. Else clamp t, recompute s for the new value
					// of t using s = Dot((P2 + D2*t) - P1,D1) / Dot(D1,D1)= (t*b - c) / a
					// and clamp s to [0, 1]
					if(t < 0.0f)
					{
						t = 0.0f;
						s = Mathf.Clamp01(-c / a);
					}
					else if(t > 1.0f)
					{
						t = 1.0f;
						s = Mathf.Clamp01((b - c) / a);
					}
				}
			}

			closest_point1 = segment1.p1 + d1 * s;
			closest_point2 = segment2.p1 + d2 * t;

			return true;
		}
		#endregion

		#region Distances
		/// <summary>
		/// Computes the distance between 2 capsules
		/// </summary>
		/// <param name="capsule1">1st capsule</param>
		/// <param name="capsule2">2nd capsule</param>
		/// <returns>Distance between the surfaces of the capsules</returns>
		static public float CapsuleCapsuleDistance(CapsuleCollider capsule1, CapsuleCollider capsule2)
        {
			return CapsuleCapsuleDistance(CapsuleCollider2Capsule(capsule1), CapsuleCollider2Capsule(capsule2));
        }

		/// <summary>
		/// Computes the distance between 2 capsules
		/// </summary>
		/// <param name="capsule1">1st capsule</param>
		/// <param name="capsule2">2nd capsule</param>
		/// <returns>Distance between the surfaces of the capsules</returns>
		static public float CapsuleCapsuleDistance(Capsule capsule1, Capsule capsule2)
		{
			Vector3 c1, c2;

			Segment s1 = new Segment(capsule1.f1, capsule1.f2);
			Segment s2 = new Segment(capsule2.f1, capsule2.f2);

			SegmentSegmentClosestPoints(s1, s2, out c1, out c2);

			return Mathf.Max(0.0f, Vector3.Magnitude(c2 - c1) - capsule1.r - capsule2.r);
		}
		#endregion

		#region Projections
		/// <summary>
		/// Computes orthogonal projection of point C on line (AB)
		/// </summary>
		/// <param name="line">Line</param>
		/// <param name="point">Point to be projected</param>
		/// <param name="projection">Projection of point on line</param>
		/// <returns>True</returns>
		static public bool ProjectPointOnLine(Line line, Vector3 point, out Vector3 projection)
		{
			Vector3 ab = line.u;
			Vector3 ac = point - line.p;
			Vector3 dir_ab = ab;
			dir_ab.Normalize();

			double ah = Vector3.Dot(ab, ac) / ab.magnitude;

			projection = line.p + dir_ab * (float)ah;

			return true;
		}

		/// <summary>
		/// Computes orthogonal projection of point on segment
		/// </summary>
		/// <param name="segment">Segment</param>
		/// <param name="point">Point to be projected</param>
		/// <param name="projection">Projection of point on segment</param>
		/// <returns>True if projection exists, null otherwise</returns>
		static public bool ProjectPointOnSegment(Segment segment, Vector3 point, out Vector3 projection)
		{
			Line l = new Line(segment);

			ProjectPointOnLine(l, point, out projection);

			Vector3 dist = l.u;

			float max = 0.0f;
			int idx = -1;

			for(int i = 0; i < 3; i++)
			{
				float abs = Mathf.Abs(dist[i]);

				if(abs > max)
				{
					max = abs;
					idx = i;
				}
			}

			if(idx >= 0 && Mathf.Abs(dist[idx]) > epsilon)
			{
				float k = (projection[idx] - segment.p1[idx]) / dist[idx];

				if(k >= 0 && k <= 1)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Computes orthogonal projection of a 3D point on a plane
		/// </summary>
		/// <param name="plane">Plane</param>
		/// <param name="point">Point to be projected</param>
		/// <param name="projection">Projection of point on plane</param>
		/// <returns>True</returns>
		static public bool ProjectPointOnPlane(Plane plane, Vector3 point, out Vector3 projection)
		{
			plane.n.Normalize();

			projection = point - Vector3.Dot(point - plane.p, plane.n) * plane.n;

			return true;
		}
		#endregion

		#region Misc
		/// <summary>
		/// Checks whether a point lies inside a triangle
		/// (point is supposed to lie in the triangle plane)
		/// </summary>
		/// <param name="t">Triangle</param>
		/// <param name="point_to_test">Position of point to be tested</param>
		/// <returns>True if point lies inside triangle, false otherwise</returns>
		static public bool IsPointInTriangle(Triangle t, Vector3 point_to_test)
		{
			Vector3 u = t.a - point_to_test;
			Vector3 v = t.b - point_to_test;
			Vector3 w = t.c - point_to_test;

			float sum = Vector3.Angle(u, v) + Vector3.Angle(v, w) + Vector3.Angle(w, u);

			return Mathf.Abs(sum - 360.0f) < 100.0f * epsilon;  // avoid floating point accuracy issue
		}

		/// <summary>
		/// Checks whether two planes are co-planar (i.e. the same plane)
		/// </summary>
		/// <param name="plane1">1st plane</param>
		/// <param name="plane2">2nd plane</param>
		/// <returns>True if planes are co-planar, false otherwise</returns>
		static public bool ArePlanesCoPlanar(Plane plane1, Plane plane2)
		{
			Vector3 cross = Vector3.Cross(plane1.n, plane2.n);

			if(cross.sqrMagnitude < epsilon)
			{
				// normals are colinear, so planes are parallel or coplanar

				// p1 and p2 belong to the same plane if p1p2 is orthogonal to either n1 or n2
				// thus both planes are coplanar
				if(Mathf.Abs(Vector3.Dot(plane2.p - plane1.p, plane1.n)) < epsilon)
				{
					return true;
				}
			}

			return false;
		}
		#endregion
	}
}