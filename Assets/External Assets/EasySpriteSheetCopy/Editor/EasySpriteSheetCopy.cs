using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class EasySpriteSheetCopy{
	
	private class CopySpriteClipboard{
		public bool isSpriteType = false;
		public bool clipboardSet = false;
		public string copyType;

		public TextureImporter spriteImporter;
		public TextureImporterSettings spriteSettings;
		public List<SpriteMetaData> spriteData;
	}

	private class PlatformTextureSettings{
		public string spritePlatform;
		public int spriteMaxTextureSize;
		public TextureImporterFormat spriteTextureFormat;
		public int spriteCompressionQuality;
	}
	
	#region Copy Method
	private static CopySpriteClipboard clipboard = new CopySpriteClipboard();
	private static List<PlatformTextureSettings> platformTextureSettings = new List<PlatformTextureSettings>();
	private static string[] availablePlatforms = new string[]{"Web", "Standalone", "iPhone", "Android", "FlashPlayer" ,"Windows Store Apps" , "WP8", "BlackBerry"};

	[MenuItem("CONTEXT/TextureImporter/Copy All Sprite Sheet Settings", false,150)]
	private static void CopySpriteTextureSettings(MenuCommand command){
		
		//Grab current Texture Importer
		clipboard.spriteImporter = command.context as TextureImporter;
		
		//Initiate Sprite Data List
		clipboard.spriteData = new List<SpriteMetaData>();

		//Copy sprite meta data
		foreach (SpriteMetaData metaData in clipboard.spriteImporter.spritesheet){
			SpriteMetaData tempMeta = new SpriteMetaData();
			tempMeta.name = metaData.name;
			tempMeta.rect = metaData.rect;
			tempMeta.pivot = metaData.pivot;
			tempMeta.alignment = metaData.alignment;
			tempMeta.border = metaData.border;
			clipboard.spriteData.Add(tempMeta);
		}

		//Copy all platform specific overrides
		foreach(string tempString in availablePlatforms){
			int tempTextureSize;
			TextureImporterFormat tempFormat;
			int tempCompressSize;

			if(clipboard.spriteImporter.GetPlatformTextureSettings(tempString,out tempTextureSize,out tempFormat,out tempCompressSize)){

				PlatformTextureSettings tempPlatSettings = new PlatformTextureSettings();
				tempPlatSettings.spritePlatform = tempString;
				tempPlatSettings.spriteMaxTextureSize = tempTextureSize;
				tempPlatSettings.spriteTextureFormat = tempFormat;
				tempPlatSettings.spriteCompressionQuality = tempCompressSize;

				platformTextureSettings.Add(tempPlatSettings);
			}
		}

		//Initiate our Settings grabber
		TextureImporterSettings tempSpriteSettings = new TextureImporterSettings();
		
		//Grab Settings
		clipboard.spriteImporter.ReadTextureSettings(tempSpriteSettings);
		
		//Assign settings to public vars
		clipboard.spriteSettings = tempSpriteSettings;
		
		//Let validator know we have data
		clipboard.clipboardSet = true;
		clipboard.copyType = "AllData";
	}

	[MenuItem("CONTEXT/TextureImporter/Copy All Except Overrides", false,151)]
	private static void CopySpriteExceptOverride(MenuCommand command){
		
		//Grab current Texture Importer
		clipboard.spriteImporter = command.context as TextureImporter;
		
		//Initiate Sprite Data List
		clipboard.spriteData = new List<SpriteMetaData>();

		//Copy sprite meta data
		foreach (SpriteMetaData metaData in clipboard.spriteImporter.spritesheet){
			SpriteMetaData tempMeta = new SpriteMetaData();
			tempMeta.name = metaData.name;
			tempMeta.rect = metaData.rect;
			tempMeta.pivot = metaData.pivot;
			tempMeta.alignment = metaData.alignment;
			tempMeta.border = metaData.border;
			clipboard.spriteData.Add(tempMeta);
		}
		
		//Initiate our Settings grabber
		TextureImporterSettings tempSpriteSettings = new TextureImporterSettings();
		
		//Grab Settings
		clipboard.spriteImporter.ReadTextureSettings(tempSpriteSettings);
		
		//Assign settings to public vars
		clipboard.spriteSettings = tempSpriteSettings;
		
		//Let validator know we have data
		clipboard.clipboardSet = true;
		clipboard.copyType = "AllDataNoOverride";
	}

	[MenuItem("CONTEXT/TextureImporter/Copy Only Overrides", false,151)]
	private static void CopyOverride(MenuCommand command){

		//Grab current Texture Importer
		clipboard.spriteImporter = command.context as TextureImporter;

		//Copy all platform specific overrides
		foreach(string tempString in availablePlatforms){
			int tempTextureSize;
			TextureImporterFormat tempFormat;
			int tempCompressSize;
			
			if(clipboard.spriteImporter.GetPlatformTextureSettings(tempString,out tempTextureSize,out tempFormat,out tempCompressSize)){
				
				PlatformTextureSettings tempPlatSettings = new PlatformTextureSettings();
				tempPlatSettings.spritePlatform = tempString;
				tempPlatSettings.spriteMaxTextureSize = tempTextureSize;
				tempPlatSettings.spriteTextureFormat = tempFormat;
				tempPlatSettings.spriteCompressionQuality = tempCompressSize;
				
				platformTextureSettings.Add(tempPlatSettings);
			}
		}
		
		//Let validator know we have data
		clipboard.clipboardSet = true;
		clipboard.copyType = "OnlyOverride";
	}
	#endregion
	
	#region Paste Method
	[MenuItem("CONTEXT/TextureImporter/Paste Copied Settings", false,200)]
	private static void PasteSpriteTextureSettings(MenuCommand command){
		//Grab current Texture Importer
		TextureImporter currentTexture = command.context as TextureImporter;
		


		//Copy over platform specific settings
		switch (clipboard.copyType){
		//***************************************
		case "AllData":
			//Copy over sprites
			currentTexture.spritesheet = clipboard.spriteData.ToArray();
			
			//Copy over settings
			currentTexture.SetTextureSettings(clipboard.spriteSettings);

			//Copy over overrides
			foreach(string tempString in availablePlatforms){
				#if UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1
					currentTexture.SetPlatformTextureSettings(tempString,1024,TextureImporterFormat.AutomaticCompressed,0);
				#else
					currentTexture.SetPlatformTextureSettings(tempString,1024,TextureImporterFormat.AutomaticCompressed,0, true);
				#endif
				currentTexture.ClearPlatformTextureSettings(tempString);
			}
			foreach (PlatformTextureSettings tempPlatSettings in platformTextureSettings){
				#if UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_0
					currentTexture.SetPlatformTextureSettings(tempPlatSettings.spritePlatform,tempPlatSettings.spriteMaxTextureSize,tempPlatSettings.spriteTextureFormat,tempPlatSettings.spriteCompressionQuality);
				#else
					currentTexture.SetPlatformTextureSettings(tempPlatSettings.spritePlatform,tempPlatSettings.spriteMaxTextureSize,tempPlatSettings.spriteTextureFormat,tempPlatSettings.spriteCompressionQuality, true);
				#endif
				
			}
			break;
		
		//***************************************
		case "AllDataNoOverride":
			//Copy over sprites
			currentTexture.spritesheet = clipboard.spriteData.ToArray();
			
			//Copy over settings
			currentTexture.SetTextureSettings(clipboard.spriteSettings);
			break;

		//***************************************
		case "OnlyOverride":
			//Copy over overrides
			foreach(string tempString in availablePlatforms){
				#if UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1
					currentTexture.SetPlatformTextureSettings(tempString,1024,TextureImporterFormat.AutomaticCompressed,0);
				#else
					currentTexture.SetPlatformTextureSettings(tempString,1024,TextureImporterFormat.AutomaticCompressed,0, true);
				#endif
				
				currentTexture.ClearPlatformTextureSettings(tempString);
			}
			foreach (PlatformTextureSettings tempPlatSettings in platformTextureSettings){
				#if UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1
					currentTexture.SetPlatformTextureSettings(tempPlatSettings.spritePlatform,tempPlatSettings.spriteMaxTextureSize,tempPlatSettings.spriteTextureFormat,tempPlatSettings.spriteCompressionQuality);
				#else
					currentTexture.SetPlatformTextureSettings(tempPlatSettings.spritePlatform,tempPlatSettings.spriteMaxTextureSize,tempPlatSettings.spriteTextureFormat,tempPlatSettings.spriteCompressionQuality, true);
				#endif
			}
			break;
		}

		//Refresh asset/apply settings
		AssetDatabase.ImportAsset(currentTexture.assetPath, ImportAssetOptions.ForceUpdate);
	}
	#endregion

	#region Clear Methods
	[MenuItem("CONTEXT/TextureImporter/Reset Overrides", false,300)]
	private static void ClearOverrides(MenuCommand command){
		//Grab current Texture Importer
		TextureImporter currentTexture = command.context as TextureImporter;

		//Clear overrides from current texture
		foreach(string tempString in availablePlatforms){
			#if UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1
				currentTexture.SetPlatformTextureSettings(tempString,1024,TextureImporterFormat.AutomaticCompressed,0);
			#else
				currentTexture.SetPlatformTextureSettings(tempString,1024,TextureImporterFormat.AutomaticCompressed,0,true);
			#endif
			currentTexture.ClearPlatformTextureSettings(tempString);
		}

		//Refresh asset/apply settings
		AssetDatabase.ImportAsset(currentTexture.assetPath, ImportAssetOptions.ForceUpdate);
	}


	[MenuItem("CONTEXT/TextureImporter/Reset Sprite Settings (Excludes Overrides)", false,301)]
	private static void ClearSpriteSettings(MenuCommand command){
		//Grab current Texture Importer
		TextureImporter currentTexture = command.context as TextureImporter;
		
		//Reset
		currentTexture.spriteImportMode = SpriteImportMode.Single;
		currentTexture.mipmapEnabled = true;
		currentTexture.spritePivot = Vector2.zero;
		#if UNITY_4_5
		    currentTexture.spritePixelsToUnits = 100;
		#else
		    currentTexture.spritePixelsPerUnit = 100;
		#endif
		currentTexture.filterMode = FilterMode.Bilinear;
		currentTexture.maxTextureSize = 1024;
		currentTexture.textureFormat = TextureImporterFormat.AutomaticCompressed;

		//Refresh asset/apply settings
		AssetDatabase.ImportAsset(currentTexture.assetPath, ImportAssetOptions.ForceUpdate);
	}
	#endregion
	
	#region Validation
	[MenuItem("CONTEXT/TextureImporter/Copy All Sprite Sheet Settings", true)]
	[MenuItem("CONTEXT/TextureImporter/Copy All Except Overrides", true)]
	[MenuItem("CONTEXT/TextureImporter/Copy Only Overrides", true)]
	[MenuItem("CONTEXT/TextureImporter/Reset Overrides", true)]
	[MenuItem("CONTEXT/TextureImporter/Reset Sprite Settings (Excludes Overrides)", true)]
	static bool ValidateTextureType(MenuCommand command){
		CopySpriteClipboard tempClipboard = new CopySpriteClipboard();
		tempClipboard.spriteImporter = command.context as TextureImporter;
        if (tempClipboard.spriteImporter.textureType == TextureImporterType.Sprite ||
            tempClipboard.spriteImporter.textureType == TextureImporterType.Default) {
            return true;
        } else {
            return false;
        }
    }

	[MenuItem("CONTEXT/TextureImporter/Paste Copied Settings", true)]
	static bool ValidateClipboard(MenuCommand command){
		return clipboard.clipboardSet;
	}
	#endregion
}