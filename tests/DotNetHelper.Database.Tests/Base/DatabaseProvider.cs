using System;
using DotNetHelper.Database.Tests.Base.Providers;

public static class DatabaseProvider<TProvider> where TProvider : IDatabaseProvider
{
	public static TProvider Instance { get; } = Activator.CreateInstance<TProvider>();
}