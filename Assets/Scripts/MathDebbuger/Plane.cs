using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;

namespace CustomMath
{
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
            this.normal = inNormal;
            this.distance = Vec3.Project(inPoint, inNormal).magnitude;
        }

        public MyPlane(Vec3 inNormal, float d)
        {
            this.normal = inNormal;
            this.distance = d;
        }

        public MyPlane(Vec3 a, Vec3 b, Vec3 c)
        {
            this.normal = Vec3.Cross(a - c, b - c).normalized;
            this.distance = Vec3.Project(a, this.normal).magnitude;
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

        public void Translate(MyPlane plane, Vec3 translation)
        {
            plane.distance = Vec3.Project(translation, normal).magnitude;
        }
        public Vec3 ClosestPointOnPlane(Vec3 point) 
        {
            return point;
        }

       public void Flip()
        {
            this.normal = -this.normal;
        }

        public void GetDistanceToPoint(Vec3 point)
        {

        }
        public void GetSide(Vec3 point)
        {

        }
        public void SameSide(Vec3 inPt0, Vec3 inPt1)
        {

        }
        public void Set3Points(Vec3 a, Vec3 b, Vec3 c)
        {
            this.normal = Vec3.Cross(a - c, b - c).normalized;
            this.distance = Vec3.Project(a, this.normal).magnitude;
        }
        public void SetNormalAndPosition(Vec3 inNormal, Vec3 inPoint) 
        {
            this.normal = inNormal;
            this.distance = Vec3.Project(inPoint, inNormal).magnitude;
        }
        public void Translate(Vec3 translation)
        {
            distance = Vec3.Project(translation, normal).magnitude;
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
            return normal == other.normal && distance == other.distance;
        }
        public override int GetHashCode()
        {
            return normal.GetHashCode() ^ distance.GetHashCode();
        }
        public override string ToString()
        {
            return "Normal: " + normal.ToString() + " Distance: " + distance.ToString();
        }
        #endregion
    }
}