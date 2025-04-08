using UnityEngine;
using System;

public class SpinnerBar : MonoBehaviour
{
    public Action OnComplete;
    public float rotationSpeed = 200f;
    private bool isSpinning = true;  // 회전 상태를 제어하는 변수

    void Start()
    {
        // RectBar의 OnComplete 이벤트에 StopSpinning 메서드를 연결
        var ProgressBar = FindFirstObjectByType<ProgressBar>();
        if (ProgressBar != null)
        {
            ProgressBar.OnComplete += StopSpinning;
        }
    }

    void Update()
    {
        if (isSpinning)
        {
            transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        }
    }

    private void StopSpinning(){
        isSpinning = false;
        if (transform.parent != null){
            Destroy(transform.parent.gameObject);
        }
        else{
            Destroy(gameObject);
            
        }
    }
}