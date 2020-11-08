using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class CameraRotator : MonoBehaviour {
    public Transform Upper;
    public Transform Lower;
    public Transform Camera;
    public float RotationSpeedUpper = 2f;
    public float RotationSpeedLower = 0.05f;
    [Space]
    public bool ShowUpper = true;
    public float TransitionTime = 2f;

    private bool lastShowUpper;
    private bool isTransitioning;
    private Vector3 sourceTransitionPosition;
    private Vector3 targetTransitionPosition;
    private Quaternion sourceTransitionRotation;
    private Quaternion targetTransitionRotation;
    private float sourceRotationSpeed;
    private float targetRotationSpeed;
    private float currentRotationSpeed;
    private float transitionTimer;

    private void OnEnable() {
        EditorApplication.update += Update;
    }

    private void OnDisable() {
        EditorApplication.update -= Update;
    }

    private void Update() {
        if (ShowUpper != lastShowUpper) {
            // start transition
            isTransitioning = true;
            sourceTransitionPosition = (ShowUpper ? Lower : Upper).localPosition;
            targetTransitionPosition = (ShowUpper ? Upper : Lower).localPosition;
            sourceTransitionRotation = (ShowUpper ? Lower : Upper).localRotation;
            targetTransitionRotation = (ShowUpper ? Upper : Lower).localRotation;
            sourceRotationSpeed = ShowUpper ? RotationSpeedLower : RotationSpeedUpper;
            targetRotationSpeed = ShowUpper ? RotationSpeedUpper : RotationSpeedLower;
            transitionTimer = TransitionTime - transitionTimer;
        }
        lastShowUpper = ShowUpper;

        if (isTransitioning) {
            transitionTimer += Time.smoothDeltaTime;
            Camera.localPosition = Vector3.Lerp(sourceTransitionPosition, targetTransitionPosition, MathUtils.EaseInOut(transitionTimer/TransitionTime));
            Camera.localRotation = Quaternion.Slerp(sourceTransitionRotation, targetTransitionRotation, MathUtils.EaseInOut(transitionTimer/TransitionTime));
            currentRotationSpeed = Mathf.Lerp(sourceRotationSpeed, targetRotationSpeed, MathUtils.EaseInOut(transitionTimer/TransitionTime));
            if (transitionTimer >= TransitionTime) {
                isTransitioning = false;
            }
        }
        if(!isTransitioning) {
            Camera.localPosition = (ShowUpper ? Upper : Lower).localPosition;
            Camera.localRotation = (ShowUpper ? Upper : Lower).localRotation;
            currentRotationSpeed = ShowUpper ? RotationSpeedUpper : RotationSpeedLower;
        }

        var currentRotation = transform.eulerAngles;
        currentRotation.y += currentRotationSpeed * Time.smoothDeltaTime;
        if (currentRotation.y >= 360) currentRotation.y -= 360;
        transform.eulerAngles = currentRotation;
    }
}