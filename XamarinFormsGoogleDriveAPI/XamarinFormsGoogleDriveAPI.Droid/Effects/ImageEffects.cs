using System.Linq;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XamarinFormsGoogleDriveAPI.Droid.Effects;
using XamarinFormsGoogleDriveAPI.Effects;

[assembly: ResolutionGroupName("GoogleDrive")]
[assembly: ExportEffect(typeof(ImageEffects), "ImageEffects")]
namespace XamarinFormsGoogleDriveAPI.Droid.Effects
{
    public class ImageEffects : PlatformEffect
    {
        protected override void OnAttached()
        {
            var imageView = Control as ImageView;
            var tintEffect = (TintEffect) Element.Effects.FirstOrDefault(row => row is TintEffect);
            imageView.SetColorFilter(tintEffect.TintColor.ToAndroid());
        }

        protected override void OnDetached()
        {

        }
    }
}