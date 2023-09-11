using UnityEngine;
using static UnityEngine.Mathf;


public class FunctionLibrary {


    public delegate Vector3 Function (float x, float z, float t);

    static Function[] functions = {Wave, MultiWave, Ripple, Sphere, Torus, Storm, VavylonGarden};

    public enum FunctionName { Wave, MultiWave, Ripple,  Sphere, Torus, Storm, VavylonGarden}


    public static Function GetFunction(FunctionName name) => functions[(int)name];

    public static FunctionName GetNextFunction(FunctionName name) =>
        ((int)name != (functions.Length -1)) ? name + 1 : 0;

    public static FunctionName GetRandomFunction(FunctionName name){
        var choice = (FunctionName)Random.Range(1, functions.Length);

        return ((choice != name) ? choice : 0);
    }

    public static Vector3 Morph(float u, float v, float t, Function from, Function To, float progress){
        // первые 2 параметра в smoothstep - смещение и масштаб функции
        // Unclamped не ограничивает выходное значение в границах а-б
        return Vector3.LerpUnclamped(from(u, v, t), To(u, v, t), SmoothStep(0f, 1f, progress));
    }


    public static Vector3 Wave(float u, float v, float t){
        Vector3 position = Vector3.zero;
        float dif = Cos(u + t);

        position.x = u;
        position.y = dif * Sin(PI * (u + v + t));
        position.z = v;
        return position;
    }


    public static Vector3 MultiWave(float u, float v, float t){
        Vector3 position = Vector3.zero;

        float y = Sin(PI *(u + 0.5f + t));
        float dif = Sin(u - v);

        y += 0.5f * Sin(2f * PI * (v + t));
        y += dif * Sin(PI * (u + v + 0.25f * t));

        position.x = u;
        position.y = y * (1f / 2.5f);
        position.z = v;

        return position;
    }


    public static Vector3 Ripple(float u, float v, float t){
        Vector3 position = Vector3.zero;

        float d =  Sqrt((u * u + v * v));
        float y =  Sin(PI * (4f * d - t));

        position.x = u;
        position.y = y / (1f + 10f * d);
        position.z = v;

        return position;
    }

    public static Vector3 Sphere(float u, float v, float t){
        Vector3 position = Vector3.zero;
        float r = 0.9f + 0.1f * Sin(PI * (6f * u + 4f * v + t));
        float s = r * Cos(0.5f * PI * v);

        position.x = s * Sin(PI * u);
        position.y = r * Sin(0.5f * PI * v);
        position.z = s * Cos(PI * u);

        return position;
    }

    public static Vector3 Torus(float u, float v, float t){
        Vector3 position = Vector3.zero;
        float r1 = 0.7f + 0.1f * Sin(PI * (6f * u + 0.5f * t));
        float r2 = 0.15f + 0.05f* Cos(PI * (8f * u + 4f * v + 2f * t));
        float s = r1 + r2 * Cos(PI * v);

        position.x = s * Sin(PI * u);
        position.y = r2 * Sin(PI * v);
        position.z = s * Cos(PI * u);

        return position;
    }

    public static Vector3 Storm(float u, float v, float t){
        Vector3 position = Vector3.zero;
        float r = Cos(4f * PI * v * t * 0.3f);

        position.x = r * Sin(PI * u);
        position.y = v;
        position.z = r * Cos(PI * u);

        return position;
    }

    public static Vector3 VavylonGarden(float u, float v, float t){
        Vector3 position = Vector3.zero;
        float r = 0.9f + 0.1f * Sin(8f * PI * v);
        float s = r * Cos(0.5f * PI * v);

        position.x = s * Sin(PI * (u + t * 0.6f));
        position.y = r * Sin(PI * 0.5f * v);
        position.z = s * Cos(PI * (u + t * 0.6f));

        return position;
    }
}
