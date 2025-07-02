using CommunityToolkit.Mvvm.Input;
using MauiApp.Models;

namespace MauiApp.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}