using System;
using System.Collections.Generic;
using System.Text;
using FAB.Forms;
using FAB.iOS;
using Xamarin.Forms;
using XamarinFormsGoogleDriveAPI.iOS.Renderer;

[assembly: ExportRenderer(typeof(FloatingActionButton), typeof(CustomFabRenderer))]
namespace XamarinFormsGoogleDriveAPI.iOS.Renderer
{
    public class CustomFabRenderer : FloatingActionButtonRenderer
    {
        public static void Init()
        {

        }
    }
}
