using iNKORE.UI.WPF.Modern.Controls.Helpers;
using iNKORE.UI.WPF.Modern.Controls.Primitives;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace STranslate.Helpers;

/// <summary>
/// 清理 iNKORE modern window style 与窗口视觉树的长生命周期引用。
/// </summary>
internal static class ModernWindowLifecycle
{
    private const string TitleBarWindowPropertyChangedMethodName = "_window_ButtonAvailabilityShouldUpdate";
    private static readonly DependencyPropertyDescriptor WindowStyleDescriptor =
        DependencyPropertyDescriptor.FromProperty(Window.WindowStyleProperty, typeof(Window));
    private static readonly DependencyPropertyDescriptor ResizeModeDescriptor =
        DependencyPropertyDescriptor.FromProperty(Window.ResizeModeProperty, typeof(Window));
    private static readonly MethodInfo? TitleBarWindowPropertyChangedMethod = typeof(TitleBarControl).GetMethod(
        TitleBarWindowPropertyChangedMethodName,
        BindingFlags.Instance | BindingFlags.NonPublic);

    /// <summary>
    /// 在窗口关闭前移除 TitleBarControl 对父窗口 WindowStyle/ResizeMode 的监听并拆除模板。
    /// </summary>
    internal static void DetachModernWindowStyle(Window window)
    {
        RemoveModernTitleBarHandlers(window);
        WindowHelper.SetUseModernWindowStyle(window, false);
        window.Template = new ControlTemplate(typeof(Window));
        window.ApplyTemplate();
        window.UpdateLayout();
    }

    /// <summary>
    /// 断开窗口内容、输入绑定和 DataContext，释放视觉树对业务对象的引用。
    /// </summary>
    internal static void DetachVisualTree(Window window)
    {
        if (window.Content is Panel panel)
        {
            for (int i = panel.Children.Count - 1; i >= 0; i--)
                panel.Children[i].ClearValue(FrameworkElement.DataContextProperty);

            panel.Children.Clear();
        }

        window.InputBindings.Clear();
        window.DataContext = null;
        window.Content = null;
    }

    private static void RemoveModernTitleBarHandlers(DependencyObject parent)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is TitleBarControl titleBar &&
                TitleBarWindowPropertyChangedMethod?.CreateDelegate<EventHandler>(titleBar) is { } handler &&
                Window.GetWindow(titleBar) is { } window)
            {
                WindowStyleDescriptor.RemoveValueChanged(window, handler);
                ResizeModeDescriptor.RemoveValueChanged(window, handler);
            }

            RemoveModernTitleBarHandlers(child);
        }
    }
}
