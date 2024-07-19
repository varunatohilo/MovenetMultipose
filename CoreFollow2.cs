using System.Numerics;
using TensorFlowLite.MoveNet;
using UnityEngine;



public class CoreFollow2 : MonoBehaviour 
{
    public void Update(){
        transform.position = new UnityEngine.Vector2(MoveNetMultiPose.coordinates2.x * 28 - 14, MoveNetMultiPose.coordinates2.y * -14 + 7);
    }


}

