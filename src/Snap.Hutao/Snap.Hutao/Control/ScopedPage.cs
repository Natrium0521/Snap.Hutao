﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Snap.Hutao.Service.Navigation;
using Snap.Hutao.ViewModel.Abstraction;

namespace Snap.Hutao.Control;

[HighQuality]
[SuppressMessage("", "CA1001")]
internal class ScopedPage : Page
{
    private readonly RoutedEventHandler unloadEventHandler;
    private readonly CancellationTokenSource viewCancellationTokenSource = new();
    private readonly IServiceScope pageScope;

    private bool inFrame = true;

    protected ScopedPage()
    {
        unloadEventHandler = OnUnloaded;
        Unloaded += unloadEventHandler;
        pageScope = Ioc.Default.GetRequiredService<IScopedPageScopeReferenceTracker>().CreateScope();
    }

    public async ValueTask NotifyRecipientAsync(INavigationData extra)
    {
        if (extra.Data is not null && DataContext is INavigationRecipient recipient)
        {
            await recipient.ReceiveAsync(extra).ConfigureAwait(false);
        }

        extra.NotifyNavigationCompleted();
    }

    /// <summary>
    /// 初始化
    /// 应当在 InitializeComponent() 前调用
    /// </summary>
    /// <typeparam name="TViewModel">视图模型类型</typeparam>
    protected void InitializeWith<TViewModel>()
        where TViewModel : class, IViewModel
    {
        try
        {
            IViewModel viewModel = pageScope.ServiceProvider.GetRequiredService<TViewModel>();
            viewModel.CancellationToken = viewCancellationTokenSource.Token;
            DataContext = viewModel;
        }
        catch (Exception ex)
        {
            pageScope.ServiceProvider.GetRequiredService<ILogger<ScopedPage>>().LogError(ex, "Failed to initialize view model.");
            throw;
        }
    }

    /// <inheritdoc/>
    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        DisposeViewModel();
        inFrame = false;
    }

    /// <inheritdoc/>
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is INavigationData extra)
        {
            NotifyRecipientAsync(extra).SafeForget();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (inFrame)
        {
            DisposeViewModel();
        }

        if (this.IsDisposed())
        {
            return;
        }

        Unloaded -= unloadEventHandler;
    }

    private void DisposeViewModel()
    {
        using (viewCancellationTokenSource)
        {
            // Cancel all tasks executed by the view model
            viewCancellationTokenSource.Cancel();
            IViewModel viewModel = (IViewModel)DataContext;

            using (SemaphoreSlim locker = viewModel.DisposeLock)
            {
                // Wait to ensure viewmodel operation is completed
                locker.Wait();
                viewModel.IsViewDisposed = true;

                // Dispose the scope
                pageScope.Dispose();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true);
            }
        }
    }
}