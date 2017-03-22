using CoreGraphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XamarinFormsGoogleDriveAPI.iOS.Renderer;

[assembly: ExportRenderer(typeof(ViewCell), typeof(CustomCellRenderer))]
namespace XamarinFormsGoogleDriveAPI.iOS.Renderer
{
    public class CustomCellRenderer : ViewCellRenderer
    {
        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            var cell =  base.GetCell(item, reusableCell, tv);
            cell.SelectionStyle = UITableViewCellSelectionStyle.Gray;
            return cell;
        }
    }
}
