using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

//We are using SystemBase, as we need to accees the managed data of the Input System.
//To access the managed  data into ECS and easilly accessing them we need use the SystemBase
public partial class PlayerInputSystem : SystemBase
{
    private SurvivoursInput survivalInput;
    protected override void OnCreate()
    {
        survivalInput = new SurvivoursInput();
        survivalInput.Enable();
    }

    protected override void OnUpdate()
    {
        var currentInput = (float2)survivalInput.Player.Move.ReadValue<Vector2>();

        foreach(var direction in 
            SystemAPI.Query<RefRW <CharacterMoveDirection>>().WithAll<PlayerTag>())
        {
            direction.ValueRW.Value = currentInput;
        }
    }
}
