using AppUpdatedXamarin.ViewModels;
using Xamarin.Forms;

namespace AppUpdatedXamarin.Models
{
    public class ProductSearchHandler : SearchHandler
    {
        public ItemsViewModel viewModel;

        protected override void OnQueryChanged(string oldValue, string newValue)
        {
            base.OnQueryChanged(oldValue, newValue);
            viewModel.SearchParameter = Query;
        }
        protected override void OnQueryConfirmed()
        {
            base.OnQueryConfirmed();
            viewModel.IsBusy = false;
            viewModel.IsBusy = true;
        }

        protected override void OnItemSelected(object item)
        {
            base.OnItemSelected(item);
        }
    }
}
