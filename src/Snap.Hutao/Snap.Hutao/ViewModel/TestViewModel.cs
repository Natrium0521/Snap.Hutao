﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Caching.Memory;
using Snap.Hutao.Core.Caching;
using Snap.Hutao.Core.ExceptionService;
using Snap.Hutao.Core.LifeCycle;
using Snap.Hutao.Core.Setting;
using Snap.Hutao.Core.Windowing;
using Snap.Hutao.Service.Notification;
using Snap.Hutao.ViewModel.Guide;
using Snap.Hutao.Web.Hutao.HutaoAsAService;

namespace Snap.Hutao.ViewModel;

/// <summary>
/// 测试视图模型
/// </summary>
[HighQuality]
[ConstructorGenerated]
[Injection(InjectAs.Scoped)]
internal sealed partial class TestViewModel : Abstraction.ViewModel
{
    private readonly HutaoAsAServiceClient homaAsAServiceClient;
    private readonly IInfoBarService infoBarService;
    private readonly ICurrentXamlWindowReference currentXamlWindowReference;
    private readonly ILogger<TestViewModel> logger;
    private readonly IMemoryCache memoryCache;
    private readonly ITaskContext taskContext;

    private UploadAnnouncement announcement = new();

    public UploadAnnouncement Announcement { get => announcement; set => SetProperty(ref announcement, value); }

    public bool SuppressMetadataInitialization
    {
        get => LocalSetting.Get(SettingKeys.SuppressMetadataInitialization, false);
        set
        {
            if (IsViewDisposed)
            {
                return;
            }

            LocalSetting.Set(SettingKeys.SuppressMetadataInitialization, value);
        }
    }

    public bool OverrideElevationRequirement
    {
        get => LocalSetting.Get(SettingKeys.OverrideElevationRequirement, false);
        set
        {
            if (IsViewDisposed)
            {
                return;
            }

            LocalSetting.Set(SettingKeys.OverrideElevationRequirement, value);
        }
    }

    public bool OverrideUpdateVersionComparison
    {
        get => LocalSetting.Get(SettingKeys.OverrideUpdateVersionComparison, false);
        set
        {
            if (IsViewDisposed)
            {
                return;
            }

            LocalSetting.Set(SettingKeys.OverrideUpdateVersionComparison, value);
        }
    }

    public bool OverridePackageConvertDirectoryPermissionsRequirement
    {
        get => LocalSetting.Get(SettingKeys.OverridePackageConvertDirectoryPermissionsRequirement, false);
        set
        {
            if (IsViewDisposed)
            {
                return;
            }

            LocalSetting.Set(SettingKeys.OverridePackageConvertDirectoryPermissionsRequirement, value);
        }
    }

    [Command("ResetGuideStateCommand")]
    private static void ResetGuideState()
    {
        UnsafeLocalSetting.Set(SettingKeys.Major1Minor10Revision0GuideState, GuideState.Language);
    }

    [Command("ExceptionCommand")]
    private static void ThrowTestException()
    {
        HutaoException.Throw("Test Exception");
    }

    [Command("ResetMainWindowSizeCommand")]
    private void ResetMainWindowSize()
    {
        if (currentXamlWindowReference.Window is MainWindow mainWindow)
        {
            double scale = mainWindow.GetRasterizationScale();
            mainWindow.AppWindow.Resize(new Windows.Graphics.SizeInt32(1372, 772).Scale(scale));
        }
    }

    [Command("UploadAnnouncementCommand")]
    private async Task UploadAnnouncementAsync()
    {
        Web.Response.Response response = await homaAsAServiceClient.UploadAnnouncementAsync(Announcement).ConfigureAwait(false);
        if (response.IsOk())
        {
            infoBarService.Success(response.Message);
            await taskContext.SwitchToMainThreadAsync();
            Announcement = new();
        }
    }

    [Command("CompensationGachaLogServiceTimeCommand")]
    private async Task CompensationGachaLogServiceTimeAsync()
    {
        Web.Response.Response response = await homaAsAServiceClient.GachaLogCompensationAsync(15).ConfigureAwait(false);
        if (response.IsOk())
        {
            infoBarService.Success(response.Message);
            await taskContext.SwitchToMainThreadAsync();
            Announcement = new();
        }
    }

    [Command("DebugPrintImageCacheFailedDownloadTasksCommand")]
    private void DebugPrintImageCacheFailedDownloadTasks()
    {
        if (memoryCache.TryGetValue($"{nameof(ImageCache)}.FailedDownloadTasks", out HashSet<string>? set))
        {
            logger.LogInformation("Failed ImageCache download tasks: [{Tasks}]", set?.ToString(','));
        }
    }
}