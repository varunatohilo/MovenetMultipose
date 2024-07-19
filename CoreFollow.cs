using System.Numerics;
using TensorFlowLite.MoveNet;
using UnityEngine;



public class CoreFollow : MonoBehaviour 
{
    public void Update(){
        transform.position = new UnityEngine.Vector2(MoveNetMultiPose.coordinates.x * 28 - 14, MoveNetMultiPose.coordinates.y * -14 + 7);
    }


}

