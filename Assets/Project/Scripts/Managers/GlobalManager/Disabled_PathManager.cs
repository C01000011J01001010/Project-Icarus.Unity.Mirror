using System.Collections;
using System.IO; // Path.Combine을 사용하기 위해 반드시 필요합니다!
using UnityEngine;

public class Disabled_PathManager : BaseGlobalManager, IGlobalManager
{
    public class Directory
    {
        // 런타임에 초기화
        private string _main;
        public string Save { get; private set; }
        public string Option { get; private set; }

        public Directory()
        {
            // 응용 프로그램에서 사용하는 데이터 경로
            // Executable Application

            // [수정됨] 하드코딩된 "/" 대신 Path.Combine 사용
            _main = Path.Combine(Application.persistentDataPath, "Datas");

#if UNITY_EDITOR
            Debug.Log("저장 경로 : " + _main);
#endif

            // [수정됨] 각 하위 폴더도 Path.Combine으로 안전하게 병합
            Save = Path.Combine(_main, "Saves");
            Option = Path.Combine(_main, "Options");
        }
    }

    public class FileName
    {
        public string GraphicSettings = "GraphicSettings.save";
    }

    // 디렉터리
    public Directory directory { get; private set; }
    public FileName fileName { get; private set; }

    public void Exit()
    {

    }

    public IEnumerator Initialize()
    {
        directory = new();
        fileName = new();
        yield return null;
    }
}