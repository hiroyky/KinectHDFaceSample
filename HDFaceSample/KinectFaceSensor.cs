using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Kinect.Face;

namespace HDFaceSample {
    /// <summary>
    /// Kinect v2に顔センサとして接続を行うクラスです．
    /// </summary>
    public class KinectFaceSensor : IFaceSensor {
        KinectSensor sensor = null;
        BodyFrameSource bodySource = null;
        BodyFrameReader bodyReader = null;
        HighDefinitionFaceFrameSource faceSource = null;
        HighDefinitionFaceFrameReader faceReader = null;
        FaceAlignment faceAlignment = null;
        FaceModel faceModel = null;

        public event EventHandler<FacePointEventArgs> FacePointUpdated;

        ~KinectFaceSensor() {
            if (faceModel != null) {
                faceModel.Dispose();
                faceModel = null;
            }
            if (sensor != null && sensor.IsOpen) {                
                sensor.Close();
            }
        }

        public void Initialize() {
            sensor = KinectSensor.GetDefault();
            if (sensor == null) {
                throw new System.IO.IOException("Failed to detect KinectSensor.");
            }

            bodySource = sensor.BodyFrameSource;
            bodyReader = bodySource.OpenReader();
            bodyReader.FrameArrived += BodyReader_FrameArrived;

            faceSource = new HighDefinitionFaceFrameSource(sensor);

            faceReader = faceSource.OpenReader();
            faceReader.FrameArrived += FaceReader_FrameArrived;

            faceModel = new FaceModel();
            faceAlignment = new FaceAlignment();
        }

        public void Open() {
            sensor.Open();
        }

        public void Close() {
            sensor.Close();
        }

        private void FaceReader_FrameArrived(object sender, HighDefinitionFaceFrameArrivedEventArgs e) {
            using (var frame = e.FrameReference.AcquireFrame()) {
                if (frame == null || !frame.IsFaceTracked) {
                    System.Diagnostics.Debug.WriteLine("frame is not face tracked.");
                    return;
                }
                frame.GetAndRefreshFaceAlignmentResult(faceAlignment);
                updateFacePoints();
            }
        }

        void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e) {
            using (var frame = e.FrameReference.AcquireFrame()) {
                if (frame == null) {
                    return;
                }

                Body[] bodies = new Body[frame.BodyCount];
                frame.GetAndRefreshBodyData(bodies);

                Body body = bodies.Where(b => b.IsTracked).FirstOrDefault();
                if (!faceSource.IsTrackingIdValid && body != null) {
                    System.Diagnostics.Debug.Write(body.TrackingId);
                    faceSource.TrackingId = body.TrackingId;
                }
            }
        }

        void updateFacePoints() {
            if (faceModel == null) {
                System.Diagnostics.Debug.WriteLine("face model is null");
                return;
            }
            if (FacePointUpdated == null) {
                System.Diagnostics.Debug.WriteLine("event handler is null");
                return;
            }

            var cameraVertices = faceModel.CalculateVerticesForAlignment(faceAlignment);
            if (cameraVertices.Count == 0) {
                System.Diagnostics.Debug.WriteLine("camera space ponits 0");
                return;
            }

            DepthSpacePoint[] depthVertices = new DepthSpacePoint[cameraVertices.Count];
            sensor.CoordinateMapper.MapCameraPointsToDepthSpace(cameraVertices.ToArray(), depthVertices);

            List<Point3> cameraPoints = new List<Point3>();
            List<Point2> depthPoints = new List<Point2>();
            for(int i = 0; i < cameraVertices.Count; ++i) { 
                cameraPoints.Add(new Point3(cameraVertices[i].X, cameraVertices[i].Y, cameraVertices[i].Z));
                depthPoints.Add(new Point2(depthVertices[i].X, depthVertices[i].Y));
            }
            FacePointUpdated(this, new FacePointEventArgs(cameraPoints, depthPoints));  
        }
    }    
}
