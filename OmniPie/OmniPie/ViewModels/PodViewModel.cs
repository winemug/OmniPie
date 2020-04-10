using System.Windows.Input;
using Xamarin.Forms;

namespace OmniPie.ViewModels
{
    public class PodViewModel : BaseViewModel
    {
        public decimal TempBasalRate { get; set; } = 0m;
        public decimal TempBasalDuration { get; set; } = 2m;

        public string RadioAddress { get; set; }
        public int Lot { get; set; }
        public int Serial { get; set; }
        public ICommand StatusCommand { get; set; }
        public ICommand SetTempBasalCommand { get; set;}
        public ICommand CancelTempBasalCommand { get; set; }

        public ICommand AdoptCommand { get; set; }

        public PodViewModel(Page page) : base(page)
        {
            StatusCommand = new Command(async () =>
            {
                if (await page.DisplayAlert("Confirm command", "Update status", "yes", "no"))
                {
                    DebugOut = await Client.UpdateStatus();
                }
            });
            SetTempBasalCommand = new Command(async () =>
            {
                if (await page.DisplayAlert("Confirm command", $"Set basal rate to {TempBasalRate} U/h for {TempBasalDuration} hours?", "yes", "no"))
                {
                    DebugOut = await Client.SetTempBasal(TempBasalRate, TempBasalDuration);
                }
                
            });
            CancelTempBasalCommand = new Command(async () =>
            {
                if (await page.DisplayAlert("Confirm command", $"Cancel temp basal rate?", "yes", "no"))
                {
                    DebugOut = await Client.CancelTempBasal();
                }
            });
            AdoptCommand = new Command(async () =>
            {
                if (await page.DisplayAlert("Confirm command", $"Adopt a new pod?", "yes", "no"))
                {
                    DebugOut = await Client.AdoptPod(Lot, Serial, RadioAddress);
                }
               
            });

        }
    }
}