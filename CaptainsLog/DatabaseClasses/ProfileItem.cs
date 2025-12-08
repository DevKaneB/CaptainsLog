using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptainsLog.DatabaseClasses
{
    public class ProfileItem : INotifyPropertyChanged
    {
        private string imageSource;

        public string ImageSource
        {
            get => imageSource;
            set
            {
                if (imageSource == value) return;
                imageSource = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageSource)));
            }
        }

        public string? BoatName { get; set; }
        public int BuildYear { get; set; }
        public int RegNumber { get; set; }

        public float Length { get; set; }
        public float Beam { get; set; }
        public float Draft { get; set; }
   
        public DateTime LicenseExpiryDate { get; set; }
        public DateTime InsuranceExpiryDate { get; set; }
        public DateTime BSSExpiryDate { get; set; }
        public DateTime LastHullServiceDate { get; set; }

        public int EngineServiceIntervalHours { get; set; }
        public int NextEngineServiceAtHours { get; set; }
        public DateTime LastServiceDate { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
