// 예: Singleton_Scene의 'TouchableObject.cs' 스크립트
using UnityEngine;

public class TouchableObject : MonoBehaviour
{
    public void OnInteract()
    {
        Debug.Log(gameObject.name + "가 클릭되었습니다!");
        // 여기에 클릭됐을 때 할 일 (색 변경, 이동 등)을 넣으세요.
    }
}