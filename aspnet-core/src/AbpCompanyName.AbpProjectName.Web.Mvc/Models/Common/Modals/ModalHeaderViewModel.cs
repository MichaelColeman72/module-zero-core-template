namespace AbpCompanyName.AbpProjectName.Web.Models.Common.Modals
{
    public class ModalHeaderViewModel
    {
        public ModalHeaderViewModel(string title)
        {
            Title = title;
        }

        public string Title { get; set; }
    }
}
