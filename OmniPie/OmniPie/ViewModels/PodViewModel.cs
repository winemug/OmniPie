using System.Windows.Input;
using Xamarin.Forms;

namespace OmniPie.ViewModels
{
    public class PodViewModel : BaseViewModel
    {
        public decimal TempBasalRate { get; set; } = 0m;
        public decimal TempBasalDuration { get; set; } = 2m;

        public ICommand StatusCommand { get; set; }
        public ICommand SetTempBasalCommand { get; set;}
        public ICommand CancelTempBasalCommand { get; set; }

        public PodViewModel()
        {
            StatusCommand = new Command(async () => DebugOut = await Client.UpdateStatus(), () => true);
            SetTempBasalCommand = new Command(async () => DebugOut = await Client.SetTempBasal(TempBasalRate, TempBasalDuration), () => true);
            CancelTempBasalCommand = new Command(async () => DebugOut = await Client.CancelTempBasal(), () => true);

        }
    }
}