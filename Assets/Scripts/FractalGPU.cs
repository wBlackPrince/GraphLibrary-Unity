using UnityEngine;

public class FractalGPU : MonoBehaviour{

    struct FractalPart{
        public Vector3 direction, worldPosition;
        public Quaternion rotation, worldRotation;
        public float spinAngle;
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

    Matrix4x4[][] matrices;

    ComputeBuffer[] matricesBuffers;

    static readonly int matricesId = Shader.PropertyToID("_Matrices");

    // для связки буфера с определенной командой рисования
    static MaterialPropertyBlock propertyBlock; 
    


    void OnEnable(){
        parts = new FractalPart[depth][];
        matrices = new Matrix4x4[depth][];
        matricesBuffers = new ComputeBuffer[depth];

        parts[0] = new FractalPart[1];
        int stride = 16 * 4;

        for(int li = 1, length = 5; li < depth; li++, length *= 5){
            parts[li] = new FractalPart[length];
            matrices[li] = new Matrix4x4[length];
            matricesBuffers[li] = new ComputeBuffer(length, stride);
        }

        parts[0][0] = CreatePart(0);


        for(int li = 1; li < depth; li++){
            FractalPart[] levelParts = parts[li];
            for(int fpi = 0; fpi < levelParts.Length; fpi+=5){
                for(int ci = 0; ci < 5; ci++){
                    levelParts[fpi + ci] = CreatePart(ci);
                }
            }
        }

        if(propertyBlock == null){
            propertyBlock ??= new MaterialPropertyBlock();
        }
    }

    void OnDisable(){
        for(int i = 0; i < matricesBuffers.Length; i++){
            matricesBuffers[i].Release();
        }

        matricesBuffers = null;
        parts = null;
        matrices = null;
    }

    void OnValidate(){
        if (parts != null && enabled){
            OnDisable();
            OnEnable();
        }
    }


    FractalPart CreatePart(int childIndex) => new FractalPart(){  
            direction = directions[childIndex],
            rotation = rotations[childIndex],
    };


    void Update(){
        float spinAngleDelta = 22.5f * Time.deltaTime;

        FractalPart rootPart = parts[0][0];
        rootPart.spinAngle += spinAngleDelta;
        rootPart.worldRotation = 
            transform.rotation *
            (rootPart.rotation * Quaternion.Euler(0f, rootPart.spinAngle, 0f));
        rootPart.worldPosition = transform.position;
        parts[0][0] = rootPart;

        float objectScale = transform.localScale.x;
        matrices[0][0] = Matrix4x4.TRS(
            rootPart.worldPosition, rootPart.worldRotation, objectScale * Vector3.one
        );

        float scale = objectScale;

        for(int li = 1; li < parts.Length; li ++){
            scale *= 0.5f;

            FractalPart[] parentsParts = parts[li - 1];
            FractalPart[] levelParts = parts[li];
            Matrix4x4[] levelMatrices = matrices[li];

            for(int fpi = 0; fpi < levelParts.Length; fpi++){
                FractalPart parent = parentsParts[fpi / 5];
                FractalPart part = levelParts[fpi];

                part.worldRotation = 
                        parent.worldRotation * Quaternion.Euler(0f, rootPart.spinAngle, 0f);
                part.worldPosition = 
                        parent.worldPosition + 
                        parent.worldRotation * // вращение родителя должно влиять на направление смешения потомка
                        (1.5f * scale * part.direction);
                levelParts[fpi] = part;
                levelMatrices[fpi] = Matrix4x4.TRS(
                    part.worldPosition, part.worldRotation, scale * Vector3.one
                );
            }
        }

        for(int i = 0; i < matricesBuffers.Length; i++){
            matricesBuffers[i].SetData(matrices[i]);  // загрузка матриц в графический процессор
        }

        var bounds = new Bounds(rootPart.worldPosition, 3f * objectScale * Vector3.one);
        for(int i = 0; i < matricesBuffers.Length; i++){
            ComputeBuffer buffer = matricesBuffers[i];
            buffer.SetData(matrices[i]);
            propertyBlock.SetBuffer(matricesId, buffer);
            Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, buffer.count, propertyBlock);
        }
    }
}
