using UnityEngine;
using TMPro;

public class FrameRateCounter : MonoBehaviour{
    [SerializeField]
    TextMeshProUGUI display;

    [SerializeField, Range(0, 2)]
    float sampleDuration;

    float duration;
    int frames;

    float bestDuration = float.MaxValue;
    float worstDuration = 0f;


    enum GameMods {FPS, MS};

    [SerializeField]
    GameMods mode;


    void Update(){
        float frameDuration = Time.unscaledDeltaTime;
        duration += Time.unscaledDeltaTime;
        frames++;

        if(bestDuration > frameDuration){
            bestDuration = frameDuration;
        }
        if(worstDuration < frameDuration){
            worstDuration = frameDuration;
        }


        if(duration >= sampleDuration){
            if(mode == GameMods.FPS){
                display.SetText(
                        "FPS\n{0:0}\n{1:0}\n{2:0}", 
                        1f/bestDuration, 
                        frames/duration, 
                        1f/worstDuration);
            }
            else{
                display.SetText(
                        "MS\n{0:1}\n{1:1}\n{2:1}", 
                        bestDuration * 1000f, 
                        frameDuration * 1000f, 
                        worstDuration * 1000f);
            }
            
            frames = 0;
            duration = 0f;
            bestDuration = float.MaxValue;
            worstDuration = 0f;
        }
    }
}
