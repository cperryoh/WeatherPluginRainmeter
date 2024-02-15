using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using Rainmeter;

// Overview: This is a blank canvas on which to build your plugin.

// Note: GetString, ExecuteBang and an unnamed function for use as a section variable
// have been commented out. If you need GetString, ExecuteBang, and/or section variables 
// and you have read what they are used for from the SDK docs, uncomment the function(s)
// and/or add a function name to use for the section variable function(s). 
// Otherwise leave them commented out (or get rid of them)!

namespace WeatherPlugin
{
    public class Current
    {
        public double temp { get; set; }
        public double temp_max { get; set; }
        public double temp_min { get; set; }
        public double humidity { get; set; }
        public Weather[] weather { get; set; }
    }
    public class Root {
        public Current current { get; set; }
    }
    public class Weather
    {
        public String Current { get; set; }
        public String description { get; set; }
        public String icon { get; set; }
    }
    class Measure
    {
        static public implicit operator Measure(IntPtr data)
        {
            return (Measure)GCHandle.FromIntPtr(data).Target;
        }

        public IntPtr buffer = IntPtr.Zero;
        public Root mainJson;
        public API api;
        public String apiKey;
        public double lon;
        public double lat;
        public String type;
        public String units;
        public String data;
    }
    public class Plugin
    {

        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
            api = (Rainmeter.API)rm;
            Measure measure = (Measure)data;


            //load inputs
            measure.apiKey = api.ReadString("key", "");
            measure.lon = api.ReadDouble("longitude", 0.0);
            measure.lat = api.ReadDouble("latitude", 0.0);
            measure.type = api.ReadString("type", "");
            measure.units = api.ReadString("units", "f");
            api.Log(API.LogType.Debug, "Skin loaded");

            measure.api = api;
            try
            {

                //get json from api
                String json = new WebClient().DownloadString("https://"+$"api.openweathermap.org/data/2.5/onecall?lat={measure.lat}&lon={measure.lon}&appid={measure.apiKey}");
                Root weather = (new JavaScriptSerializer()).Deserialize<Root>(json);
                measure.mainJson = weather;

                //determain which object we are going to return
                if (measure.type.Equals("temp") || measure.type.Equals(""))
                {
                    measure.data = convert(weather.current.temp, measure.units).ToString();
                    api.Log(API.LogType.Debug, "Skin getting temp");
                }
                if (measure.type.Equals("temp_min"))
                {
                    measure.data = convert(weather.current.temp_min, measure.units).ToString();
                    api.Log(API.LogType.Debug, "Skin getting temp_min");
                }
                if (measure.type.Equals("temp_max"))
                {
                    measure.data = convert(weather.current.temp_max, measure.units).ToString();
                    api.Log(API.LogType.Debug, "Skin getting temp_man");
                }
                if (measure.type.Equals("humidity"))
                {
                    measure.data = weather.current.humidity.ToString();
                    api.Log(API.LogType.Debug, "Skin getting humidity");
                }
                if (measure.type.Equals("description"))
                {
                    measure.data = weather.current.weather[0].description;
                    api.Log(API.LogType.Debug, "Skin getting description");
                }
            }
            catch (Exception e)
            {
                api.Log(API.LogType.Error, "Skin failed to load");
                api.Log(API.LogType.Error, e.Message);
            }
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            Measure measure = (Measure)data;
            if (measure.buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(measure.buffer);
            }
            GCHandle.FromIntPtr(data).Free();
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            Measure measure = (Measure)data;
            api = (Rainmeter.API)rm;
            measure.apiKey = api.ReadString("key", "");
            measure.lon = api.ReadDouble("longitude", 0.0);
            measure.lat = api.ReadDouble("latitude", 0.0);
            measure.type = api.ReadString("type", "");
            measure.units = api.ReadString("units", "f");
            api.Log(API.LogType.Debug, "Skin reloaded");
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            Measure measure = (Measure)data;

            try
            {
                String json = new WebClient().DownloadString("https://" + $"api.openweathermap.org/data/2.5/onecall?lat={measure.lat}&lon={measure.lon}&appid={measure.apiKey}");
                Root weather = (new JavaScriptSerializer()).Deserialize<Root>(json);
                measure.mainJson = weather;
                if (measure.type.Equals("temp") || measure.type.Equals(""))
                {
                    measure.data = convert(weather.current.temp, measure.units).ToString();
                    return convert(weather.current.temp, measure.units);
                }
                if (measure.type.Equals("temp_min"))
                {
                    measure.data = convert(weather.current.temp_min, measure.units).ToString();
                    return convert(weather.current.temp_min, measure.units);
                }
                if (measure.type.Equals("temp_max"))
                {
                    measure.data = convert(weather.current.temp_max, measure.units).ToString();
                    return convert(weather.current.temp_max, measure.units);
                }
                if (measure.type.Equals("humidity"))
                {
                    measure.data = weather.current.humidity.ToString();
                    return weather.current.humidity;
                }
                if (measure.type.Equals("description"))
                {
                    measure.data = weather.current.weather[0].description;
                }
                return convert(weather.current.temp, measure.units);
            }
            catch (Exception e)
            {
                return -1;
            }
        }
        public static double convert(double val, String units)
        {
            if (units.ToLower().Equals("c"))
            {
                return Math.Round(toC(val));
            }
            else if(units.ToLower().Equals("f"))
            {
                return Math.Round(toF(val));
            }
            else
            {
                api.Log(API.LogType.Error, "Unit choice is incorrect, please choose c for celcius and f for fahrenheit");
                return 0;
            }
        }
        public static double toF(double val)
        {
            return (toC(val) * (9.0 / 5.0)) + 32.0;
        }
        public static double toC(double val)
        {
            return val - 273.15;
        }
        [DllExport]
        public static IntPtr GetString(IntPtr data)
        {
            Measure measure = (Measure)data;
            if (measure.buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(measure.buffer);
                measure.buffer = IntPtr.Zero;
            }

            return Marshal.StringToHGlobalUni(measure.data);
        }
        static API api;
        //[DllExport]
        //public static void ExecuteBang(IntPtr data, [MarshalAs(UnmanagedType.LPWStr)]String args)
        //{
        //    Measure measure = (Measure)data;
        //}

        [DllExport]
        public static IntPtr getValue(IntPtr data, int argc,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 1)] string[] argv)
        {
            Measure measure = (Measure)data;
            string outVal = "";
            measure.api.Log(API.LogType.Debug, "Running get value");
            if (argv[0].Equals("temp") || argv[0].Equals(""))
            {
                outVal = convert(measure.mainJson.current.temp, measure.units).ToString();
            }
            if (argv[0].Equals("temp_min"))
            {
                outVal = convert(measure.mainJson.current.temp_min, measure.units).ToString();
            }
            if (argv[0].Equals("temp_max"))
            {
                outVal = convert(measure.mainJson.current.temp_max, measure.units).ToString();
            }
            if (argv[0].Equals("humidity"))
            {
                outVal = measure.mainJson.current.humidity.ToString();
            }
            if (argv[0].Equals("description"))
            {
                outVal = measure.mainJson.current.weather[0].description;
            }
            if (argv[0].Equals("iconUrl"))
            {
                outVal = "http://" + $"openweathermap.org/img/wn/{measure.mainJson.current.weather[0].icon}@2x.png";
            }
            if (argv[0].Equals("icon"))
            {
                outVal = measure.mainJson.current.weather[0].icon;
            }
            measure.api.Log(API.LogType.Debug, $"Returning {outVal}");
            return Marshal.StringToHGlobalUni(outVal);
        }
    }
}   