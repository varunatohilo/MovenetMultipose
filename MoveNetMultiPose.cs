namespace TensorFlowLite.MoveNet
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Assertions;

    /// <summary>
    /// MoveNet Example
    /// https://www.tensorflow.org/hub/tutorials/movenet
    /// </summary>
    public class MoveNetMultiPose : BaseVisionTask
    {
        [System.Serializable]
        public class Options
        {
            [FilePopup("*.tflite")]
            public string modelPath = string.Empty;
            public AspectMode aspectMode = AspectMode.Fit;
            public TfLiteDelegateType delegateType = TfLiteDelegateType.GPU;
        }

        public static MoveNetPose.Joint coordinates;        
        public static MoveNetPose.Joint coordinates2;        


        // [6, 56]
        // Up to 6 people
        // 17 * 3 (y, x, confidence) + [y_min, x_min, y_max, x_max, score] = 56
        private readonly float[,] outputs0;
        public readonly MoveNetPoseWithBoundingBox[] poses;

        public MoveNetMultiPose(Options options)
        {
            var interpreterOptions = new InterpreterOptions();
            interpreterOptions.AutoAddDelegate(options.delegateType, typeof(byte));
            Load(FileUtil.LoadFile(options.modelPath), interpreterOptions);

            AspectMode = options.aspectMode;

            int[] outputShape = interpreter.GetOutputTensorInfo(0).shape;

            Assert.AreEqual(1, outputShape[0]);
            Assert.AreEqual(6, outputShape[1]);
            Assert.AreEqual(56, outputShape[2]);

            outputs0 = new float[outputShape[1], outputShape[2]];

            int poseCount = outputShape[1];
            poses = new MoveNetPoseWithBoundingBox[poseCount];
            for (int i = 0; i < poseCount; i++)
            {
                poses[i] = new MoveNetPoseWithBoundingBox();
            }
        }


        protected override void PostProcess()
        {
            interpreter.GetOutputTensorData(0, outputs0);
            GetResults();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async UniTask PostProcessAsync(CancellationToken cancellationToken)
        {
            interpreter.GetOutputTensorData(0, outputs0);
            GetResults();
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        private float Distance( MoveNetPose.Joint leftEye, MoveNetPose.Joint rightEye ){
            return Mathf.Sqrt((leftEye.x - rightEye.x)*(leftEye.x - rightEye.x) + (leftEye.y - rightEye.y)*(leftEye.y - rightEye.y));
        }
        public MoveNetPoseWithBoundingBox[] GetResults()
        {
            for (int poseIndex = 0; poseIndex < poses.Length; poseIndex++)
            {
                var pose = poses[poseIndex];
                for (int jointIndex = 0; jointIndex < pose.Length; jointIndex++)
                {
                    pose[jointIndex] = new MoveNetPose.Joint(
                        y: outputs0[poseIndex, jointIndex * 3 + 0],
                        x: outputs0[poseIndex, jointIndex * 3 + 1],
                        score: outputs0[poseIndex, jointIndex * 3 + 2]
                    );

                }

                const int BOX_OFFSET = MoveNetPose.JOINT_COUNT * 3;
                pose.boundingBox = Rect.MinMaxRect(
                    outputs0[poseIndex, BOX_OFFSET + 1],
                    outputs0[poseIndex, BOX_OFFSET + 0],
                    outputs0[poseIndex, BOX_OFFSET + 3],
                    outputs0[poseIndex, BOX_OFFSET + 2]
                );
                pose.score = outputs0[poseIndex, BOX_OFFSET + 4];
            }
                int personInFrontIndex = 0;
                float distanceBetweenEyes = Distance(poses[0].joints[3],poses[0].joints[4]);
            for(int i = 1 ; i< poses.Length; i++){
                float newDistance = Distance(poses[i].joints[3],poses[i].joints[4]);
                if(newDistance>distanceBetweenEyes){
                    distanceBetweenEyes = newDistance;
                    personInFrontIndex = i;
                }
            }

            coordinates = poses[personInFrontIndex].joints[3];
            coordinates2 = poses[personInFrontIndex].joints[4];
            return poses;
        }
    }
}
