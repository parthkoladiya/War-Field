using UnityEngine;
using GoogleMobileAds.Api;

public class AdMobAdsScript : MonoBehaviour
{

    string Banner_Id = "ca-app-pub-2716074254005843/2815843327";
    private BannerView bannerView;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        MobileAds.Initialize(initstatus => { });
        RequestBannerAd();
    }

    public void RequestBannerAd()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
        }

        bannerView = new BannerView(Banner_Id, AdSize.Banner, AdPosition.Bottom);

        AdRequest request = new AdRequest();
        bannerView.LoadAd(request);
        bannerView.Show();
    }

    public void DestoryBannerAd()
    {
        if (bannerView != null)
        {
            bannerView.Hide();
            bannerView.Destroy();
            bannerView = null;
        }
    }


    //public string appID = "ca-app-pub-2716074254005843~4674051600";

    //#if UNITY_ANDROID
    //string bannerId = "ca-app-pub-2716074254005843/2815843327";
    //string interId = "" ;
    //string rewaredId = "";
    //string netivId ="";

    //#endif

    //BannerView bannerView;
    //InterstitialAd interstitialAd;

    //void Start()
    //{
    //MobileAds.RaiseAdEventsOnUnityMainThread = true;
    //MobileAds.Initialize(initstatus => {

    //print(" Ads Initialize ");

    //});

    //}
    //#region Banner

    //public void LoadBannerAd()
    //{
    //CreateBannerView();
    //lisitonBannerAD();

    //if (bannerView == null)
    //{
    //CreateBannerView();
    //}

    //var adrequest = new AdRequest();
    //adrequest.Keywords.Add("unity-admob-sample");

    //print("Loading Banner ads");
    //bannerView.LoadAd(adrequest);
    //}

    //public void CreateBannerView()
    //{
    //if (bannerView != null)
    //{
    //DestoryBannerAd();
    //}
    //bannerView = new BannerView(bannerId, AdSize.Banner, AdPosition.Top);
    //}

    //public void lisitonBannerAD()
    //{
    //// Raised when an ad is loaded into the banner view.
    //bannerView.OnBannerAdLoaded += () =>
    //{
    //Debug.Log("Banner view loaded an ad with response : "
    //+ bannerView.GetResponseInfo());
    //};
    //// Raised when an ad fails to load into the banner view.
    //bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
    //{
    //Debug.LogError("Banner view failed to load an ad with error : "
    //+ error);
    //};
    //// Raised when the ad is estimated to have earned money.
    //bannerView.OnAdPaid += (AdValue adValue) =>
    //{
    //Debug.Log("Banner view paid {0} {1}."+
    //adValue.Value+
    //adValue.CurrencyCode);
    //};
    //// Raised when an impression is recorded for an ad.
    //bannerView.OnAdImpressionRecorded += () =>
    //{
    //Debug.Log("Banner view recorded an impression.");
    //};
    //// Raised when a click is recorded for an ad.
    //bannerView.OnAdClicked += () =>
    //{
    //Debug.Log("Banner view was clicked.");
    //};
    //// Raised when an ad opened full screen content.
    //bannerView.OnAdFullScreenContentOpened += () =>
    //{
    //Debug.Log("Banner view full screen content opened.");
    //};
    //// Raised when the ad closed full screen content.
    //bannerView.OnAdFullScreenContentClosed += () =>
    //{
    //Debug.Log("Banner view full screen content closed.");
    //};
    //}

    //public void DestoryBannerAd()
    //{
    //if (bannerView != null)
    //{
    //print("Bannerad was destopry");
    //bannerView.Destroy();
    //bannerView = null;
    //}
    //}

    //#endregion

}
