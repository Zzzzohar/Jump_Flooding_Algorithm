using UnityEngine;

public class JumpFloodingAlgorithm : MonoBehaviour
{
    public ComputeShader jumpFloodingCS;

    public Texture2D initTex;
    public Material _mat;
    public Material _distanceField;
    public Material _mat3;

    public string savePath = "Assets/JumpFloodingAlgorithm/SavedDistanceField/";
    
    private int size;
    private int initJFAUVKernel;
    private int jfaIterationPassKernel;
    private Vector2Int dispatchCount;
    private RenderTexture jfaUVRT;
    private RenderTexture jfaUVRT2;
    
    
    // Start is called before the first frame update
    void Start()
    {
        DoJFA();
    }

    public void SaveDistanceField()
    {
        DoJFA(true);
    }
    public void DoJFA(bool save = false)
    {
        size = initTex.width;
        
        jfaUVRT = CreateRenderTexture(RenderTextureFormat.ARGBFloat);
        
        initJFAUVKernel = jumpFloodingCS.FindKernel("InitJFAUV");
        jumpFloodingCS.SetInt("size",size);
        jumpFloodingCS.SetTexture(initJFAUVKernel,"InitTex",initTex);
        jumpFloodingCS.SetTexture(initJFAUVKernel,"JFAUV",jfaUVRT);
        
        uint threadX = 0;
        uint threadY = 0;
        uint threadZ = 0;
        jumpFloodingCS.GetKernelThreadGroupSizes(initJFAUVKernel, out threadX, out threadY, out threadZ);
        dispatchCount.x = Mathf.CeilToInt(size / threadX);
        dispatchCount.y = Mathf.CeilToInt(size / threadY);
        jumpFloodingCS.Dispatch(initJFAUVKernel,dispatchCount.x,dispatchCount.y,1);
        
        jfaUVRT2 = CreateRenderTexture(RenderTextureFormat.ARGBFloat);
        
        
        jfaIterationPassKernel = jumpFloodingCS.FindKernel("JFAIterationPass");
        jumpFloodingCS.GetKernelThreadGroupSizes(jfaIterationPassKernel, out threadX, out threadY, out threadZ);
        dispatchCount.x = Mathf.CeilToInt(size / threadX);
        dispatchCount.y = Mathf.CeilToInt(size / threadY);
        int maxIteration = (int)Mathf.Log(size / 2,2 );
        Debug.Log(maxIteration);
        for(int i = 1; i <= maxIteration; i++)
        {
            float StepSize = 1 / Mathf.Pow(2,i);
            jumpFloodingCS.SetFloat("StepSize",StepSize);
            if(i % 2 != 1)
            {
                jumpFloodingCS.SetTexture(jfaIterationPassKernel,"JFAUV",jfaUVRT2);
                jumpFloodingCS.SetTexture(jfaIterationPassKernel,"Output",jfaUVRT);
                VisualizeJFA(jfaUVRT, true);

            }
            else
            {
                jumpFloodingCS.SetTexture(jfaIterationPassKernel,"JFAUV",jfaUVRT);
                jumpFloodingCS.SetTexture(jfaIterationPassKernel,"Output",jfaUVRT2);
                VisualizeJFA(jfaUVRT2, true);
            }
            jumpFloodingCS.Dispatch(jfaIterationPassKernel,dispatchCount.x,dispatchCount.y,1);
        }
    }

    public void VisualizeJFA(RenderTexture rt, bool save = false)
    {
        _mat.SetTexture("_MainTex",rt);
        _distanceField.SetTexture("_MainTex",rt);
        _mat3.SetTexture("_MainTex",rt);
        if (save)
        {
            RenderTexture distanceFieldRT = CreateRenderTexture(RenderTextureFormat.ARGBFloat);
            Graphics.Blit(rt,distanceFieldRT,_distanceField);
            SaveRenderTextureToPNG(distanceFieldRT, "DistanceField");
        }
    }

    
    
    
    public void SaveRenderTextureToPNG(RenderTexture rt, string name)
    {
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBAFloat, false);
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        //如果没找到文件夹先创建一个
        if (!System.IO.Directory.Exists(Application.dataPath + "/JumpFloodingAlgorithm/SavedDistanceField/"))
        {
            System.IO.Directory.CreateDirectory(Application.dataPath + "/JumpFloodingAlgorithm/SavedDistanceField/");
        }
        System.IO.File.WriteAllBytes(Application.dataPath + "/JumpFloodingAlgorithm/SavedDistanceField/" + name + ".png", bytes);
    }
    
    public RenderTexture CreateRenderTexture(RenderTextureFormat format)
    {
        RenderTexture texture = new RenderTexture(size, size, 0, format);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.enableRandomWrite = true;
        texture.Create();
        return texture;
    }
    
}
