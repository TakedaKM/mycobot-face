using OpenCvSharp;
using OpenCvSharp.Demo;
using UnityEngine;
using UnityEngine.UI;

public class FacePos : WebCamera
{
    public TextAsset faces;
    public Image mask;
    public RectTransform canvasRectTransform;
    private CascadeClassifier cascadeFaces;

    protected override void Awake()
    {
        base.Awake();

        FileStorage storageFaces = new FileStorage(faces.text, FileStorage.Mode.Read | FileStorage.Mode.Memory);

        //  Initialize face detection
        cascadeFaces = new CascadeClassifier();
        //    if(!cascadeFaces.Load("Assets/OpenCV+Unity/Demo/Face_Detector/haarcascade_frontalface_default"))
        //    {
        //        Debug.LogError("Failed to load cascade classifier for faces!");
        //    }

        if (!cascadeFaces.Read(storageFaces.GetFirstTopLevelNode()))
        {
            throw new System.Exception("FaceProcessor.Initialize: Failed to load faces cascade classifier");
        }
    }



    protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
    {
        // Change Mat
        Mat image = OpenCvSharp.Unity.TextureToMat(input);

        // Change Gray
        Mat gray = image.CvtColor(ColorConversionCodes.BGR2GRAY);

        Mat equalizeHisMat = new Mat();

        // Detect Face
        OpenCvSharp.Rect[] rawFaces = cascadeFaces.DetectMultiScale(gray, 1.1, 6);

        //
        for(int i = 0; i < rawFaces.Length; i++)
        {
            // FacePos
            float centerX = rawFaces[i].X + (rawFaces[i].Width / 2f);
            float centerY = rawFaces[i].Y + (rawFaces[i].Height / 2f);

            // Disp
            Vector2 screenPos = new Vector2(centerX / gray.Width, centerY / gray.Height);

            Debug.Log("Face " + i + " position: " + screenPos);

            //Viewport座標をCanvas内の座標に変換
            Vector2 canvasPos = new Vector2(screenPos.x * canvasRectTransform.sizeDelta.x, screenPos.y * canvasRectTransform.sizeDelta.y);

            mask.GetComponent<RectTransform>().position = canvasPos;

        }

        // OpenCV MAT to Texture2D
        output = OpenCvSharp.Unity.MatToTexture(image);

        return true;
    }

    
}
