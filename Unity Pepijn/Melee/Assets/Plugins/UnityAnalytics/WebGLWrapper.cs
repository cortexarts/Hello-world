#if UNITY_WEBGL && !UNITY_EDITOR
namespace UnityEngine.Cloud.Analytics
{
	internal class WebGLWrapper : BasePlatformWrapper
	{
		public override string deviceUniqueIdentifier
		{
			get 
			{ 
				return System.Guid.NewGuid ().ToString();
			}
		}
	}
}
#endif