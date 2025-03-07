﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Xaml;

namespace Snap.Hutao.Core.Windowing.Abstraction;

internal interface IXamlWindowContentAsFrameworkElement
{
    FrameworkElement ContentAccess { get; }
}