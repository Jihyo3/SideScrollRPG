using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SideScrollRPG.EditorTools
{
    /// <summary>
    /// Windows 빌드. 계획서 7일차의 "빌드를 남에게 건넬 수 있다"를 위한 최소 스크립트.
    /// 배치 실행: -executeMethod SideScrollRPG.EditorTools.BuildScript.BuildWindows
    /// </summary>
    public static class BuildScript
    {
        const string OutputPath = "Build/Windows/무너진문.exe";

        [MenuItem("SideScrollRPG/Windows 빌드", false, 3)]
        public static void BuildWindows()
        {
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("[BUILD] 빌드 설정에 씬이 없습니다. 먼저 프로젝트 셋업을 실행하세요.");
                return;
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            var summary = BuildPipeline.BuildPlayer(options).summary;

            Debug.Log($"[BUILD] 결과={summary.result} 에러={summary.totalErrors} 경고={summary.totalWarnings} " +
                      $"크기={summary.totalSize / (1024 * 1024)}MB 시간={summary.totalTime}");

            if (summary.result != BuildResult.Succeeded)
                Debug.LogError("[BUILD] 빌드 실패");
        }
    }
}
