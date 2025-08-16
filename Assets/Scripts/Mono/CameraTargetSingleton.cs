using UnityEngine;

public class CameraTargetSingleton : MonoBehaviour
{
   public static CameraTargetSingleton Instance;

    //Assign the singleton instance in the dirty way
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }
        Instance = this;
    }
}
