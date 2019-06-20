using System;

namespace DotNetHelper.Database.Interface
{
    public interface IColumnSerializer
    {
        Func<object, string> Serialize { get; }
        Func<object, string> Deserialize { get; }
    }
}
