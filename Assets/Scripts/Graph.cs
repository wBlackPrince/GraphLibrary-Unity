using System;
using UnityEngine;

public class Graph : MonoBehaviour{

    [SerializeField]
    Transform pointPrefab;

    [SerializeField, Range(10,200)]
    int resolution = 10;

    [SerializeField]
    FunctionLibrary.FunctionName function;

    Transform[] points;

    [SerializeField, Min(0f)]
    float functionDuration = 1f, transitionDuration = 1f;

    bool transitioning;

    FunctionLibrary.FunctionName transitionFunction;

    float duration;

    enum GameMode {Cycle, Random};

    [SerializeField]
    GameMode mode;



    void Awake(){
        float step = 2f/resolution;
        var position = Vector3.zero;
        var scale  = Vector3.one * step;

        points = new Transform[resolution * resolution];


        for(int i = 0; i < points.Length; i++){
            Transform point = points[i] = Instantiate(pointPrefab);

            point.localPosition = position;
            point.localScale = scale;
            point.SetParent(transform, false);
        }
    }



    void Update(){

        duration += Time.deltaTime;
        if(transitioning){ // время интерполяции
            if(duration >= transitionDuration){ // завершение интерполяции
                duration -= transitionDuration;
                transitioning = false;
            }
        }
        else if(duration >= functionDuration){  // начало интерполяции
            duration -= functionDuration;
            transitioning = true;
            transitionFunction = function;
            PickNextFunction();
        }


        if (transitioning){
            UpdateFunctionTransition();
        }
        else{
            UpdateFunction();
        }
    }


    void UpdateFunction(){
        FunctionLibrary.Function f = FunctionLibrary.GetFunction(function);
        float time = Time.time;
        float step = 2f/resolution;

        float v = 0.5f * step - 1f;

        for(int i = 0,x = 0, z = 0; i < points.Length; i++, x++){
            if(x == resolution){
                x = 0;
                z++;
                v = (z + 0.5f) * step - 1f;
            }

            float u = (x + 0.5f) * step - 1f;

            Vector3 position = f(u, v, time);
            points[i].localPosition = position;
        }
    }


    void UpdateFunctionTransition(){
        FunctionLibrary.Function from = FunctionLibrary.GetFunction(transitionFunction),
                to = FunctionLibrary.GetFunction(function);
        float progress = duration / transitionDuration;
        float time = Time.time;
        float step = 2f/resolution;

        float v = 0.5f * step - 1f;

        for(int i = 0,x = 0, z = 0; i < points.Length; i++, x++){
            if(x == resolution){
                x = 0;
                z++;
                v = (z + 0.5f) * step - 1f;
            }

            float u = (x + 0.5f) * step - 1f;

            points[i].localPosition = FunctionLibrary.Morph(
                u, v, time, from, to, progress
            );
        }
    }


    void PickNextFunction(){
        function = (mode == GameMode.Cycle) ? 
                    FunctionLibrary.GetNextFunction(function) : 
                    FunctionLibrary.GetRandomFunction(function);
    }
}
