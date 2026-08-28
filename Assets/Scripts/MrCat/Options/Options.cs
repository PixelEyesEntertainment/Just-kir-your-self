
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Options : MonoBehaviour
{
	// =========================================================
	// AUDIO
	// =========================================================

	[Header("========== AUDIO ==========")]

	[SerializeField] private AudioMixer audioMixer;

	[SerializeField] private Slider masterSlider;
	[SerializeField] private Slider musicSlider;
	[SerializeField] private Slider sfxSlider;
	[SerializeField] private Slider voiceSlider;

	[Header("Audio Mixer Parameters")]
	[SerializeField] private string masterParameter = "Master";
	[SerializeField] private string musicParameter = "Music";
	[SerializeField] private string sfxParameter = "SFX";
	[SerializeField] private string voiceParameter = "Voice";

	private const string MASTER_KEY = "Options_Master";
	private const string MUSIC_KEY = "Options_Music";
	private const string SFX_KEY = "Options_SFX";
	private const string VOICE_KEY = "Options_Voice";


	// =========================================================
	// VIDEO
	// =========================================================

	[Header("========== VIDEO ==========")]

	[Header("Motion Blur")]
	[SerializeField] private Volume postProcessVolume;

	[Header("Ambient Occlusion")]
	[Tooltip("Render Feature مربوط به Ambient Occlusion را اینجا قرار بده.")]
	[SerializeField] private ScriptableRendererFeature ambientOcclusionFeature;

	private const string MOTION_BLUR_KEY = "Options_MotionBlur";
	private const string TEXTURE_QUALITY_KEY = "Options_TextureQuality";
	private const string SHADOW_QUALITY_KEY = "Options_ShadowQuality";
	private const string AMBIENT_OCCLUSION_KEY = "Options_AmbientOcclusion";
	private const string MESH_LOD_KEY = "Options_MeshLOD";


	// =========================================================
	// DISPLAY
	// =========================================================

	[Header("========== DISPLAY ==========")]

	[SerializeField] private TMP_Dropdown resolutionDropdown;

	[SerializeField] private bool fullscreen = true;

	private Resolution[] availableResolutions;

	private const string RESOLUTION_KEY = "Options_Resolution";
	private const string VSYNC_KEY = "Options_VSync";
	private const string SENSITIVITY_KEY = "Options_Sensitivity";

	private int selectedResolutionIndex = 0;


	// =========================================================
	// SENSITIVITY
	// =========================================================

	[Header("Sensitivity")]
	[SerializeField] private Slider sensitivitySlider;

	[SerializeField] private float defaultSensitivity = 1f;


	// =========================================================
	// DEFAULT VALUES
	// =========================================================

	[Header("Default Values")]

	[SerializeField] private float defaultMaster = 1f;
	[SerializeField] private float defaultMusic = 1f;
	[SerializeField] private float defaultSFX = 1f;
	[SerializeField] private float defaultVoice = 1f;

	[SerializeField] private bool defaultMotionBlur = true;
	[SerializeField] private bool defaultAmbientOcclusion = true;

	[SerializeField] private int defaultTextureQuality = 0;
	[SerializeField] private int defaultShadowQuality = 2;
	[SerializeField] private int defaultMeshLOD = 2;

	[SerializeField] private bool defaultVSync = true;


	// =========================================================
	// START
	// =========================================================

	private void Start()
	{
		SetupResolutionDropdown();
		LoadAllSettings();

		Debug.Log("Sensitivity: " + sensitivitySlider.value);
	}


	// =========================================================
	// AUDIO
	// =========================================================

	public void SetMasterVolume(float value)
	{
		SetMixerVolume(masterParameter, value);
		PlayerPrefs.SetFloat(MASTER_KEY, value);
		PlayerPrefs.Save();
	}

	public void SetMusicVolume(float value)
	{
		SetMixerVolume(musicParameter, value);
		PlayerPrefs.SetFloat(MUSIC_KEY, value);
		PlayerPrefs.Save();
	}

	public void SetSFXVolume(float value)
	{
		SetMixerVolume(sfxParameter, value);
		PlayerPrefs.SetFloat(SFX_KEY, value);
		PlayerPrefs.Save();
	}

	public void SetVoiceVolume(float value)
	{
		SetMixerVolume(voiceParameter, value);
		PlayerPrefs.SetFloat(VOICE_KEY, value);
		PlayerPrefs.Save();
	}

	private void SetMixerVolume(string parameter, float value)
	{
		if (audioMixer == null)
			return;

		value = Mathf.Clamp01(value);

		// 0 = -80 dB
		// 1 = 0 dB
		float dB = value <= 0.0001f
			? -80f
			: Mathf.Log10(value) * 20f;

		audioMixer.SetFloat(parameter, dB);
	}


	// =========================================================
	// MOTION BLUR
	// =========================================================

	public void MotionBlurOn()
	{
		SetMotionBlur(true);
	}

	public void MotionBlurOff()
	{
		SetMotionBlur(false);
	}

	private void SetMotionBlur(bool enabled)
	{
		PlayerPrefs.SetInt(MOTION_BLUR_KEY, enabled ? 1 : 0);
		PlayerPrefs.Save();

		if (postProcessVolume == null)
			return;

		if (postProcessVolume.profile == null)
			return;

		MotionBlur motionBlur;

		if (postProcessVolume.profile.TryGet(out motionBlur))
		{
			motionBlur.active = enabled;
		}
	}


	// =========================================================
	// TEXTURE QUALITY
	// =========================================================

	// Ultra
	public void TextureUltra()
	{
		SetTextureQuality(0);
	}

	// High
	public void TextureHigh()
	{
		SetTextureQuality(1);
	}

	// Medium
	public void TextureMedium()
	{
		SetTextureQuality(2);
	}

	// Low
	public void TextureLow()
	{
		SetTextureQuality(3);
	}

	private void SetTextureQuality(int quality)
	{
		quality = Mathf.Clamp(quality, 0, 3);

		// Unity:
		// 0 = Full Resolution
		// 1 = Half Resolution
		// 2 = Quarter Resolution
		// 3 = Eighth Resolution

		QualitySettings.globalTextureMipmapLimit = quality;

		PlayerPrefs.SetInt(TEXTURE_QUALITY_KEY, quality);
		PlayerPrefs.Save();
	}


	// =========================================================
	// SHADOW QUALITY
	// =========================================================

	// Ultra
	public void ShadowUltra()
	{
		SetShadowQuality(0);
	}

	// High
	public void ShadowHigh()
	{
		SetShadowQuality(1);
	}

	// Medium
	public void ShadowMedium()
	{
		SetShadowQuality(2);
	}

	// Low
	public void ShadowLow()
	{
		SetShadowQuality(3);
	}

	private void SetShadowQuality(int quality)
	{
		quality = Mathf.Clamp(quality, 0, 3);

		UniversalRenderPipelineAsset urp =
			GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

		if (urp == null)
			return;

		/*
		* Unity's URP does not expose a public setter for
		* mainLightShadowmapResolution.
		*
		* Therefore we modify the internal field using Reflection.
		*/

		int resolution;

		switch (quality)
		{
			// Ultra
		case 0:
			resolution = 4096;
			break;

			// High
		case 1:
			resolution = 2048;
			break;

			// Medium
		case 2:
			resolution = 1024;
			break;

			// Low
		default:
			resolution = 512;
			break;
		}

		SetURPShadowResolution(urp, resolution);

		PlayerPrefs.SetInt(SHADOW_QUALITY_KEY, quality);
		PlayerPrefs.Save();
	}

	private void SetURPShadowResolution(
		UniversalRenderPipelineAsset urp,
		int resolution)
	{
		FieldInfo field = typeof(UniversalRenderPipelineAsset)
			.GetField(
			"m_MainLightShadowmapResolution",
			BindingFlags.Instance |
			BindingFlags.NonPublic
			);

		if (field == null)
		{
			Debug.LogWarning(
				"Could not find URP Main Light Shadowmap Resolution field."
			);

			return;
		}

		field.SetValue(
			urp,
			(UnityEngine.Rendering.Universal.ShadowResolution)resolution
		);
	}


	// =========================================================
	// AMBIENT OCCLUSION
	// =========================================================

	public void AmbientOcclusionOn()
	{
		SetAmbientOcclusion(true);
	}

	public void AmbientOcclusionOff()
	{
		SetAmbientOcclusion(false);
	}

	private void SetAmbientOcclusion(bool enabled)
	{
		PlayerPrefs.SetInt(
			AMBIENT_OCCLUSION_KEY,
			enabled ? 1 : 0
		);

		PlayerPrefs.Save();

		if (ambientOcclusionFeature == null)
			return;

		ambientOcclusionFeature.SetActive(enabled);
	}


	// =========================================================
	// MESH LOD
	// =========================================================

	// Ultra
	public void MeshLODUltra()
	{
		SetMeshLOD(0);
	}

	// High
	public void MeshLODHigh()
	{
		SetMeshLOD(1);
	}

	// Medium
	public void MeshLODMedium()
	{
		SetMeshLOD(2);
	}

	// Low
	public void MeshLODLow()
	{
		SetMeshLOD(3);
	}

	private void SetMeshLOD(int quality)
	{
		quality = Mathf.Clamp(quality, 0, 3);

		/*
		* Unity 6.2's Mesh LOD system uses a project-wide
		* Mesh LOD Threshold.
		*
		* The values below make the LOD system increasingly
		* aggressive as quality decreases.
		*/

		float threshold;

		switch (quality)
		{
			// Ultra
		case 0:
			threshold = 0.5f;
			break;

			// High
		case 1:
			threshold = 1f;
			break;

			// Medium
		case 2:
			threshold = 2f;
			break;

			// Low
		default:
			threshold = 4f;
			break;
		}

		SetMeshLODThreshold(threshold);

		PlayerPrefs.SetInt(MESH_LOD_KEY, quality);
		PlayerPrefs.Save();
	}

	private void SetMeshLODThreshold(float value)
	{
		/*
		* Unity 6.2 exposes the Mesh LOD threshold through
		* the Quality settings system.
		*
		* Reflection is used here so the script remains
		* safe if Unity changes the public API.
		*/

		Type qualitySettingsType = typeof(QualitySettings);

		PropertyInfo property =
			qualitySettingsType.GetProperty(
			"meshLodThreshold",
			BindingFlags.Public |
			BindingFlags.Static
			);

		if (property != null && property.CanWrite)
		{
			property.SetValue(null, value);
			return;
		}

		FieldInfo field =
			qualitySettingsType.GetField(
			"meshLodThreshold",
			BindingFlags.Public |
			BindingFlags.Static
			);

		if (field != null)
		{
			field.SetValue(null, value);
			return;
		}

		/*
		* Fallback:
		*
		* Traditional LOD Groups still use lodBias.
		* This does NOT control the new Mesh LOD system,
		* but gives older/custom LOD Groups a sensible
		* quality response.
		*/

		switch (value)
		{
		case <= 0.5f:
			QualitySettings.lodBias = 2f;
			break;

		case <= 1f:
			QualitySettings.lodBias = 1.5f;
			break;

		case <= 2f:
			QualitySettings.lodBias = 1f;
			break;

		default:
			QualitySettings.lodBias = 0.5f;
			break;
		}
	}


	// =========================================================
	// RESOLUTION
	// =========================================================

	private void SetupResolutionDropdown()
	{
		if (resolutionDropdown == null)
			return;

		Resolution[] allResolutions = Screen.resolutions;

		List<Resolution> uniqueResolutions =
			new List<Resolution>();

		HashSet<string> addedResolutions =
			new HashSet<string>();

		for (int i = 0; i < allResolutions.Length; i++)
		{
			Resolution resolution = allResolutions[i];

			string key =
				resolution.width +
				"x" +
				resolution.height;

			// Only add each width/height once.
			if (addedResolutions.Contains(key))
				continue;

			addedResolutions.Add(key);
			uniqueResolutions.Add(resolution);
		}

		availableResolutions = uniqueResolutions.ToArray();

		resolutionDropdown.ClearOptions();

		List<string> options =
			new List<string>();

		for (int i = 0; i < availableResolutions.Length; i++)
		{
			Resolution resolution =
				availableResolutions[i];

			options.Add(
				resolution.width +
				" x " +
				resolution.height
			);
		}

		resolutionDropdown.AddOptions(options);

		int savedWidth =
			PlayerPrefs.GetInt(
			"Options_ResolutionWidth",
			Screen.currentResolution.width
			);

		int savedHeight =
			PlayerPrefs.GetInt(
			"Options_ResolutionHeight",
			Screen.currentResolution.height
			);

		selectedResolutionIndex = 0;

		for (int i = 0; i < availableResolutions.Length; i++)
		{
			if (
				availableResolutions[i].width == savedWidth &&
				availableResolutions[i].height == savedHeight
			)
			{
				selectedResolutionIndex = i;
				break;
			}
		}

		resolutionDropdown.value = selectedResolutionIndex;
		resolutionDropdown.RefreshShownValue();
	}

	public void SetResolution(int index)
	{
		if (
			availableResolutions == null ||
			availableResolutions.Length == 0
		)
			return;

		if (
			index < 0 ||
			index >= availableResolutions.Length
		)
			return;

		selectedResolutionIndex = index;

		Resolution resolution =
			availableResolutions[index];

		Screen.SetResolution(
			resolution.width,
			resolution.height,
			fullscreen
		);

		PlayerPrefs.SetInt(
			"Options_ResolutionWidth",
			resolution.width
		);

		PlayerPrefs.SetInt(
			"Options_ResolutionHeight",
			resolution.height
		);

		PlayerPrefs.Save();
	}


	// =========================================================
	// VSYNC
	// =========================================================

	public void VSyncOn()
	{
		SetVSync(true);
	}

	public void VSyncOff()
	{
		SetVSync(false);
	}

	private void SetVSync(bool enabled)
	{
		QualitySettings.vSyncCount =
			enabled ? 1 : 0;

		PlayerPrefs.SetInt(
			VSYNC_KEY,
			enabled ? 1 : 0
		);

		PlayerPrefs.Save();
	}


	// =========================================================
	// SENSITIVITY
	// =========================================================

	public void SetSensitivity(float value)
	{
		PlayerPrefs.SetFloat(
			SENSITIVITY_KEY,
			value
		);

		PlayerPrefs.Save();
	}


	// =========================================================
	// LOAD ALL SETTINGS
	// =========================================================

	private void LoadAllSettings()
	{
		// -------------------------
		// Audio
		// -------------------------

		float master =
			PlayerPrefs.GetFloat(
			MASTER_KEY,
			defaultMaster
			);

		float music =
			PlayerPrefs.GetFloat(
			MUSIC_KEY,
			defaultMusic
			);

		float sfx =
			PlayerPrefs.GetFloat(
			SFX_KEY,
			defaultSFX
			);

		float voice =
			PlayerPrefs.GetFloat(
			VOICE_KEY,
			defaultVoice
			);

		if (masterSlider != null)
			masterSlider.SetValueWithoutNotify(master);

		if (musicSlider != null)
			musicSlider.SetValueWithoutNotify(music);

		if (sfxSlider != null)
			sfxSlider.SetValueWithoutNotify(sfx);

		if (voiceSlider != null)
			voiceSlider.SetValueWithoutNotify(voice);

		SetMixerVolume(masterParameter, master);
		SetMixerVolume(musicParameter, music);
		SetMixerVolume(sfxParameter, sfx);
		SetMixerVolume(voiceParameter, voice);


		// -------------------------
		// Motion Blur
		// -------------------------

		bool motionBlur =
			PlayerPrefs.GetInt(
			MOTION_BLUR_KEY,
			defaultMotionBlur ? 1 : 0
			) == 1;

		SetMotionBlur(motionBlur);


		// -------------------------
		// Texture Quality
		// -------------------------

		int textureQuality =
			PlayerPrefs.GetInt(
			TEXTURE_QUALITY_KEY,
			defaultTextureQuality
			);

		SetTextureQuality(textureQuality);


		// -------------------------
		// Shadow Quality
		// -------------------------

		int shadowQuality =
			PlayerPrefs.GetInt(
			SHADOW_QUALITY_KEY,
			defaultShadowQuality
			);

		SetShadowQuality(shadowQuality);


		// -------------------------
		// Ambient Occlusion
		// -------------------------

		bool ambientOcclusion =
			PlayerPrefs.GetInt(
			AMBIENT_OCCLUSION_KEY,
			defaultAmbientOcclusion ? 1 : 0
			) == 1;

		SetAmbientOcclusion(ambientOcclusion);


		// -------------------------
		// Mesh LOD
		// -------------------------

		int meshLOD =
			PlayerPrefs.GetInt(
			MESH_LOD_KEY,
			defaultMeshLOD
			);

		SetMeshLOD(meshLOD);


		// -------------------------
		// VSync
		// -------------------------

		bool vsync =
			PlayerPrefs.GetInt(
			VSYNC_KEY,
			defaultVSync ? 1 : 0
			) == 1;

		SetVSync(vsync);


		// -------------------------
		// Sensitivity
		// -------------------------

		float sensitivity =
			PlayerPrefs.GetFloat(
			SENSITIVITY_KEY,
			defaultSensitivity
			);

		if (sensitivitySlider != null)
		{
			sensitivitySlider.SetValueWithoutNotify(
				sensitivity
			);
		}

		Debug.Log(
			"Sensitivity: " +
			sensitivity
		);


		// -------------------------
		// Resolution
		// -------------------------

		int savedWidth =
			PlayerPrefs.GetInt(
			"Options_ResolutionWidth",
			Screen.currentResolution.width
			);

		int savedHeight =
			PlayerPrefs.GetInt(
			"Options_ResolutionHeight",
			Screen.currentResolution.height
			);

		ApplySavedResolution(
			savedWidth,
			savedHeight
		);
	}


	// =========================================================
	// APPLY SAVED RESOLUTION
	// =========================================================

	private void ApplySavedResolution(
		int width,
		int height
	)
	{
		if (
			availableResolutions == null ||
			availableResolutions.Length == 0
		)
			return;

		int index = 0;

		for (int i = 0; i < availableResolutions.Length; i++)
		{
			if (
				availableResolutions[i].width == width &&
				availableResolutions[i].height == height
			)
			{
				index = i;
				break;
			}
		}

		selectedResolutionIndex = index;

		Resolution resolution =
			availableResolutions[index];

		Screen.SetResolution(
			resolution.width,
			resolution.height,
			fullscreen
		);

		if (resolutionDropdown != null)
		{
			resolutionDropdown.SetValueWithoutNotify(
				index
			);

			resolutionDropdown.RefreshShownValue();
		}
	}


	// =========================================================
	// RESET SETTINGS
	// =========================================================

	public void ResetAllSettings()
	{
		PlayerPrefs.DeleteKey(MASTER_KEY);
		PlayerPrefs.DeleteKey(MUSIC_KEY);
		PlayerPrefs.DeleteKey(SFX_KEY);
		PlayerPrefs.DeleteKey(VOICE_KEY);

		PlayerPrefs.DeleteKey(MOTION_BLUR_KEY);
		PlayerPrefs.DeleteKey(TEXTURE_QUALITY_KEY);
		PlayerPrefs.DeleteKey(SHADOW_QUALITY_KEY);
		PlayerPrefs.DeleteKey(AMBIENT_OCCLUSION_KEY);
		PlayerPrefs.DeleteKey(MESH_LOD_KEY);

		PlayerPrefs.DeleteKey(RESOLUTION_KEY);
		PlayerPrefs.DeleteKey("Options_ResolutionWidth");
		PlayerPrefs.DeleteKey("Options_ResolutionHeight");

		PlayerPrefs.DeleteKey(VSYNC_KEY);
		PlayerPrefs.DeleteKey(SENSITIVITY_KEY);

		PlayerPrefs.Save();

		LoadAllSettings();
	}
}
