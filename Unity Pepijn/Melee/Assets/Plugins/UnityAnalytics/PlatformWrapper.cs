//Unity 4.5 and above switched WWW to use Dictionary instead of Hashtable
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 
#define UNITY_USE_WWW_HASHTABLE
#endif


#if UNITY_IPHONE || UNITY_ANDROID || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_METRO
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.Cloud.Analytics
{
	internal static class PlatformWrapper
	{
		public static IPlatformWrapper platform
		{
			get {
				#if UNITY_ANDROID && !UNITY_EDITOR
				return new AndroidWrapper();
				#elif UNITY_IPHONE && !UNITY_EDITOR
				return new iOSWrapper();
				#elif UNITY_WEBGL && !UNITY_EDITOR
				return new WebGLWrapper();
				#else
				return new BasePlatformWrapper();
				#endif
			}
		}
	}
	
	internal class BasePlatformWrapper : IPlatformWrapper, IWWWFactory
	{
		private System.Random m_Random;

		internal BasePlatformWrapper()
		{
			m_Random = new System.Random();
		}

		#region IPlatformWrapper
		public virtual string appVersion
		{
			get { return null; }
		}
		
		public virtual string appBundleIdentifier
		{
			get { return null; }
		}
		
		public virtual string appInstallMode
		{
			get { return null; }
		}
		
		public virtual bool isRootedOrJailbroken
		{
			get { return false; }
		}
		#endregion

		#region IApplication
		public virtual string deviceMake
		{
			get { return Application.platform.ToString(); }
		}

		public virtual bool isNetworkReachable
		{
			get { return Application.internetReachability != NetworkReachability.NotReachable; }
		}

		public virtual bool isWebPlayer
		{
			get { return Application.isWebPlayer; }
		}

		public virtual bool isAndroidPlayer
		{
			get { return Application.platform == RuntimePlatform.Android; }
		}

		public virtual bool isIPhonePlayer
		{
			get { return Application.platform == RuntimePlatform.IPhonePlayer; }
		}

		public virtual bool isWebGLPlayer
		{
			get 
			{
				#if UNITY_WEBGL     
				return Application.platform == RuntimePlatform.WebGLPlayer;
				#else	
				return false;
				#endif
			}
		}

		public virtual bool isEditor
		{
			get { return Application.isEditor; }
		}

		public virtual int levelCount
		{
			get { return Application.levelCount; }
		}

		public virtual int loadedLevel
		{
			get { return Application.loadedLevel; }
		}

		public virtual string loadedLevelName
		{
			get { return Application.loadedLevelName; }
		}

		public virtual string persistentDataPath
		{
			get { return Application.persistentDataPath; }
		}

		public virtual string platformName
		{
			get {
				return Application.platform.ToString();
			}
		}

		public virtual string unityVersion
		{
			get { return Application.unityVersion; }
		}
		#endregion

		#region ISystemInfo
		public long GetLongRandom()
		{
			var buffer = new byte[8];
			m_Random.NextBytes(buffer);
			return (long)(System.BitConverter.ToUInt64(buffer, 0) & System.Int64.MaxValue);
		}
		#endregion

		#region ISystemInfo
		public virtual string deviceModel
		{
			get { return SystemInfo.deviceModel; }
		}

		public virtual string deviceUniqueIdentifier
		{
			get { 
				#if UNITY_ANDROID && !UNITY_EDITOR
				return "";
				#else
				return SystemInfo.deviceUniqueIdentifier; 
				#endif
			}
		}

		public virtual string operatingSystem
		{
			get { return SystemInfo.operatingSystem; }
		}

		public virtual string processorType
		{
			get { return SystemInfo.processorType; }
		}

		public virtual int systemMemorySize
		{
			get { return SystemInfo.systemMemorySize; }
		}
		#endregion

		#if UNITY_USE_WWW_HASHTABLE
		public IWWW newWWW(string url, byte[] body, Dictionary<string, string> headers)
		{
			WWW www = new WWW(url, body, DictToHash(headers));
			return new UnityWWW(www);
		}
		
		private Hashtable DictToHash(Dictionary<string, string> headers)
		{
			var result = new Hashtable();
			foreach (var kvp in headers)
				result[kvp.Key] = kvp.Value;
			return result;
		}
		#else
		public IWWW newWWW(string url, byte[] body, Dictionary<string, string> headers)
		{
			WWW www = new WWW(url, body, headers);
			return new UnityWWW(www);
		}
		#endif
		
	}
	
}
#endif