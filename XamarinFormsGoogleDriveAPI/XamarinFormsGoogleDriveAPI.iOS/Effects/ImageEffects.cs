using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XamarinFormsGoogleDriveAPI.Effects;
using XamarinFormsGoogleDriveAPI.iOS.Effects;

[assembly: ResolutionGroupName("GoogleDrive")]
[assembly: ExportEffect(typeof(ImageEffects), "ImageEffects")]
namespace XamarinFormsGoogleDriveAPI.iOS.Effects
{
    public class ImageEffects : PlatformEffect
    {
        protected override void OnAttached()
        {
            var imageView = Control as UIImageView;
            var tintEffect = (TintEffect) Element.Effects.FirstOrDefault(row => row is TintEffect);
            imageView.TintColor = tintEffect.TintColor.ToUIColor();
        }

        protected override void OnDetached()
        {

        }
    }
}
