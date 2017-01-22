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
        HighDefinitionFaceFrameSource hdFaceFrameSource = null;
        HighDefinitionFaceFrameReader hdFaceFrameReader = null;
        FaceAlignment faceAlignment = null;
        FaceModel faceModel = null;
        FaceModelBuilder faceModelBuilder = null;

        public event EventHandler<FacePointEventArgs> FacePointUpdated;

        public int FrameWidth {
            get { return sensor.DepthFrameSource.FrameDescription.Width; }
        }

        public int FrameHeight {
            get { return sensor.DepthFrameSource.FrameDescription.Height; }
        }

        public bool IsFaceModelProduced { get; private set; }

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

            hdFaceFrameSource = new HighDefinitionFaceFrameSource(sensor);

            hdFaceFrameReader = hdFaceFrameSource.OpenReader();
            hdFaceFrameReader.FrameArrived += FaceReader_FrameArrived;

            faceModel = new FaceModel();
            faceAlignment = new FaceAlignment();

            FaceModelBuilderAttributes attributes = FaceModelBuilderAttributes.None;
            faceModelBuilder = hdFaceFrameSource.OpenModelBuilder(attributes);
            if (faceModelBuilder == null) {
                throw new System.IO.IOException("Failed to open model builder.");
            }
            faceModelBuilder.BeginFaceDataCollection();
            faceModelBuilder.CollectionStatusChanged += FaceModelBuilder_CollectionStatusChanged;
            faceModelBuilder.CaptureStatusChanged += FaceModelBuilder_CaptureStatusChanged;
            faceModelBuilder.CollectionCompleted += FaceModelBuilder_CollectionCompleted;
        }

        private void FaceModelBuilder_CollectionStatusChanged(object sender, FaceModelBuilderCollectionStatusChangedEventArgs e) {
            if (faceModelBuilder == null) {
                return;
            }
            System.Diagnostics.Debug.WriteLine("CollectionStatus: " + faceModelBuilder.CollectionStatus.ToString());
        }

        private void FaceModelBuilder_CaptureStatusChanged(object sender, FaceModelBuilderCaptureStatusChangedEventArgs e) {
        }

        public void Open() {
            sensor.Open();
        }

        public void Close() {
            sensor.Close();
        }

        void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e) {
            using (var frame = e.FrameReference.AcquireFrame()) {
                if (frame == null) {
                    return;
                }

                Body[] bodies = new Body[frame.BodyCount];
                frame.GetAndRefreshBodyData(bodies);

                Body body = bodies.Where(b => b.IsTracked).FirstOrDefault();
                if (!hdFaceFrameSource.IsTrackingIdValid && body != null) {
                    System.Diagnostics.Debug.Write(body.TrackingId);
                    hdFaceFrameSource.TrackingId = body.TrackingId;
                }
            }
        }

        void FaceReader_FrameArrived(object sender, HighDefinitionFaceFrameArrivedEventArgs e) {
            using (var frame = e.FrameReference.AcquireFrame()) {
                if (frame == null || !frame.IsFaceTracked) {
                    return;
                }
                frame.GetAndRefreshFaceAlignmentResult(faceAlignment);
                updateFacePoints();
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

        void FaceModelBuilder_CollectionCompleted(object sender, FaceModelBuilderCollectionCompletedEventArgs e) {

            System.Diagnostics.Debug.WriteLine("FaceModelBuild Complete!");
            faceModel = e.ModelData.ProduceFaceModel();
            IsFaceModelProduced = true;
            faceModelBuilder.Dispose();
            faceModelBuilder = null;
        }

    }
}
