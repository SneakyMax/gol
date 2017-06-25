using UnityEngine;

namespace Assets.Scripts
{
    public static class Utils
    {
        public static Vector3 GolemLevelPosition(this GameObject obj)
        {
            var cameraPosition = World.Instance.MainCamera.transform.position;
            var objPosition = obj.transform.position;

            var cameraToObjVector = (objPosition - cameraPosition).normalized;

            Vector3 intersection;
            Math3d.LinePlaneIntersection(out intersection, cameraPosition, cameraToObjVector, Vector3.up, Vector3.zero);

            return intersection;
        }

        public static float NormalizeDegrees(this float degrees)
        {
            while (degrees > 360)
                degrees -= 360;
            while (degrees < 0)
                degrees += 360;
            return degrees;
        }

        public static float DistanceTo(this Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b);
        }

        public static float ChangeScale(float fromMin, float fromMax, float toMin, float toMax, float value)
        {
            return (value - fromMin) * ((toMax - toMin) / (fromMax - fromMin)) + toMin;
        }
    }
}
