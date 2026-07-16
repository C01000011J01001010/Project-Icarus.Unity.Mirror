using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    // Utility.Default
    public static partial class Utility
    {
        public static bool isUnityNull<T>(T obj)
        {
            // 순수 C# 차원에서의 진짜 Null 검사
            // 최적화를 위해 연산자 오버로딩이 없는 is 사용
            if (obj is null)
            {
                return true;
            }
            // 객체가 유니티 엔진의 객체(UnityEngine.Object)인 경우에만 Fake Null 검사
            if (obj is UnityEngine.Object unityObj)
            {
                return unityObj == null;
            }
            return false;
        }
    }
}
