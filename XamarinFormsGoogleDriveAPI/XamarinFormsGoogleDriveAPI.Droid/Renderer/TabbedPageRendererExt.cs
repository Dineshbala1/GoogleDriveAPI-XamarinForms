using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Platform.Android.AppCompat;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XamarinFormsGoogleDriveAPI.Droid.Renderer;

[assembly: ExportRenderer(typeof(NavigationPage), typeof(TabbedPageRendererExt))]
namespace XamarinFormsGoogleDriveAPI.Droid.Renderer
{
    public class TabbedPageRendererExt : NavigationPageRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<NavigationPage> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null)
            {
            }
        }

        private void InvertLayoutThroughScale()
        {
            ViewGroup.ScaleY = -1;

            TabLayout tabLayout = null;
            ViewPager viewPager = null;

            for (int i = 0; i < ChildCount; ++i)
            {
                Android.Views.View view = (Android.Views.View)GetChildAt(i);
                if (view is TabLayout) tabLayout = (TabLayout)view;
                else if (view is ViewPager) viewPager = (ViewPager)view;
            }

            tabLayout.ScaleY = viewPager.ScaleY = -1;
            viewPager.SetPadding(0, -tabLayout.MeasuredHeight, 0, 0);
        }
    }
}