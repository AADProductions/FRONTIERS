using UnityEngine;
using System.Collections;
using Frontiers;
using System.Collections.Generic;
using System;

namespace Frontiers.GUI
{
	public class GUILoading : MonoBehaviour
	{
		public static void Lock(System.Object lockObject)
		{
			LockObject = lockObject;
		}

		public static void Unlock(System.Object lockObject)
		{
			if (lockObject == LockObject) {
				LockObject = null;
			}
		}

		public static System.Object LockObject;

		public static bool IsLocked {
			get {
				return LockObject != null;
			}
		}

		public static bool IsLoading = false;
		public static GUILoading Get;
		public static bool SplashScreen = false;
		public static float BackgroundSpriteAlphaMultiplier = 0.5f;
		public UISprite SplashScreenSprite;
		public UISprite BackgroundSprite;
		public UISprite BackgroundOverlaySprite;
		public float SplashScreenInterval = 0.25f;
		public Camera LoadingCamera;
		public GameObject LoadingCompass;
		public UIPanel LoadingCompassPanel;
		public UIPanel LoadingCompassArrowPanel;
		public UILabel DetailsInfoLabel;
		public UILabel ActivityInfoLabel;
		public UILabel ErrorMessage;
		public UILabel QuitMessage;
		public UIAnchor LoadingAnchor;
		public GameObject LoadingCompassBig;
		public UIPanel LoadingCompassPanelBig;
		public UIPanel LoadingCompassArrowPanelBig;
		public UILabel DetailsInfoLabelBig;
		public UILabel ActivityInfoLabelBig;
		public UILabel ErrorMessageBig;
		public UILabel QuitMessageBig;
		public UIAnchor LoadingAnchorBig;
		public GameObject LoadingCompassLittle;
		public UIPanel LoadingCompassPanelLittle;
		public UIPanel LoadingCompassArrowPanelLitte;
		public UILabel DetailsInfoLabelLittle;
		public UILabel ActivityInfoLabelLittle;
		public UILabel ErrorMessageLittle;
		public UILabel QuitMessageLittle;
		public UIAnchor LoadingAnchorLittle;

		public void SetLittleCompass(bool littleCompass)
		{
			if (littleCompass) {
				LoadingCompass = LoadingCompassLittle;
				LoadingCompassPanel = LoadingCompassPanelLittle;
				DetailsInfoLabel = DetailsInfoLabelLittle;
				ActivityInfoLabel = ActivityInfoLabelLittle;
				ErrorMessage = ErrorMessageLittle;
				QuitMessage = QuitMessageLittle;
				LoadingAnchor = LoadingAnchorLittle;
				LoadingCompassArrowPanel = LoadingCompassArrowPanelLitte;

				LoadingCompassPanelBig.enabled = false;
				LoadingCompassPanelLittle.enabled = true;
			} else {
				LoadingCompass = LoadingCompassBig;
				LoadingCompassPanel = LoadingCompassPanelBig;
				DetailsInfoLabel = DetailsInfoLabelBig;
				ActivityInfoLabel = ActivityInfoLabelBig;
				ErrorMessage = ErrorMessageBig;
				QuitMessage = QuitMessageBig;
				LoadingAnchor = LoadingAnchorBig;
				LoadingCompassArrowPanel = LoadingCompassArrowPanelBig;

				LoadingCompassPanelBig.enabled = true;
				LoadingCompassPanelLittle.enabled = false;
			}
			LoadingCompassArrowPanel.enabled = false;

		}

		public UILabel QuoteText;
		public GameObject CharacterObject;
		public Light CharacterLight;
		public List <CharacterQuote> CharacterQuotes = new List <CharacterQuote>();

		public static string ActivityInfo {
			get {
				return Get.ActivityInfoLabel.text;
			}
			set {
				Get.ActivityInfoLabel.text = value;
				if (!Get.ActivityInfoLabel.enabled) {
					Get.ActivityInfoLabel.enabled = true;
				}
			}
		}

		public static string DetailsInfo {
			get {
				return Get.DetailsInfoLabel.text;
			}
			set {
				Get.DetailsInfoLabel.text = value;
				if (!Get.DetailsInfoLabel.enabled) {
					Get.DetailsInfoLabel.enabled = true;
				}
			}
		}

		public void Awake()
		{
			Initialize();
		}

		public void Initialize()
		{
			if (mInitialized)
				return;

			HideCharacterQuote();

			DontDestroyOnLoad(transform);

			Get = this;
			Get.LoadingCamera.enabled = true;

			SetLittleCompass(true);

			ActivityInfo = "Loading";
			DetailsInfo = string.Empty;
			QuitMessage.enabled = false;
			ErrorMessage.enabled = false;
			SplashScreen = false;
			//SplashScreenSprite.enabled	= false;
			LoadingCompass.gameObject.SetActive(false);

			SetLittleCompass(false);

			ActivityInfo = "Loading";
			DetailsInfo = string.Empty;
			QuitMessage.enabled = false;
			ErrorMessage.enabled = false;
			SplashScreen = false;
			//SplashScreenSprite.enabled	= false;
			LoadingCompass.gameObject.SetActive(false);

			mInitialized = true;
		}

		protected static bool mInitialized = false;

		public void Start()
		{
			if (GameManager.Get.TestingEnvironment) {
				SplashScreen = false;
			} else {
				StartCoroutine(SplashScreenOverTime());
			}
		}

		public IEnumerator SplashScreenOverTime()
		{
			yield break;
			//TEMP - killing this to save memory
//			BackgroundSprite.alpha = 1.0f;
//			BackgroundSprite.enabled = true;
//			//fade in...
//			SplashScreen = true;
//			SplashScreenSprite.alpha = 0f;
//			SplashScreenSprite.enabled	= true;
//			SplashScreenSprite.animation.Play ("LogoFadeIn");
//			while (SplashScreenSprite.animation ["LogoFadeIn"].normalizedTime < 1f) {	//Debug.Log ("Fading in..." + SplashScreenSprite.animation ["LogoFadeIn"].normalizedTime);
//				yield return null;
//			}
//			//stay...
//			yield return new WaitForSeconds (SplashScreenInterval);
//			//fade out...
//			SplashScreenSprite.animation.Play ("LogoFadeOut");
//			while (SplashScreenSprite.animation ["LogoFadeOut"].normalizedTime < 1f) {	//Debug.Log ("Fading out..." + SplashScreenSprite.animation ["LogoFadeOut"].normalizedTime);
//				yield return null;
//			}
//			SplashScreenSprite.alpha = 0f;
//			SplashScreenSprite.enabled	= false;
//			SplashScreen = false;
		}

		public static void DisplayError(string errorMessage)
		{
			Get.ErrorMessage.enabled = true;
			Get.ErrorMessage.text = errorMessage;
			Get.QuitMessage.enabled = true;
		}

		public static IEnumerator LoadStart(Mode mode, bool startBlack)
		{
			if (!mInitialized) {
				GameObject guiLoading = GameObject.Find("=LOADING=");
				Get = guiLoading.GetComponent <GUILoading>();
				Get.Initialize();
			}
			Get.LoadingCamera.enabled = true;
			float backgroundAlphaTarget = 0f;
			//Debug.Log ("GUILOAING: LoadStart");
			if (IsLoading) {
				return Get.BreakImmediately();
			} else {
				switch (mode) {
					case Mode.SmallInGame:
					default:
						Get.SetLittleCompass(true);
						Get.BackgroundSprite.enabled = false;
						Get.BackgroundOverlaySprite.enabled = false;
						Get.LoadingAnchor.side = UIAnchor.Side.Center;
						break;

					case Mode.FullScreenBlack:
						Get.SetLittleCompass(false);
						backgroundAlphaTarget = 1.0f;
						Get.DisplayRandomCharacterQuote();
						if (GameManager.Is(FGameState.Startup) || startBlack) {
							Get.BackgroundSprite.alpha = 1f;
							Get.BackgroundOverlaySprite.alpha = BackgroundSpriteAlphaMultiplier;
						} else {
							Get.BackgroundSprite.alpha = 0f;
							Get.BackgroundOverlaySprite.alpha = 0f;
						}
						Get.QuoteText.alpha = Get.BackgroundSprite.alpha;
						Get.BackgroundSprite.enabled = true;
						Get.BackgroundOverlaySprite.enabled = true;
						Get.LoadingAnchor.side = UIAnchor.Side.BottomLeft;
						break;
				}
				return Get.LoadStartOverTime(backgroundAlphaTarget);
			}
		}

		public static IEnumerator LoadStart(Mode mode)
		{
			if (!mInitialized) {
				GameObject guiLoading = GameObject.Find("=LOADING=");
				Get = guiLoading.GetComponent <GUILoading>();
				Get.Initialize();
			}
			Get.LoadingCamera.enabled = true;
			float backgroundAlphaTarget = 0f;
			//Debug.Log ("GUILOAING: LoadStart");
			if (IsLoading) {
				return Get.BreakImmediately();
			} else {
				switch (mode) {
					case Mode.SmallInGame:
					default:
						Get.SetLittleCompass(true);
						Get.BackgroundSprite.enabled = false;
						Get.BackgroundOverlaySprite.enabled = false;
						Get.LoadingAnchor.side = UIAnchor.Side.Center;
						break;

					case Mode.FullScreenBlack:
						Get.SetLittleCompass(false);
						backgroundAlphaTarget = 1.0f;
						Get.DisplayRandomCharacterQuote();
						if (GameManager.Is(FGameState.Startup)) {
							Get.BackgroundSprite.alpha = 1f;
							Get.BackgroundOverlaySprite.alpha = BackgroundSpriteAlphaMultiplier;
						} else {
							Get.BackgroundSprite.alpha = 0f;
							Get.BackgroundOverlaySprite.alpha = 0f;
						}
						Get.QuoteText.alpha = Get.BackgroundSprite.alpha;
						Get.BackgroundSprite.enabled = true;
						Get.BackgroundOverlaySprite.enabled = true;
						Get.LoadingAnchor.side = UIAnchor.Side.BottomLeft;
						break;
				}
				return Get.LoadStartOverTime(backgroundAlphaTarget);
			}
		}

		public static IEnumerator LoadFinish()
		{
			//Debug.Log ("GUILOAING: LoadFinish");
			if (!IsLoading) {
				Get.BackgroundSprite.enabled = false;
				Get.BackgroundOverlaySprite.enabled = false;
				return Get.BreakImmediately();
			}
			if (IsLocked) {
				return Get.BreakImmediately();
			}
			return Get.LoadFinishOverTime();
		}

		public IEnumerator LoadFinishOverTime()
		{
			//Debug.Log ("GUILOAING: LoadFinishOverTime");
			ActivityInfo = "Finished Loading...";
			DetailsInfo = string.Empty;
			LoadingCompass.animation["NGUIScaleDown"].normalizedTime = 0f;
			LoadingCompass.animation.Play("NGUIScaleDown");
			yield return null;
			//now that the background won't be opaque
			//turn on the camera again
			GameManager.Get.GameCamera.enabled = true;
			while (LoadingCompass.animation["NGUIScaleDown"].normalizedTime < 0.95f) {	//wait for the animation to complete
				LoadingCompass.animation["NGUIScaleDown"].time += (float)WorldClock.RTDeltaTimeSmooth;
				LoadingCompass.animation.Sample();
				//Debug.Log (LoadingCompass.animation ["NGUIScaleDown"].normalizedTime);
				BackgroundSprite.alpha = Mathf.Lerp(BackgroundSprite.alpha, 0f, (float)WorldClock.RTDeltaTimeSmooth * 2.5f);
				BackgroundOverlaySprite.alpha = BackgroundSprite.alpha * BackgroundSpriteAlphaMultiplier;
				QuoteText.alpha = BackgroundSprite.alpha;
				yield return null;
			}
			LoadingCompass.animation.Stop();
			while (BackgroundSprite.alpha > 0.05f) {
				BackgroundSprite.alpha = Mathf.Lerp(BackgroundSprite.alpha, 0f, (float)WorldClock.RTDeltaTimeSmooth * 2.5f);
				BackgroundOverlaySprite.alpha = BackgroundSprite.alpha * BackgroundSpriteAlphaMultiplier;
				QuoteText.alpha = BackgroundSprite.alpha;
				yield return null;
			}
			LoadingCompass.animation["NGUIScaleUp"].normalizedTime = 0f;
			LoadingCompass.animation.Sample();
			LoadingCompass.transform.localScale = Vector3.zero;
			LoadingCompass.SetActive(false);
			BackgroundSprite.alpha = 0f;
			BackgroundOverlaySprite.alpha = 0f;
			BackgroundSprite.enabled = false;
			IsLoading = false;
			HideCharacterQuote();
			Get.LoadingCamera.enabled = false;
			//disable all panels
			gameObject.SetActive(false);
			yield break;
		}

		public IEnumerator LoadStartOverTime(float backgroundAlphaTarget)
		{
			//this prevents a weird quirk where the arrow panel doesn't behave
			gameObject.SetActive(true);
			//Debug.Log ("GUILOAING: LoadStartOverTime");
			IsLoading = true;
			LoadingCompass.transform.localScale = Vector3.zero;
			LoadingCompass.SetActive(true);
			ActivityInfo = "Loading...";
			DetailsInfo = string.Empty;
			LoadingCompass.animation["NGUIScaleUp"].normalizedTime = 0f;
			LoadingCompass.animation.Play("NGUIScaleUp");
			yield return null;
			LoadingCompassArrowPanel.enabled = true;
			while (LoadingCompass.animation["NGUIScaleUp"].normalizedTime < 0.97f) {	//wait for the animation to complete
				LoadingCompass.animation["NGUIScaleUp"].time += (float)WorldClock.RTDeltaTimeSmooth;
				LoadingCompass.animation.Sample();
				//Debug.Log (LoadingCompass.animation ["NGUIScaleUp"].normalizedTime);
				QuoteText.alpha = Mathf.Lerp(QuoteText.alpha, 1.0f, (float)WorldClock.RTDeltaTimeSmooth * 5f);
				if (GameManager.Is(FGameState.Startup)) {
					BackgroundSprite.alpha = 1f;
					BackgroundOverlaySprite.alpha = BackgroundSpriteAlphaMultiplier;
				} else {
					BackgroundSprite.alpha = Mathf.Lerp(BackgroundSprite.alpha, backgroundAlphaTarget, (float)WorldClock.RTDeltaTimeSmooth * 10f);
					BackgroundOverlaySprite.alpha = BackgroundSprite.alpha * BackgroundSpriteAlphaMultiplier;
				}
				yield return null;
			}
			LoadingCompass.animation["NGUIScaleUp"].normalizedTime = 1f;
			LoadingCompass.animation.Sample();
			BackgroundSprite.alpha = backgroundAlphaTarget;
			BackgroundOverlaySprite.alpha = backgroundAlphaTarget * BackgroundSpriteAlphaMultiplier;
			yield return null;
			//now that background sprite is 1f
			//we can disable the camera
						if (backgroundAlphaTarget == 1f) {
								GameManager.Get.GameCamera.enabled = false;
						}
			yield break;
		}

		public IEnumerator WaitForLoadFinish()
		{
			while (IsLoading) {
				yield return null;
			}
			yield break;
		}

		public void CleanQuotes()
		{
			foreach (CharacterQuote quote in CharacterQuotes) {
				quote.Quote = quote.Quote.Replace("\"", "");
				string[] splitQuote = quote.Quote.Split(new string [] { "-" }, StringSplitOptions.RemoveEmptyEntries);
				quote.Quote = splitQuote[0].Trim();
			}
		}

		protected void DisplayRandomCharacterQuote()
		{
			try {
				bool hasQuote = false;
				CharacterQuote quote = null;
				while (!hasQuote) {
					int randomQuoteIndex = UnityEngine.Random.Range(0, CharacterQuotes.Count);
					quote = CharacterQuotes[randomQuoteIndex];
					if (quote != null && quote.RequiresPlayerName && !(Profile.Get.HasCurrentGame && Profile.Get.CurrentGame.HasCreatedCharacter)) {
						continue;
					}
					bool completedMission = true;
					if (quote.RequireMissionComplete && !(Missions.Get.MissionCompletedByName(quote.MissionName, ref completedMission) && completedMission)) {
						continue;
					}
					hasQuote = true;
				}
				CharacterObject.SetActive(true);
				CharacterLight.enabled = true;
				QuoteText.enabled = true;
				if (quote.RequiresPlayerName) {
					QuoteText.text = quote.FormattedQuote.Replace("{playername}", Profile.Get.CurrentGame.Character.FirstName);
				} else {
					QuoteText.text = quote.FormattedQuote;
				}
			} catch (Exception e) {
				Debug.LogException(e);
			}
		}

		protected void HideCharacterQuote()
		{
			CharacterObject.SetActive(false);
			CharacterLight.enabled = false;
			QuoteText.enabled = false;
		}

		public IEnumerator BreakImmediately()
		{
			yield break;
		}

		public enum Mode
		{
			FullScreenBlack,
			SmallInGame,
		}

		[Serializable]
		public class CharacterQuote
		{
			public string FormattedQuote {
				get {
					Color textColor = Colors.ColorFromString(CharacterName, 200);
					Color nameColor = Colors.Saturate(Colors.ColorFromString(CharacterName, 100));
					return Colors.ColorWrap(Quote, textColor) + Colors.ColorWrap("\n - " + CharacterName, nameColor);
				}
			}

			public string CharacterName;
			public string Quote;
			public bool RequireMissionComplete = false;
			public string MissionName = string.Empty;
			public bool RequiresPlayerName = false;
		}
	}
}