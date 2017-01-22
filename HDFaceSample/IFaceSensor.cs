using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDFaceSample {
    public interface IFaceSensor {
        void Initialize();
        void Open();
        void Close();
        event EventHandler<FacePointEventArgs> FacePointUpdated;
        int FrameWidth { get; }
        int FrameHeight { get; }
    }
}
