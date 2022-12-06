using System;
using BeautyPortionAdmin.Fonts;
using BeautyPortionAdmin.Helpers;
using Xamarin.Forms;

namespace BeautyPortionAdmin.Views.Dialogs.ActionSheet
{
    public enum ActionSheetType
    {
        Default,
        FaIcon,
        SvgIcon
    }

    public abstract class ActionSheetOption
    {
        public ActionSheetOption(string option, ActionSheetType actionSheetType)
        {
            Option = option;
            ActionSheetType = actionSheetType;
        }

        public string Option { get; }
        public ActionSheetType ActionSheetType { get; }
    }

    public class DefaultActionSheetOption : ActionSheetOption
    {
        public DefaultActionSheetOption(string option) : base(option, ActionSheetType.Default)
        {
        }
    }

    public class FaIconActionSheetOption : ActionSheetOption
    {
        public FaIconActionSheetOption(string option, string faIcon, FaFontStyle fontStyle = FaFontStyle.Solid) : base(option, ActionSheetType.FaIcon)
        {
            FaIcon = faIcon;
            FontFamily = GetFontFamily(fontStyle);
            IconColor = Colors.PrimaryDarkColor;
        }

        public FaIconActionSheetOption(string option, string faIcon, Color iconColor, FaFontStyle fontStyle = FaFontStyle.Solid) : base(option, ActionSheetType.FaIcon)
        {
            FaIcon = faIcon;
            IconColor = iconColor;
            FontFamily = GetFontFamily(fontStyle);
        }

        public string FaIcon { get; }
        public string FontFamily { get; }
        public Color IconColor { get; }

        string GetFontFamily(FaFontStyle fontStyle)
        {
            return fontStyle switch
            {
                FaFontStyle.Solid => "FontAwesomeSolid",
                FaFontStyle.Regular => "FontAwesomeRegular",
                FaFontStyle.Light => "FontAwesomeLight",
                FaFontStyle.Brands => "FontAwesomeBrands",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public class SvgIconActionSheetOption : ActionSheetOption
    {
        public SvgIconActionSheetOption(string option, string svgIcon, double widthSize = 25, double heightSize = 25) : base(option, ActionSheetType.SvgIcon)
        {
            SvgIcon = svgIcon;
            WidthSize = widthSize;
            HeightSize = heightSize;
        }

        public string SvgIcon { get; }
        public double WidthSize { get; }
        public double HeightSize { get; }
    }
}
