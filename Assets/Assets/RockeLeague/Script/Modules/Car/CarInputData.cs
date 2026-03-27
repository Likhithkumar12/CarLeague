using Fusion;
using UnityEngine;

public struct CarInputData : INetworkInput
{
    public Vector2 Move;
    public float Roll;
    public NetworkBool Jump;
    public NetworkBool Boost;
    public NetworkBool Drift;
}