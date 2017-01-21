using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDFaceSample {
    public class FacePointEventArgs : EventArgs {
        public List<Point3> CameraSpacePoints;
        public List<Point2> DepthSpacePoints;

        public FacePointEventArgs() : base() { }
        public FacePointEventArgs(List<Point3> cameraSpacePoints, List<Point2> depthSpacePoints) : base() {
            this.CameraSpacePoints = cameraSpacePoints;
            this.DepthSpacePoints = depthSpacePoints;
        }
    }
}
