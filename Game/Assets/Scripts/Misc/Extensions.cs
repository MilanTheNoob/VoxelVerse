using UnityEngine;

/// <summary>
/// A small static class storing basic extensions to floats & Vector3s
/// </summary>
public static class Extensions
{
    /// <summary>Checks whether value is near to zero within a tolerance</summary>
    public static bool isZero(this float value) { return Mathf.Abs(value) < 0.0000000001f; }
    /// <summary>Checks whether vector is near to zero within a tolerance</summary>
    public static bool isZero(this Vector3 vector3) { return vector3.sqrMagnitude < 9.99999943962493E-11; }
    /// <summary> Checks whether vector is exceeding the magnitude within a small error tolerance</summary>
    public static bool isExceeding(this Vector3 vector3, float magnitude) { return vector3.sqrMagnitude > magnitude * magnitude * 1.01f; }
}