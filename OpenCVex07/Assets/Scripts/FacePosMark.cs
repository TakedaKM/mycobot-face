//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using UnityEngine;
//using UnityEngine.UI;
//using OpenCvSharp;
//using OpenCvSharp.Demo;

//public class FacePosMark : WebCamera
//{
//	string serverIP = "172.16.0.54";
//	//IPAddress serverIPAddress;
//	const int serverPort = 11001;
//	//const int port = 50000;
//	UdpClient client;
//	public Image mask;
//	public bool isPaused = false;
//	List<float> posData;
 

//	//private UdpClient client;

//	public TextAsset faces;
//	public TextAsset eyes;
//	public TextAsset shapes;
//	private CascadeClassifier cascadeFaces;

//	private FaceProcessorLive<WebCamTexture> processor;

//    /// <summary>
//    /// Default initializer for MonoBehavior sub-classes
//    /// </summary>
//    ///



//	protected override void Awake()
//	{
//		//posData


//		// udpClient = new IPEndPoint(IPAddress.Any, port);
//		//IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
//		//UDPクライアントソケットの作成
//		//client = new UdpClient();


//		base.Awake();
//		base.forceFrontalCamera = true; // we work with frontal cams here, let's force it for macOS s MacBook doesn't state frontal cam correctly

//		cascadeFaces = new CascadeClassifier();
//		byte[] shapeDat = shapes.bytes;
//		if (shapeDat.Length == 0)
//		{
//			string errorMessage =
//				"In order to have Face Landmarks working you must download special pre-trained shape predictor " +
//				"available for free via DLib library website and replace a placeholder file located at " +
//				"\"OpenCV+Unity/Assets/Resources/shape_predictor_68_face_landmarks.bytes\"\n\n" +
//				"Without shape predictor demo will only detect face rects.";

//#if UNITY_EDITOR
//			// query user to download the proper shape predictor
//			if (UnityEditor.EditorUtility.DisplayDialog("Shape predictor data missing", errorMessage, "Download", "OK, process with face rects only"))
//				Application.OpenURL("http://dlib.net/files/shape_predictor_68_face_landmarks.dat.bz2");
//#else
//             UnityEngine.Debug.Log(errorMessage);
//#endif
//		}

//		processor = new FaceProcessorLive<WebCamTexture>();
//		processor.Initialize(faces.text, eyes.text, shapes.bytes);

//		// data stabilizer - affects face rects, face landmarks etc.
//		processor.DataStabilizer.Enabled = true;        // enable stabilizer
//		processor.DataStabilizer.Threshold = 2.0;       // threshold value in pixels
//		processor.DataStabilizer.SamplesCount = 2;      // how many samples do we need to compute stable data

//		// performance data - some tricks to make it work faster
//		processor.Performance.Downscale =  1024;          // processed image is pre-scaled down to N px by long side
//		processor.Performance.SkipRate = 0;             // we actually process only each Nth frame (and every frame for skipRate = 0)
//	}



//    // void Update()
//    // {
//    //     if (Input.GetKeyDown(KeyCode.Space))
//    //     {
//    //isPaused = !isPaused;
//    //string ms = isPaused == true ? "stop" : "go";
//    //Debug.Log(isPaused);
//    //Debug.Log(ms);
//    //     }
//    // }

//    public void IsPause()
//    {
//		isPaused = !isPaused;
//		string ms = isPaused == true ? "stop" : "go";
//		Debug.Log(isPaused);
//		Debug.Log(ms);
//	}


//	/// <summary>
//	/// Per-frame video capture processor
//	/// </summary>
//	protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
//	{
//		if (isPaused)
//		{
//			Debug.Log("処理中断中");
//			return true;
//		}

//		// detect everything we're interested in
//		processor.ProcessTexture(input, TextureParameters);


//		// 検出された顔の中心座標を取得し続ける
//		//Debug.Log(processor.Faces.Count);

//		for (int i = 0; i < processor.Faces.Count; i++)
//		{
//			// mark detected objects
//			processor.MarkDetected();

//			var face = processor.Faces[i];

//			// 顔の中心座標を計算
//			float centerX = face.Region.X + (face.Region.Width / 2f);
//			float centerY = face.Region.Y + (face.Region.Height / 2f);

//			if (centerX == 0 && centerY == 0) continue;

//			float transformedX = ConvertXPosition(centerX);
//			float transformedY = ConvertYPosition(centerY);
//			float transformedZ = (Math.Abs(transformedX) < 140) ? ConvertZPosition(transformedX) : 170;

//			//float[] arrayPosX = { transformedX, transformedY, transformedZ };

//			//EqualizationPos(arrayPos);


//			Send(transformedX, transformedY, transformedZ);
		
//		}

//		// processor.Image now holds data we'd like to visualize
//		output = OpenCvSharp.Unity.MatToTexture(processor.Image, output);   // if output is valid texture it's buffer will be re-used, otherwise it will be re-created

//		return true;
//	}







//	float ConvertZPosition(float x)
//    {
//		float result = Math.Abs(1.0f / 1.4f * x);
//		return result;
//	}


//	float ConvertXPosition(float x)
//    {
//		float result = (x - 320.0f) / 1.3333f;
//		return result;
//    }

//	float ConvertYPosition(float y)
//	{
//		float result = (y - 100.0f) / 0.71428f;
//		if(result < 320)
//        {
//			Debug.Log("Y座標" + result);
//			return 320;
//        }
//		return result;
//	}

//	//public float[] EqualizationPos(float x, float y, float z)
// //   {
//	//	return 
// //   }


//	public void Send(float x, float y, float z)
//	{
//		client = new UdpClient();
//		string message = (x + "/" + y + "/" + z + "/-90/0/170/100");
//		Debug.Log(message);
//		byte[] data = Encoding.ASCII.GetBytes(message);
//		client.Send(data, data.Length, serverIP, serverPort);

//		client.Close();
//	}

	



	 
//	//private void OnApplicationQuit()
//	//{
		
//	//	Debug.Log("ソケットを閉じる");
//	//}
//}