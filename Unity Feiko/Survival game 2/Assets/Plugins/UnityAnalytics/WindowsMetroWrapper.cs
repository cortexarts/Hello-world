#if UNITY_METRO && UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public class WindowsCapability {
	static WindowsCapability()
	{
		#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
			PlayerSettings.Metro.SetCapability (PlayerSettings.MetroCapability.InternetClientServer, true);
			PlayerSettings.Metro.SetCapability (PlayerSettings.MetroCapability.InternetClient, true);
		#else
			PlayerSettings.WSA.SetCapability (PlayerSettings.WSACapability.InternetClientServer, true);
			PlayerSettings.WSA.SetCapability (PlayerSettings.WSACapability.InternetClient, true);
		#endif
	}
}
#endif