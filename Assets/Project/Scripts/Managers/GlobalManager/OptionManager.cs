using CoreEngine.EventBus;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.DebugUI;

// 버전별로 struct를 만드는 것이 좋음
public struct GraphicOptionValues
{
    /*
     * 해상도 :
     * enum으로 하나씩 다 쓰기 -> enum으로 저장했기 때문에 적용할 때 width와 height를 계산
     * 비율, 기준 픽셀 -> 픽셀과 비율을 모두 입력해놓아야함, 설정창에서 글자를 계산해야함
     */
    public AA_Sampling Antialiasing;
    public ResolutionType   resolutionType;
    public UnityEngine.ShadowQuality shadowLevel;
    public int frameRate;
    public GraphicLevel graphicLevel;
    public int brightness; // 0 ~ 9
    public float contrast; // 0 ~ 1
    public float fileldOfView; // FOV
    public bool verticalSync; // 수직동기화
    public bool fullScreen;

    public static GraphicOptionValues defaultOption = new()
    {
        // ctrl + space를 누르면 초기화 하지 않은 변수를 확인 가능함
        Antialiasing = AA_Sampling.MSAA_2X,
        resolutionType = ResolutionType._1920x1080,
        shadowLevel = UnityEngine.ShadowQuality.HardOnly, // 안표시/간단표시/다표시
        frameRate = 30,
        graphicLevel = GraphicLevel.Midium, // projectSettings의 Quality -> setting의 순서대로
        brightness = 10,
        contrast = 1f,
        fileldOfView = 60f,
        verticalSync = true,
        fullScreen = true
    };

#if UNITY_EDITOR
    public static GraphicOptionValues testOption = new()
    {
        Antialiasing = AA_Sampling.MSAA_8X,
        resolutionType = ResolutionType._1920x1080,
        shadowLevel = UnityEngine.ShadowQuality.HardOnly,
        frameRate = 30,
        graphicLevel = GraphicLevel.High,
        brightness = 10,
        contrast = 1f,
        fileldOfView = 90f,
        verticalSync = true,
        fullScreen = true
    };
#endif
}

public delegate void DelegateGraphicOptionChanged(GraphicOptionValues value);

public struct GraphicOptionChangedEvent : IEvent
{
    GraphicOptionValues optionValues;
    public GraphicOptionChangedEvent(GraphicOptionValues optionValues) { this.optionValues = optionValues; }
}

public class OptionManager : MonoBehaviour// BaseGlobalManager, IGlobalManager
{
    public static event DelegateGraphicOptionChanged OnGraphicOptionChanged;
    public static GraphicOptionValues appliedGraphicOption; // 현재 적용중인 그래픽 세팅을 저장

    private static Vector2Int DefaultResolution = new Vector2Int(1920, 1080);
    private Volume optionVolume; // 씬의 시각적 효과를 제어하는 객체, 주로 후처리
    private ColorAdjustments adjustments;

    

    public void Exit()
    {

    }

    public IEnumerator Initialize()
    {
        CreateVolume();
        //ApplyGraphicSetting(FileManager.savedGraphicOption);

        yield return null;
    }
    private void CreateVolume()
    {
        optionVolume = gameObject.AddComponent<Volume>();
        optionVolume.isGlobal = true; // 카메라가 어디에 있든 상관없이 후처리를 적용 가능
        optionVolume.priority = -1.0f; // 값이 높을수록 효과가 우선적으로 적용 -> 다른 옵션이 들어오면 기준 설정을 무시하도록 하기 위해 -1을 사용


        // volume을 만들고 나면 profile을 넣어야한다.
        // profile은 ScriptableObject
        // 클래스이니 파일이 아닌 코드로도 만들 수 있음
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();

        // ovveride를 켬으로써 바로 사용할 수 있도록 함
        adjustments = profile.Add<ColorAdjustments>(true);

        optionVolume.profile = profile;
    }

    public void SetGraphicLevel(GraphicLevel level)
    {
        appliedGraphicOption.graphicLevel = level;
        QualitySettings.SetQualityLevel((int)level);
    }

    public void ApplyGraphicSetting(GraphicOptionValues value)
    {
        appliedGraphicOption = value;

        //graphicLevel -> 레벨을 바꾸면 다른 데이터가 덮어씌워지니 먼저 변경
        QualitySettings.SetQualityLevel((int)value.graphicLevel);

        //resolutionType, fullScreen -> 해상도와 전체화면 여부
        //Vector2Int resolution = GetResolution(value.resolutionType);
        Vector2Int resolution = CalculateResolution(value.resolutionType);

        //또는 RectInt rect = RectInt.zero;
        Screen.SetResolution(resolution.x, resolution.y , value.fullScreen);

        //frameRate -> 프로그램이 돌아가는 속도
        Application.targetFrameRate = value.frameRate;

        //shadowLevel 0, 1, 2
        QualitySettings.shadows = value.shadowLevel;

        //verticalSync;              프레임 중간에 몇개나 그려놓을까?
        QualitySettings.vSyncCount = value.verticalSync ? 1 : 0;

        // Antialiasing;
        //      Rendering Path
        //      Vertex Shader
        //      Pixel Shader -> Post Process (후처리)로 안티에일리어싱 실행
        //      target픽셀에서 주변 픽셀과의 차이를 보고 경계로 판단(Sampling)
        //      주변 픽셀을 가져오는(Sampling의) 범위를 지정해야함
        //      [0, 8] 범위
        QualitySettings.antiAliasing = (int)value.Antialiasing;

        //brightness
        //contrast
        if(adjustments is not null)
        {
            adjustments.contrast.overrideState = true;
            adjustments.postExposure.overrideState = true;

            //   0 ~ 1  :   0 ~ 1   <- x
            //   0 ~ 50 :   0 ~ B-A <- x(B-A)
            // -20 ~ 30 :   A ~ B   <- x(B-A) + A

            //   0 ~ 1  : 선형보간법
            //              0 ~ 1   <- (x) * B          == 0 ~ B 이 숫자의 범위를 집합 B
            //              1 ~ 0   <- (1 - x) * A      == A ~ 0 이 숫자의 범위를 집합 A
            //              A U B == (1 - x) * A + Bx   == A ~ B
            // 선형보간법 : [L]inear int[ERP]olation -> Lerp
            adjustments.contrast.value = Mathf.Lerp(-20, 30, value.contrast);

            // -1 ~ 0.5
            adjustments.postExposure.value = Mathf.Lerp(-1, 0.5f, value.brightness / 10.0f);
        }

        // 그래픽 옵션이 바뀌었다는 것을 알림
        OnGraphicOptionChanged?.Invoke(value);

        //      Built-In  : 유니티 기본 렌더 파이프라인 (그래픽이 뛰어나진 않지만 빠름)
        //                   -> 모바일, 저사양pc 저격
        //                   -> Post Process Volume 사용
        //      ShaderGraph  -> 변수명부터 다름

        //      URP(SRP)  : Universal Render Pipeline (범용적으로 사용하고 있는 파이프라인) 그래픽 완성도는 HDRP보다 떨어짐
        //                   -> 범용
        //                   -> Volume
        //      ShaderGraph  -> 제일 많이 사용

        //      HDRP      : High Definitial Render Pipeline (언리얼 저격 -> 그래픽이 매우 뛰어나지만 상대적으로 시간 성능이 떨어짐)
        //                   -> 고사양 pc 저격
        //                   -> Volume
        //      ShaderGraph  -> 제일 많이 사용
    }

    // 반환형식으로 하나의 문장임
    public static Vector2Int GetResolution(ResolutionType resolutionType) => resolutionType switch
    {
        ResolutionType._800x480 => new Vector2Int(800, 480),
        ResolutionType._800x600 => new Vector2Int(800, 600),
        ResolutionType._1152x768 => new Vector2Int(1152, 768),
        ResolutionType._1280x720 => new Vector2Int(1280, 720),
        ResolutionType._1440x1080 => new Vector2Int(1440, 10800),
        ResolutionType._1920x1080 => new Vector2Int(1920, 1080),
        ResolutionType._2048x1080 => new Vector2Int(2048, 1080),
        ResolutionType._2560x1440 => new Vector2Int(2560, 1440),

        _=> DefaultResolution,
    };

    public static Vector2Int CalculateResolution(ResolutionType resolutionType)
    {
        if (Enum.IsDefined(typeof(ResolutionType), resolutionType))
        {
            string originName = resolutionType.ToString();

            originName = originName.TrimStart('_');
            string[] values = originName.Split('x');

            if (values.Length == 2 &&
                int.TryParse(values[0], out int width) &&
                int.TryParse(values[1], out int height))
            {
                return new Vector2Int(width, height);
            }
        }

        return DefaultResolution;
    }
}
