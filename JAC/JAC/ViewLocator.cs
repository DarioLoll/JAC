using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using FluentAvalonia.UI.Controls;
using JAC.ViewModels;

namespace JAC;

public class ViewLocator : IDataTemplate, INavigationPageFactory
{
    public Control? Build(object? data)
    {
        if (data is null)
            return null;

        var name = data.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type != null)
        {
            var control = (Control)Activator.CreateInstance(type)!;
            control.DataContext = data;
            return control;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }

    public Control GetPage(Type srcType)
    {
        return null;
    }

    public Control GetPageFromObject(object target)
    {
        return Build(target)!;
    }
}