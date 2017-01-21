using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDFaceSample {
    public struct Point3 : IEquatable<Point3> {
        public float X, Y, Z;

        public Point3(float x, float y, float z) {
            X = x;
            Y = y;
            Z = z;
        }

        public bool Equals(Point3 other) {
            if (this.X != other.X) {
                return false;
            }
            if (this.Y != other.Y) {
                return false;
            }
            if (this.Z != other.Z) {
                return false;
            }
            return true;
        }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }
            if (!base.Equals(obj)) {
                return false;
            }
            return Equals((Point3)obj);
        }

        public override int GetHashCode() {
            return (int)(X + Y + Z);
        }

        public static bool operator ==(Point3 a, Point3 b) {
            if (a == null) {
                return b == null;
            }
            if (b == null) {
                return a == null;
            }
            return a.Equals(b);
        }

        public static bool operator !=(Point3 a, Point3 b) {
            return !(a == b);
        }
    }
}
