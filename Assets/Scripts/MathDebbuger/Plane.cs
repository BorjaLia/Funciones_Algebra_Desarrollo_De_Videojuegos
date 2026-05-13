using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;


namespace CustomMath
{
    [System.Serializable]
    public struct MyPlane : IEquatable<MyPlane> /*, IFormattable*/
    {

        #region Variables
        public Vec3 normal;
        public float distance;

        public readonly MyPlane flipped { get { return new MyPlane(-normal, -distance); } }

        #endregion

        #region constants
        public const float epsilon = 1e-05f;
        #endregion

        #region Default Values
       // public static Vec3 Zero { get { return new Vec3(0.0f, 0.0f, 0.0f); } }

        #endregion                                                                                                                                                                               

        #region Constructors
        public MyPlane(Vec3 inNormal,Vec3 inPoint)
        {
            this.normal = inNormal.normalized;
            this.distance = -Vec3.Dot(inNormal, inPoint);
        }

        public MyPlane(Vec3 inNormal, float d)
        {
            this.normal = inNormal.normalized;
            this.distance = d;
        }

        public MyPlane(Vec3 a, Vec3 b, Vec3 c)
        {
            //this.normal = Vec3.Cross(a - c, b - c).normalized;
            this.normal = Vec3.Cross(b - a, c - a).normalized;
            this.distance = -Vec3.Dot(this.normal, a);
        }

        #endregion

        #region Operators
        public static bool operator ==(MyPlane lhs, MyPlane rhs)
        {
            return (lhs.normal == rhs.normal) && (lhs.distance == rhs.distance);
        }
        public static bool operator !=(MyPlane lhs, MyPlane rhs)
        {
            return (lhs.normal != rhs.normal) || (lhs.distance != rhs.distance);
        }

        #endregion

        #region Functions

        public static MyPlane Translate(MyPlane plane, Vec3 translation)
        {
            return new MyPlane(plane.normal,plane.distance - Vec3.Dot(plane.normal, translation));
        }

        public void Translate(Vec3 translation)
        {
            //distance = Vec3.Project(translation, this.normal).magnitude;
            this.distance -= Vec3.Dot(this.normal, translation);
        }

        public Vec3 ClosestPointOnPlane(Vec3 point) 
        {
            //return Vec3.Project(point,Vec3.Cross(this.normal,Vec3.Cross(normal,point))) + (normal*distance);
            float distToPlane = GetDistanceToPoint(point);
            return point - (this.normal * distToPlane);
        }

       public void Flip()
        {
            this.normal = -this.normal;
            this.distance = -this.distance;
        }

        public float GetDistanceToPoint(Vec3 point)
        {
            return Vec3.Dot(this.normal, point) + this.distance;
        }
        public bool GetSide(Vec3 point)
        {
            //return (Vec3.Dot(this.normal*this.distance,point) > 0.0f);
            return (GetDistanceToPoint(point) > 0.0f);
        }
        public bool SameSide(Vec3 inPt0, Vec3 inPt1)
        {
            return (GetSide(inPt0) == GetSide(inPt1));
        }
        public void Set3Points(Vec3 a, Vec3 b, Vec3 c)
        {
            //this.normal = Vec3.Cross(a - c, b - c).normalized;
            //this.distance = Vec3.Project(a, this.normal).magnitude;

            this.normal = Vec3.Cross(b - a, c - a).normalized;
            this.distance = -Vec3.Dot(this.normal, a);
        }
        public void SetNormalAndPosition(Vec3 inNormal, Vec3 inPoint) 
        {
            //this.normal = inNormal;
            //this.distance = Vec3.Project(inPoint, inNormal).magnitude;

            this.normal = inNormal.normalized;
            this.distance = -Vec3.Dot(inNormal, inPoint);
        }


        #endregion

        #region Internals
        public override bool Equals(object other)
        {
            if (!(other is MyPlane)) return false;
            return Equals((MyPlane)other);
        }

        public bool Equals(MyPlane other)
        {
            return (this.normal == other.normal && distance == other.distance);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(this.normal.GetHashCode(), this.distance.GetHashCode());
            //return normal.GetHashCode() ^ distance.GetHashCode();
        }
        public override string ToString()
        {
            return "Normal: " + this.normal.ToString() + " Distance: " + this.distance.ToString();
        }
        #endregion
    }
}