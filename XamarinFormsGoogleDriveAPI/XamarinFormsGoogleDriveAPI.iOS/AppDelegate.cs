using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using Xamarin.Auth;
using XamarinFormsGoogleDriveAPI.iOS.Renderer;

namespace XamarinFormsGoogleDriveAPI.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            string userAgent =
                "Mozilla/5.0 (Linux; Android 5.1.1; Nexus 5 Build/LMY48B; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/43.0.2357.65 Mobile Safari/537.36";

            // set default useragent
            NSDictionary dictionary = NSDictionary.FromObjectAndKey(NSObject.FromObject(userAgent),
                NSObject.FromObject("UserAgent"));
            NSUserDefaults.StandardUserDefaults.RegisterDefaults(dictionary);
            var userDefaults = NSUserDefaults.StandardUserDefaults.StringForKey("FirstRun");
            if (string.IsNullOrEmpty(userDefaults))
            {
                var account = AccountStore.Create().FindAccountsForService("Google").FirstOrDefault();
                if (account != null)
                    AccountStore.Create().Delete(account, "Google");
                NSUserDefaults.StandardUserDefaults.SetString("Yes", "FirstRun");
            }
            global::Xamarin.Forms.Forms.Init();
            CustomFabRenderer.Init();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }
    }
}
