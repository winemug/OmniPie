using System;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OmniPie.Api;
using Xamarin.Forms;

namespace OmniPie.ViewModels
{
    public class PodViewModel : BaseViewModel
    {
        public decimal TempBasalRate { get; set; } = 0m;
        public decimal TempBasalDuration { get; set; } = 2m;

        
        public string BasalStatusString1 { get; set; }
        public string BasalStatusString2 { get; set; }
        public string BasalStatusString3 { get; set; }
        public DateTimeOffset? ActiveTempBasalStart { get; set; } = null;
        public TimeSpan ActiveTempBasalDuration { get; set; }
        public decimal ActiveTempBasalRate { get; set; }

        public string RadioAddress { get; set; }
        public int Lot { get; set; }
        public int Serial { get; set; }
        public ICommand StatusCommand { get; set; }
        public ICommand SetTempBasalCommand { get; set;}
        public ICommand CancelTempBasalCommand { get; set; }
        
        public decimal Delivered { get; set; }
        public decimal Reservoir { get; set; }

        public ICommand AdoptCommand { get; set; }

        public PodViewModel(Page page) : base(page)
        {
            StatusCommand = new Command(async () =>
            {
                if (await page.DisplayAlert("Confirm command", "Update status", "yes", "no"))
                {
                    ParseOutput(await Client.UpdateStatus());
                }
            });
            SetTempBasalCommand = new Command(async () =>
            {
                if (await page.DisplayAlert("Confirm command", $"Set basal rate to {TempBasalRate} U/h for {TempBasalDuration} hours?", "yes", "no"))
                {
                    ParseOutput(await Client.SetTempBasal(TempBasalRate, TempBasalDuration));
                }
                
            });
            CancelTempBasalCommand = new Command(async () =>
            {
                if (await page.DisplayAlert("Confirm command", $"Cancel temp basal rate?", "yes", "no"))
                {
                    ParseOutput(await Client.CancelTempBasal());
                }
            });
            AdoptCommand = new Command(async () =>
            {
                if (await page.DisplayAlert("Confirm command", $"Adopt a new pod?", "yes", "no"))
                {
                    ParseOutput(await Client.AdoptPod(Lot, Serial, RadioAddress));
                }
               
            });

        }

        void ParseOutput(string output)
        {
            try
            {
                dynamic o = JsonConvert.DeserializeObject(output);
                Delivered = o.status.insulin_delivered;
                Reservoir = o.status.insulin_reservoir;
                ActiveTempBasalStart = DateTimeOffset.FromUnixTimeMilliseconds((long) ((double)o.status.last_enacted_temp_basal_start * 1000));
                ActiveTempBasalDuration = TimeSpan.FromHours((double) o.status.last_enacted_temp_basal_duration);
                ActiveTempBasalRate = (decimal) o.status.last_enacted_temp_basal_amount;
            }
            catch (Exception e)
            {
            }

            DebugOut = output;

            BasalStatusString1 = "Scheduled basal rate active";
            BasalStatusString2 = "";
            BasalStatusString3 = "";
            
            if (ActiveTempBasalStart.HasValue)
            {
                var started = ActiveTempBasalStart.Value;
                var ending = started + ActiveTempBasalDuration;
                var now = DateTimeOffset.UtcNow;
                if (ending > now)
                {
                    BasalStatusString1 = $"Temporary rate: {ActiveTempBasalRate}U/h";
                    BasalStatusString2 = $"Running {now - started} Remaining {ending - now}";
                    BasalStatusString3 = $"Ends at {ending.LocalDateTime}";
                }
            }
                 
        }
    }
}