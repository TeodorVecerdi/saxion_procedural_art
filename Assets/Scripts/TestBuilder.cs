using System;
using UnityEngine;

public class TestBuilder : MonoBehaviour {
    public static TestBuilder CurrentActive;
    public TestBuilder StraightPrefab;
    public TestBuilder CurvePrefab;

    public Vector3 currentPosition;
    public Vector3 currentRotation;
    public Vector3 nextRotation;
    public Vector3 currentDirection;

    private void Awake() {
        if (CurrentActive == null) {
            CurrentActive = this;
            // Initialize(Vector3.zero, Vector3.zero, Vector3.back, Vector3.zero);
        }
    }

    public void Initialize(Vector3 position, Vector3 rotation, Vector3 direction, Vector3 nextRotation) {
        currentPosition = position;
        currentRotation = rotation;
        currentDirection = direction;
        this.nextRotation = nextRotation;

        transform.position = currentPosition;
        transform.eulerAngles = currentRotation;
    }

    private void Update() {
        if (CurrentActive != this) return;

        if (Input.GetKeyDown(KeyCode.S)) {
            CreateStraight();
        } else if (Input.GetKeyDown(KeyCode.L)) {
            CreateCurveL();
        } else if (Input.GetKeyDown(KeyCode.R)) {
            CreateCurveR();
        }
    }

    private void CreateStraight() {
        var newObj = Instantiate(StraightPrefab, transform.parent);
        newObj.Initialize(currentPosition + currentDirection, nextRotation, currentDirection, nextRotation);
        CurrentActive = newObj;
    }

    private void CreateCurveL() {
        var newObj = Instantiate(CurvePrefab, transform.parent);
        var rotatedDirection = Quaternion.AngleAxis(-90, Vector3.up) * currentDirection;
        newObj.Initialize(currentPosition + currentDirection, nextRotation - 90 * Vector3.up, rotatedDirection, currentRotation - 90 * Vector3.up);
        CurrentActive = newObj;
    }

    private void CreateCurveR() {
        var newObj = Instantiate(CurvePrefab, transform.parent);
        var rotatedDirection = Quaternion.AngleAxis(-90, Vector3.up) * currentDirection;
        newObj.Initialize(currentPosition + currentDirection, nextRotation + 90 * Vector3.up, rotatedDirection, currentRotation + 90 * Vector3.up);
        CurrentActive = newObj;
    }
}