using UnityEngine;

public class Fractal : MonoBehaviour{

    [SerializeField, Range(1,8)]
    int depth = 4;

    void Start(){

        if (depth <= 1){
            return;
        }

        Fractal childA = CreateChild(depth, Vector3.up, Quaternion.identity);
        Fractal childB = CreateChild(depth, Vector3.right, Quaternion.Euler(0f, 0f, -90f));
        Fractal childC = CreateChild(depth, Vector3.left, Quaternion.Euler(0f, 0f, 90f));
        Fractal childD = CreateChild(depth, Vector3.forward, Quaternion.Euler(90f, 0f, 0f));
        Fractal childE = CreateChild(depth, Vector3.back, Quaternion.Euler(-90f, 0f, 0f));

        childA.transform.SetParent(transform, false);
        childB.transform.SetParent(transform, false);
        childC.transform.SetParent(transform, false);
        childD.transform.SetParent(transform, false);
        childE.transform.SetParent(transform, false);
    }


    Fractal CreateChild(int depth, Vector3 direction, Quaternion rotation){
        name = "Fractal" + depth;

        Fractal child = Instantiate(this);
        child.depth = depth - 1;
        child.transform.localPosition = 0.75f * direction;
        child.transform.localScale = 0.5f * Vector3.one;
        child.transform.localRotation = rotation;

        return child;
    }


    void Update(){
        transform.Rotate(22.5f * Time.deltaTime, 22.5f * Time.deltaTime, 22.5f * Time.deltaTime);
    }

}
