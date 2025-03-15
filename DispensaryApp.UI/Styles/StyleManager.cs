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
            var cssProvider = new CssProvider();
            cssProvider.LoadFromData(@"
                treeview {
                    background-color: white;
                    border: 1px solid #ddd;
                }
                treeview:selected {
                    background-color: @primary-color;
                    color: white;
                }
                treeview header {
                    background-color: #f5f5f5;
                    border-bottom: 1px solid #ddd;
                }
                treeview header button {
                    padding: 8px;
                    border: none;
                    background: transparent;
                }
            ");

            treeView.StyleContext.AddProvider(
                cssProvider,
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
    }
} 