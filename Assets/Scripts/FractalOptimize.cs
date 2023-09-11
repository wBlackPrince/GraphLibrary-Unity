using UnityEngine;

public class FractalOptimize : MonoBehaviour{

    struct FractalPart{
        public Vector3 direction;
        public Quaternion rotation;
        public Transform transform;
    }


    [SerializeField, Range(1,8)]
    int depth = 4;


    [SerializeField]
    Mesh mesh;


    [SerializeField]
    Material material;

    static Vector3[] directions = {
        Vector3.up, Vector3.right, Vector3.left, Vector3.forward, Vector3.back}; 

    static Quaternion[] rotations = {
        Quaternion.identity,
        Quaternion.Euler(0f, 0f, -90f), Quaternion.Euler(0f, 0f, 90f),
        Quaternion.Euler(90f, 0f, 0f), Quaternion.Euler(-90f, 0f, 0f),
    };


    FractalPart[][] parts;


    void Awake(){
        float scale = 1f;
        
        parts = new FractalPart[depth][];

        parts[0] = new FractalPart[1];

        for(int li = 1, length = 5; li < depth; li++, length *= 5){
            parts[li] = new FractalPart[length];
        }

        parts[0][0] = CreatePart(0, 0, scale);

        for(int li = 1; li < depth; li++){
            scale *= 0.5f;
            FractalPart[] levelParts = parts[li];
            for(int fpi = 0; fpi < levelParts.Length; fpi+=5){
                for(int ci = 0; ci < 5; ci++){
                    levelParts[fpi + ci] = CreatePart(li, ci, scale);
                }
            }
        }
    }


    FractalPart CreatePart(int li, int childIndex, float scale){
        var go = new GameObject("FractalPart" + li + childIndex);
        go.transform.localScale = scale * Vector3.one;
        go.transform.SetParent(transform, false);
        go.AddComponent<MeshFilter>().mesh = mesh;
        go.AddComponent<MeshRenderer>().material = material;

        return new FractalPart(){  
            direction = directions[childIndex],
            rotation = rotations[childIndex],
            transform = go.transform
        };
    }


    void Update(){
        Quaternion deltaRotation = Quaternion.Euler(0f, 22.5f * Time.deltaTime, 0f);

        FractalPart rootPart = parts[0][0];
        rootPart.rotation *= deltaRotation;
        rootPart.transform.localRotation = rootPart.rotation;
        parts[0][0] = rootPart;



        for(int li = 1; li < parts.Length; li ++){
            FractalPart[] parentsParts = parts[li - 1];
            FractalPart[] levelParts = parts[li];
            for(int fpi = 0; fpi < levelParts.Length; fpi++){
                Transform parentTransform = parentsParts[fpi / 5].transform;
                FractalPart part = levelParts[fpi];

                part.rotation *= deltaRotation;

                part.transform.localRotation = parentTransform.localRotation * part.rotation;
                part.transform.localPosition = 
                        parentTransform.localPosition + 
                        parentTransform.localRotation * // вращение родителя должно влиять на направление смешения потомка
                        (1.5f * part.transform.localScale.x * part.direction);
                levelParts[fpi] = part;
            }
        }
    }
}
