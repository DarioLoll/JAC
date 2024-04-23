using System;
using JAC.Shared;

namespace JAC.ViewModels;

public class MainViewModel : ViewModelBase
{
    public override void DisplayError(ErrorType error)
    {
        //temporarily display the error in the console
        Console.WriteLine(error.ToString());
    }
}