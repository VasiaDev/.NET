using Gtk;
using System;
using Gdk;
using System.IO;

namespace DispensaryApp.UI.Styles
{
    public static class StyleManager
    {
        private static CssProvider? _cssProvider;

        public static void Initialize()
        {
            _cssProvider = new CssProvider();
            var themePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Styles", "Theme.css");
            _cssProvider.LoadFromPath(themePath);
            
            StyleContext.AddProviderForScreen(
                Gdk.Screen.Default,
                _cssProvider,
                StyleProviderPriority.Application
            );
        }

        public static void ApplyButtonStyle(Button button)
        {
            button.StyleContext.AddClass("primary-button");
        }

        public static void ApplySecondaryButtonStyle(Button button)
        {
            button.StyleContext.AddClass("secondary-button");
        }

        public static void ApplyTreeViewStyle(TreeView treeView)
        {
            if (_cssProvider == null) return;
            
            treeView.StyleContext.AddProvider(
                _cssProvider,
                StyleProviderPriority.Application
            );
        }

        public static void ApplyEntryStyle(Entry entry)
        {
            entry.StyleContext.AddClass("entry");
        }

        public static void ApplyComboBoxStyle(ComboBox comboBox)
        {
            comboBox.StyleContext.AddClass("combo-box");
        }

        public static void ApplyErrorStyle(Widget widget)
        {
            widget.StyleContext.AddClass("error");
        }

        public static void ApplySuccessStyle(Widget widget)
        {
            widget.StyleContext.AddClass("success");
        }

        public static void ApplyTitleStyle(Label label)
        {
            label.StyleContext.AddClass("title");
        }

        public static void ApplySubtitleStyle(Label label)
        {
            label.StyleContext.AddClass("subtitle");
        }

        public static void ApplyInfoMessageStyle(Label label)
        {
            label.StyleContext.AddClass("info-message");
        }

        public static void ApplyErrorMessageStyle(Label label)
        {
            label.StyleContext.AddClass("error-message");
        }

        public static void ApplySuccessMessageStyle(Label label)
        {
            label.StyleContext.AddClass("success-message");
        }
    }
} 