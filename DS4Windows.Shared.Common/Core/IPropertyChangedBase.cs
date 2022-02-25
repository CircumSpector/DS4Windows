using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace DS4Windows.Shared.Common.Core
{
    public interface IPropertyChangedBase<T> : INotifyPropertyChanged
        where T : IPropertyChangedBase<T>
    {
        void FirePropertyChanged<TValue>(Expression<Func<T, TValue>> propertySelector);
    }
}
