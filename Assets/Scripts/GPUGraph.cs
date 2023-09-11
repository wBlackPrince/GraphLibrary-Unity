using System;
using UnityEngine;

public class GPUGraph : MonoBehaviour{
    // размер выделения для нашего буфера
    const int maxResolution = 1000;

    [SerializeField, Range(10,maxResolution)]
    int resolution = 10;

    [SerializeField]
    FunctionLibrary.FunctionName function;


    [SerializeField, Min(0f)]
    float functionDuration = 1f, transitionDuration = 1f;

    bool transitioning;

    FunctionLibrary.FunctionName transitionFunction;

    float duration;

    enum GameMode {Cycle, Random};

    [SerializeField]
    GameMode mode;

    // хранение позиций точек на графическом процессоре
    ComputeBuffer positionsBuffer;

    [SerializeField]
    ComputeShader computeShader;

    [SerializeField]
    Material material;


    [SerializeField]
    Mesh mesh;


    // обращение к свойствам вычислительного шейдера
    static readonly int 
            positionsId = Shader.PropertyToID("_Positions"),
            resolutionId = Shader.PropertyToID("_Resolution"),
            stepId = Shader.PropertyToID("_Step"),
            timeId = Shader.PropertyToID("_Time"),
            transitionProgressId = Shader.PropertyToID("_TransitionProgress");




    void OnEnable(){
        // вторым аргументом мы указывем объем памяти для 1 элемента
        positionsBuffer = new ComputeBuffer(maxResolution * maxResolution, 3 * 4);
    }

    void OnDisable(){
        positionsBuffer.Release();
        positionsBuffer = null;
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


        UpdateFunctionOnGPU();
    }


    void UpdateFunctionOnGPU(){ // установка свойств шейдера
        float step = 2f / resolution;
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetFloat(timeId, Time.time);

        if(transitioning){
            computeShader.SetFloat(
                transitionProgressId,
                Mathf.SmoothStep(0f, 1f, duration / transitionDuration)
            );
        }

        var kernelIndex = (int)function + (int) (transitioning ? transitionFunction : function) * 7;

        // связь буфера с ядром (установка буфера позиций)
        // 1 аргумент - индекс ядра
        computeShader.SetBuffer(kernelIndex, positionsId, positionsBuffer);

        // у нас фиксированный размер группы 8х8, которые нам нужны в 2 измерениях
        int groups = Mathf.CeilToInt(resolution / 8f);
        // запуск ядра (1 аргумент - индекс ядра, 
        // остальные - число запускаемых групп, разделенных по измерениям)
        computeShader.Dispatch(kernelIndex, groups, groups, 1);

        material.SetBuffer(positionsId, positionsBuffer);
        material.SetFloat(stepId, step);

        // процедурное рисование не использует игровые объекты unity
        // мы укажем границу рисования (прямоугольник)
        // учтем также масштаб точек графика
        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f/resolution));
        Graphics.DrawMeshInstancedProcedural(
            mesh, 0 , material, bounds, resolution * resolution
        );

    }


    void PickNextFunction(){
        function = (mode == GameMode.Cycle) ? 
                    FunctionLibrary.GetNextFunction(function) : 
                    FunctionLibrary.GetRandomFunction(function);
    }
}
