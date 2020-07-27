using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

// ReSharper disable once CheckNamespace
namespace DotNetHelper.Database.Tests
{
	public static class Skip
	{
		public static void Inconclusive(string reason = "inconclusive")
			=> throw new SkipTestException(reason);

		public static void If<T>(object obj, string reason = null)
			where T : class
		{
			if (obj is T)
				Skip.Inconclusive(reason ?? $"not valid for {typeof(T).FullName}");
		}
	}

#pragma warning disable RCS1194 // Implement exception constructors.
	[Serializable]
	public class SkipTestException : Exception
	{
		public SkipTestException(string reason) : base(reason)
		{
		}

		public SkipTestException()
		{
		}

		public SkipTestException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected SkipTestException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
		{
			//throw new NotImplementedException();
		}
	}
#pragma warning restore RCS1194 // Implement exception constructors.

	public class FactDiscoverer : Xunit.Sdk.FactDiscoverer
	{
		public FactDiscoverer(IMessageSink diagnosticMessageSink) : base(diagnosticMessageSink) { }

		protected override IXunitTestCase CreateTestCase(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
			=> new SkippableTestCase(DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod);
	}

	public class TheoryDiscoverer : Xunit.Sdk.TheoryDiscoverer
	{
		public TheoryDiscoverer(IMessageSink diagnosticMessageSink) : base(diagnosticMessageSink) { }

		protected override IEnumerable<IXunitTestCase> CreateTestCasesForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute, object[] dataRow)
			=> new[] { new SkippableTestCase(DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, dataRow) };

		protected override IEnumerable<IXunitTestCase> CreateTestCasesForSkip(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute, string skipReason)
			=> new[] { new SkippableTestCase(DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod) };

		protected override IEnumerable<IXunitTestCase> CreateTestCasesForTheory(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute)
			=> new[] { new SkippableTheoryTestCase(DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod) };

		protected override IEnumerable<IXunitTestCase> CreateTestCasesForSkippedDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute, object[] dataRow, string skipReason)
			=> new[] { new NamedSkippedDataRowTestCase(DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, skipReason, dataRow) };
	}

	public class SkippableTestCase : XunitTestCase
	{
		protected override string GetDisplayName(IAttributeInfo factAttribute, string displayName) =>
			base.GetDisplayName(factAttribute, displayName).StripName();

		[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
		public SkippableTestCase() { }

		public SkippableTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod, object[] testMethodArguments = null)
			: base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments)
		{
		}


	}

	public class SkippableTheoryTestCase : XunitTheoryTestCase
	{
		protected override string GetDisplayName(IAttributeInfo factAttribute, string displayName) =>
			base.GetDisplayName(factAttribute, displayName).StripName();

		[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
		public SkippableTheoryTestCase() { }

		public SkippableTheoryTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod)
			: base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod) { }


	}

	public class NamedSkippedDataRowTestCase : XunitSkippedDataRowTestCase
	{
		protected override string GetDisplayName(IAttributeInfo factAttribute, string displayName) =>
			base.GetDisplayName(factAttribute, displayName).StripName();

		[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
		public NamedSkippedDataRowTestCase() { }

		public NamedSkippedDataRowTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod, string skipReason, object[] testMethodArguments = null)
		: base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, skipReason, testMethodArguments) { }
	}

	internal static class XUnitExtensions
	{
		internal static string StripName(this string name) =>
			name.Replace(" DotNetHelper.Database.Tests.", "");


	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class SkippableFactAttribute : FactAttribute
	{
	}
}