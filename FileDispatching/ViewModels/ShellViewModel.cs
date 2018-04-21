using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FileDispatching.ViewModels
{
    public class ShellViewModel : Screen
    {
        private readonly SimpleContainer _container;
        private INavigationService _navigationService;

        public ShellViewModel(SimpleContainer container)
        {
            this._container = container;
        }

        public void RegisterFrame(Frame frame)
        {
            _navigationService = new FrameAdapter(frame);
            _container.Instance(_navigationService);
            _navigationService.NavigateToViewModel(typeof(StartViewModel));
        }
    }
}
