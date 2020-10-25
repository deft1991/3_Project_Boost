using System;
using UnityEngine;

[DisallowMultipleComponent]
public class Oscillator : MonoBehaviour
{
    const double TAU = (Math.PI * 2); // about 6,28

    [SerializeField] private Vector3 movementVector = new Vector3(10, 10, 10);

    [SerializeField] float period = 2f;
    private float _movementFactor; // 0 for not moved, 1 for fully moved.

    private Vector3 _startingPos;

    // Start is called before the first frame update
    void Start()
    {
        _startingPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (period <= Mathf.Epsilon) // protect against period is zero
        {
            return;
        }

        var cycles = Time.time / period; // continually from 0. if 0 / 2 = 0, 10/2 = 5 ect.
        var rawSinWave = Math.Sin(TAU * cycles); // goes from -1 to 1
        // movementFactor = (float) (rawSinWave / 2f + 0.5f); // use if want to have direction from 0 to 1
        _movementFactor = (float) (rawSinWave); // use if want to have direction from -1 to 1

        Vector3 offset = movementVector * _movementFactor;
        transform.position = _startingPos + offset;
    }
}