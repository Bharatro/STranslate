using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using STranslate.Helpers;
using STranslate.ViewModels;
using System.ComponentModel;

namespace STranslate.Views;

public partial class ImageTranslateWindow
{
    private readonly ImageTranslateWindowViewModel _viewModel;
    private readonly IServiceScope _serviceScope;

    public ImageTranslateWindow()
    {
        _serviceScope = Ioc.Default.CreateScope();
        try
        {
            _viewModel = _serviceScope.ServiceProvider.GetRequiredService<ImageTranslateWindowViewModel>();
            DataContext = _viewModel;

            InitializeComponent();
        }
        catch
        {
            _serviceScope.Dispose();
            throw;
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        _viewModel.CancelOperations();
        base.OnClosing(e);

        if (!e.Cancel)
            ModernWindowLifecycle.DetachModernWindowStyle(this);
    }

    protected override void OnClosed(EventArgs e)
    {
        try
        {
            ModernWindowLifecycle.DetachVisualTree(this);
        }
        finally
        {
            try
            {
                // VM 由独立 DI scope 持有，只释放 scope，避免 root provider 跟踪
                // Transient + IDisposable 的 VM 并将其保留到应用退出。
                _serviceScope.Dispose();
            }
            finally
            {
                base.OnClosed(e);
            }
        }
    }

}
