using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;
using OpenCvSharp.Demo;
using System.Linq;

public class FacePosAngleMove : WebCamera
{
	string serverIP = "192.168.111.109";
	const int serverPort = 11001;
	UdpClient client;
	public Image mask;
	public bool isPaused = false;
	public List<float> xPosList;
	public List<float> JPos;
	private float PassXPos = 0;


	public TextAsset faces;
	public TextAsset eyes;
	public TextAsset shapes;
	private CascadeClassifier cascadeFaces;

	private FaceProcessorLive<WebCamTexture> processor;

	/// <summary>
	/// Default initializer for MonoBehavior sub-classes
	/// </summary>
	///



	protected override void Awake()
	{
		xPosList = new List<float> { };
		JPos = new List<float> { };

		base.Awake();
		base.forceFrontalCamera = true; // we work with frontal cams here, let's force it for macOS s MacBook doesn't state frontal cam correctly

		cascadeFaces = new CascadeClassifier();
		byte[] shapeDat = shapes.bytes;
		if (shapeDat.Length == 0)
		{
			string errorMessage =
				"In order to have Face Landmarks working you must download special pre-trained shape predictor " +
				"available for free via DLib library website and replace a placeholder file located at " +
				"\"OpenCV+Unity/Assets/Resources/shape_predictor_68_face_landmarks.bytes\"\n\n" +
				"Without shape predictor demo will only detect face rects.";

#if UNITY_EDITOR
			// query user to download the proper shape predictor
			if (UnityEditor.EditorUtility.DisplayDialog("Shape predictor data missing", errorMessage, "Download", "OK, process with face rects only"))
				Application.OpenURL("http://dlib.net/files/shape_predictor_68_face_landmarks.dat.bz2");
#else
             UnityEngine.Debug.Log(errorMessage);
#endif
		}

		processor = new FaceProcessorLive<WebCamTexture>();
		processor.Initialize(faces.text, eyes.text, shapes.bytes);

		// data stabilizer - affects face rects, face landmarks etc.
		processor.DataStabilizer.Enabled = true;        // enable stabilizer
		processor.DataStabilizer.Threshold = 2.0;       // threshold value in pixels
		processor.DataStabilizer.SamplesCount = 2;      // how many samples do we need to compute stable data

		// performance data - some tricks to make it work faster
		processor.Performance.Downscale = 1024;          // processed image is pre-scaled down to N px by long side
		processor.Performance.SkipRate = 0;             // we actually process only each Nth frame (and every frame for skipRate = 0)
	}



	public void IsPause()
	{
		isPaused = !isPaused;
		string ms = isPaused == true ? "stop" : "go";
		Debug.Log(isPaused);
		Debug.Log(ms);
	}


	/// <summary>
	/// Per-frame video capture processor
	/// </summary>
	protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
	{
		if (isPaused)
		{
			Debug.Log("処理中断中");
			return true;
		}

		// detect everything we're interested in
		processor.ProcessTexture(input, TextureParameters);


		// 検出された顔の中心座標を取得し続ける
		//Debug.Log(processor.Faces.Count);

		for (int i = 0; i < processor.Faces.Count; i++)
		{
			
			// mark detected objects
			processor.MarkDetected();

			var face = processor.Faces[i];

			// 顔の中心座標を計算
			float centerX = face.Region.X + (face.Region.Width / 2f);
			float centerY = face.Region.Y + (face.Region.Height / 2f);

			if (centerX == 0 && centerY == 0) continue;

			float transformedX = ConvertXPosition(centerX);
			if (transformedX == PassXPos) continue;

			if (Math.Abs(transformedX) < 115)
			{
				JPos = innerCirclePos(transformedX);
            }
            else
            {
				JPos = outCirclePos(transformedX);
			}

			Send(JPos);
			PassXPos = transformedX;
		}

		// processor.Image now holds data we'd like to visualize
		output = OpenCvSharp.Unity.MatToTexture(processor.Image, output);   // if output is valid texture it's buffer will be re-used, otherwise it will be re-created
		
		return true;
	}




	float ConvertXPosition(float x)
	{
		float average = 0;
		float result = (x - 320.0f) / 1.3333f;
		if (result > 200) result = 200;
		if (result < -200) result = -200;

		if(xPosList.Count <= 3)
        {
			xPosList.Add(result);
			average = xPosList.Average();
			return average;
		}

		if(xPosList.Count >= 4)
        {
			xPosList.RemoveAt(0);
			xPosList.Add(result);
			average = xPosList.Average();
			return average;
        }

		return average;
	}


	public List<float> innerCirclePos(float x)
    {
		Debug.Log(x);

		float signValue = 1f;
		if (x < 0) signValue = -1f;

		float value = x * 0.7826087f * signValue;

		float J2 = -value * signValue;
		float J3 = value * signValue;
		float J4 = 0;
		List<float> JPos = new List<float> { J2, J3, J4 };
		return JPos;
    }

	public List<float> outCirclePos(float x)
	{
		Debug.Log(x);
		float signValue = 1f;
		if (x < 0) signValue = -1f;

		float value = (x - 115 * signValue) * signValue * 1.05882353f;

		float J2 = -90 * signValue;
		//float J3 = -(90 - value * signValue);
		float J3 = signValue * (90 - value);

		float J4 = value * signValue;
		List<float> JPos = new List<float> { J2, J3, J4 };
		return JPos;
	}



	public void Send(List<float> JPos)
	{
		client = new UdpClient();
		string message = ("0/" + JPos[0] + "/" + JPos[1] + "/" + JPos[2] + "/-90/0/100");
		Debug.Log(message);
		byte[] data = Encoding.ASCII.GetBytes(message);
		client.Send(data, data.Length, serverIP, serverPort);

		client.Close();
	}
}