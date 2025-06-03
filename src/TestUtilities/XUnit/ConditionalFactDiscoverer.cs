// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Borrowed from https://github.com/dotnet/aspnetcore/blob/95ed45c67/src/Testing/src/xunit/

using Xunit.Sdk;
using Xunit.v3;

// Do not change this namespace without changing the usage in ConditionalFactAttribute
namespace Microsoft.TestUtilities;

internal sealed class ConditionalFactDiscoverer : FactDiscoverer
{
    private readonly IMessageSink _diagnosticMessageSink;

    public ConditionalFactDiscoverer(IMessageSink diagnosticMessageSink)
        : base(diagnosticMessageSink)
    {
        _diagnosticMessageSink = diagnosticMessageSink;
    }

    protected override IXunitTestCase CreateTestCase(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, IFactAttribute factAttribute)
    {
        var skipReason = testMethod.EvaluateSkipConditions();
        return skipReason != null
            ? new SkippedTestCase(skipReason, _diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), TestMethodDisplayOptions.None, testMethod)
            : base.CreateTestCase(discoveryOptions, testMethod, factAttribute);
    }
}
