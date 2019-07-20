using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;

namespace Design.ViewModels
{
    public class ViewModelLocator
    {
        public const string HometaskPage = "Hometask";

        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            var nav = new NavigationService();
            nav.Configure(HometaskPage, typeof(Hometask));

            //Register your services used here
            SimpleIoc.Default.Register<INavigationService>(() => nav);
            SimpleIoc.Default.Register<Hometask>();
            SimpleIoc.Default.Register<ViewModel>();
        }

        // <summary>
        // Gets the FirstPage view model.
        // </summary>
        // <value>
        // The FirstPage view model.
        // </value>
        public Hometask HometaskPageInstance
        {
            get
            {
                return ServiceLocator.Current.GetInstance<Hometask>();
            }
        }

        public ViewModel ViewModel
        {
            get { return ServiceLocator.Current.GetInstance<ViewModel>(); }
        }
    }
}
