using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace App2
{
    public class MyCompassViewModel : MvvmHelpers.BaseViewModel
    {
        double current_latitude = 25.2618616;
        double current_longitude = 55.3254198;
        readonly double QiblaLatitude = 21.4224779;
        readonly double QiblaLongitude = 39.8251832;
        readonly SensorSpeed speed = SensorSpeed.UI;
        public MyCompassViewModel()
        {
            StopCommand = new Command(Stop);
            StartCommand = new Command(Start);
            LocationCommand = new Command(GetLocation);
        }
        string headingDisplay;
        public string HeadingDisplay
        {
            get => headingDisplay;
            set => SetProperty(ref headingDisplay, value);
        }

        double heading = 0;
        public double Heading
        {
            get => heading;
            set => SetProperty(ref heading, value);
        }
        string info;
        public string Info
        {
            get => info;
            set => SetProperty(ref info, value);
        }
        public Command StopCommand { get; }

        void Stop()
        {
            if (!Compass.IsMonitoring)
                return;

            Compass.ReadingChanged -= Compass_ReadingChanged;
            Compass.Stop();
        }


        public Command StartCommand { get; }

        void Start()
        {
            //if (Compass.IsMonitoring)
            //    return;
            
            //GetLocation();
            //Compass.ApplyLowPassFilter = true;
            Compass.ReadingChanged += Compass_ReadingChanged;
            //Compass.Start(SensorSpeed.UI);

        }
        public Command LocationCommand { get; }
        void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            try
            {
                var qiblaLocation = new Location(QiblaLatitude, QiblaLongitude);
                var position = new Location(current_latitude, current_longitude);
                var res = DistanceCalculator.Bearing(position, qiblaLocation);
                var TargetHeading = (360 - res) % 360;

                var currentHeading = 360 - e.Reading.HeadingMagneticNorth;
                Heading = currentHeading - TargetHeading;
                
                //var d = GetDegree() - e.Reading.HeadingMagneticNorth;
                //Heading = d;// e.Reading.HeadingMagneticNorth;
                //Info = HeadingDisplay = $"Heading: {Heading}";
                Info = string.Format("Lat: {0} Long: {1} degree:{2}", current_latitude, current_longitude, Heading);

            }catch(Exception ex){}
        }
        private async void GetLocation()
        {
            IsBusy = true;
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromMilliseconds(10));
                var location = await Geolocation.GetLocationAsync(request);
                if (location != null)
                {
                    current_latitude = location.Latitude;
                    current_longitude = location.Longitude;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally { IsBusy = false; }
        }
    }
    public static class DistanceCalculator
    {
        const double kDegreesToRadians = Math.PI / 180.0;
        const double kRadiansToDegrees = 180.0 / Math.PI;

        public static double Bearing(Location position, Location location)
        {
            double fromLong = position.Longitude * kDegreesToRadians;
            double toLong = location.Longitude * kDegreesToRadians;
            double toLat = location.Latitude * kDegreesToRadians;
            double fromLat = position.Latitude * kDegreesToRadians;

            double dlon = toLong - fromLong;
            double y = Math.Sin(dlon) * Math.Cos(toLat);
            double x = Math.Cos(fromLat) * Math.Sin(toLat) - Math.Sin(fromLat) * Math.Cos(toLat) * Math.Cos(dlon);

            double direction = Math.Atan2(y, x);

            // convert to degrees
            direction *= kRadiansToDegrees;
            // normalize
            double fraction = modf(direction + 360.0, direction);
            direction += fraction;

            if (direction > 360)
            {
                direction -= 360;
            }

            return direction;
        }
        private static double modf(double orig, double ipart)
        {
            return orig - (Math.Floor(orig));
        }
    }
}
