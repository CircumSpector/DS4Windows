﻿using Vapour.Client.Core.View;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Vapour.Client.Core.ViewModel;

public abstract class ViewModel<TViewModel> : ObservableObject, IViewModel<TViewModel>
    where TViewModel : IViewModel<TViewModel>
{
    public virtual async Task Initialize()
    {
        await Task.FromResult(0);
    }

    public List<IView> Views { get; } = new();
    public object? MainView => Views.FirstOrDefault();

    public void AddView(IView view)
    {
        Views?.Add(view);
        OnPropertyChanged(nameof(MainView));
    }

    #region Dispose

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~ViewModel()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}