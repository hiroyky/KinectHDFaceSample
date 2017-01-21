using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDFaceSample {
    public class Point2 : IEquatable<Point2> {
        public float X, Y;

        public Point2(float x, float y) {
            X = x;
            Y = y;
        }

        public bool Equals(Point2 other) {
            if (this.X != other.X) {
                return false;
            }
            if (this.Y != other.Y) {
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
            return Equals((Point2)obj);
        }

        public override int GetHashCode() {
            return (int)(X + Y);
        }

        public static bool operator ==(Point2 a, Point2 b) {
            if (a == null) {
                return b == null;
            }
            if (b == null) {
                return a == null;
            }
            return a.Equals(b);
        }

        public static bool operator !=(Point2 a, Point2 b) {
            return !(a == b);
        }
    }
}
