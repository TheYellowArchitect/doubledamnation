using UnityEditor;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.UnityEditor
{
	public class ForgeNetworkingAboutEditor : EditorWindow
	{
		#region Autocheck
		[InitializeOnLoad]
		private class RuntimeCheck
		{
			static RuntimeCheck()
			{
				//ForgeNetworkingAboutEditor.Init();
			}
		}
		#endregion

		#region Constants
		private static ForgeNetworkingAboutEditor _instance;
		private const string EDITOR_PREF_DATE = "FNR_CHECK_DATE";
		private const string EDITOR_PREF_IGNORE = "FNR_IGNORE";
		private const string EDITOR_ASSET_STORE_LINK = "https://assetstore.unity.com/packages/slug/38344";
		private const string EDITOR_GITHUB_LINK = "https://github.com/BeardedManStudios/ForgeNetworkingRemastered";
		private const string EDITOR_DISCORD_LINK = "https://discord.gg/yzZwEYm";
		private const string EDITOR_FORUM_LINK = "https://forum.unity3d.com/threads/no-ccu-limit-forge-networking-superpowered-fully-cross-platform.286900/";
		private string Version { get { return Resources.Load<TextAsset>(ForgeNetworkingEditor.EDITOR_RESOURCES_DIR + "/version").text; } }

		//private static bool ProVersion = false;
		private static bool IgnoreEditorStartup = false;
		#endregion

		#region Private
		private Texture2D _icon;
		private Vector2 _scroll;
		#endregion

		private static void Init()
		{
			//Commented the fuck out of this popup annoyance.
		}
	}
}
