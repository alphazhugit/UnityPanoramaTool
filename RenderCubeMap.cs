using UnityEngine;
using UnityEditor;
using System.IO;

public class RenderCubeMap : EditorWindow
{
    [SerializeField]
    static int clickCount = 0;

    [MenuItem("Tools/RenderCubeMap/Render", false, 11)]
    static void Init()
    {
        clickCount++;

        var sceneViewCam = SceneView.lastActiveSceneView.camera;
        var camPos = sceneViewCam.transform.position;
        var camRot = sceneViewCam.transform.rotation;

        var camGo = new GameObject("tmpCam", typeof(Camera));

        camGo.hideFlags = HideFlags.HideInHierarchy;
        var cam = camGo.GetComponent<Camera>();
        cam.hideFlags = HideFlags.HideAndDontSave;
        cam.fieldOfView = 45;
        cam.farClipPlane = 4000;
        cam.allowMSAA = false;

        //Debug.Log(sceneViewCam.transform.position + " === " + sceneViewCam.transform.rotation);

        camGo.transform.position = camPos;
        camGo.transform.rotation = camRot;

        RenderToCubeMap(cam);
    }

    [MenuItem("Tools/RenderCubeMap/Clean")]
    static void CleanAll()
    {
        clickCount = 0;
        var allTmpCamera = Resources.FindObjectsOfTypeAll(typeof(Camera));
        foreach(Camera c in allTmpCamera)
        {
            if (c.name == "tmpCam")
            {
                DestroyImmediate(c.gameObject, false);
            }
        }
    }

    static void RenderToCubeMap(Camera Cam)
    {
        //var cam = Cam.GetComponent<Camera>();

        var renderTexCube = new RenderTexture(4096, 4096, 32, RenderTextureFormat.ARGB32);
        var renderTex2D = new RenderTexture(4096, 2048, 32, RenderTextureFormat.ARGB32);

        renderTexCube.dimension = UnityEngine.Rendering.TextureDimension.Cube;
        renderTexCube.Create();
        renderTex2D.Create();
        //var cubeSavePath = "Assets/RenderCubeMap/RTCube.renderTexture";
        //var texSavePath = "Assets/RenderCubeMap/RT2D.renderTexture";
        //var tex2DSavePath = @"C:\Users\Alpha\Desktop\Can Clear\20182_temp\Assets\RenderCubeMap\Tex2D.jpg";
        var tex2DSavePath = Application.dataPath + "/Game/Editor/RenderCubeMap/360tex" + "_" + clickCount +  ".jpg";

        //AssetDatabase.CreateAsset(renderTexCube, cubeSavePath);
        //AssetDatabase.CreateAsset(renderTex2D, texSavePath);

        Cam.RenderToCubemap(renderTexCube,63, Camera.MonoOrStereoscopicEye.Mono);
        renderTexCube.ConvertToEquirect(renderTex2D, Camera.MonoOrStereoscopicEye.Mono);

        Texture2D tex2d = new Texture2D(4096, 2048, TextureFormat.RGB24,false);
        RenderTexture.active = renderTex2D;
        tex2d.ReadPixels(new Rect(0,0,renderTex2D.width, renderTex2D.height),0,0);
        tex2d.Apply();
        DestroyImmediate(renderTex2D, true);
        DestroyImmediate(renderTexCube, true);

        byte[] bytes = tex2d.EncodeToJPG();
        DestroyImmediate(tex2d, true);

        File.WriteAllBytes(tex2DSavePath, bytes);

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Tips","如果全部渲染完毕，记得执行清理按钮!","OK");
    }
}
