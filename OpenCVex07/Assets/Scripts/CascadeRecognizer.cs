using OpenCvSharp;
using OpenCvSharp.Demo;
using UnityEngine;
using UnityEngine.UI;

public class CascadeRecognizer : WebCamera
{
    public TextAsset faces;
    public Image mask;
    public RectTransform canvasRectTransform;
    private CascadeClassifier cascadeFaces;

    protected override void Awake()
    {
        base.Awake();

        // classifier
        FileStorage storageFaces = new FileStorage(faces.text, FileStorage.Mode.Read | FileStorage.Mode.Memory);
        cascadeFaces = new CascadeClassifier();
        if (!cascadeFaces.Read(storageFaces.GetFirstTopLevelNode()))
        {
            throw new System.Exception("FaceProcessor.Initialize: Failed to load faces cascade classifier");
        }
    }

    protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
    {
        Mat image = OpenCvSharp.Unity.TextureToMat(input);
        Mat gray = image.CvtColor(ColorConversionCodes.BGR2GRAY);
        Mat equalizeHistMat = new Mat();
        Cv2.EqualizeHist(gray, equalizeHistMat);
        OpenCvSharp.Rect[] rawFaces = cascadeFaces.DetectMultiScale(gray, 1.1, 6);
        for (int i = 0; i < rawFaces.Length; i++)
        {
            //Cv2.Rectangle((InputOutputArray)image, rawFaces[i], Scalar.LightGreen, 2);

            //顔検出位置の座標の計算
            var cx = rawFaces[i].TopLeft.X + (rawFaces[i].Width / 2f);
            var cy = rawFaces[i].TopLeft.Y + (rawFaces[i].Height / 2f);

            //顔検出された座標をカメラ画像のViewport座標系に変換
            Vector2 viewportPos = new Vector2(cx / gray.Width, 1 - cy / gray.Height);

            //Viewport座標をCanvas内の座標に変換
            Vector2 canvasPos = new Vector2(viewportPos.x * canvasRectTransform.sizeDelta.x, viewportPos.y * canvasRectTransform.sizeDelta.y);

            mask.GetComponent<RectTransform>().position = canvasPos;
        }
        output = OpenCvSharp.Unity.MatToTexture(image);
        return true;
    }
}