using System;
using Microsoft.UI.Xaml.Controls;

namespace Finexa_App.Services
{
    public interface INavigationService
    {
        Frame? Frame { get; set; }
        bool Navigate(Type pageType, object? parameter = null);
        bool GoBack();
    }

    public class NavigationService : INavigationService
    {
        public Frame? Frame { get; set; }

        public bool Navigate(Type pageType, object? parameter = null)
        {
            if (Frame == null) return false;
            return Frame.Navigate(pageType, parameter);
        }

        public bool GoBack()
        {
            if (Frame == null || !Frame.CanGoBack) return false;
            Frame.GoBack();
            return true;
        }
    }
}
