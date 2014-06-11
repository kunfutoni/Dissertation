using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.Entity;

namespace SolarApplication
{
    class ReadingRepository : ConnectionClass
    {
        public ReadingRepository()
            : base()
        { }
        public string getMeterByID(int id)
        {
            return Entity.meters.SingleOrDefault(m => m.ID == id).Name;
        }
        public List<reading> getGeneratedReadings()
        {
            return Entity.readings.Where(g => g.ReadingTypeID == 1).ToList();
        }
        public double getMetersSum(int id, DateTime startDate, DateTime endDate)
        {
            try
            {
                return Entity.readings.Where(g => g.MeterID == id && g.readTime >= startDate && g.readTime <= endDate).Sum(g => g.readValue);
            }
            catch
            {
                return 0;
            }
        }
        public double getLowestMeterReadingValue(int meterId, DateTime startDate, DateTime endDate)
        {
            List<reading> read = Entity.readings.Where(m => m.MeterID == meterId && m.readTime >= startDate && m.readTime <= endDate).ToList();
            if (read.Count == 0)
                return 0;
            return read.Min(m => m.readValue);
            
        }
        public List<reading> getGeneratedReadingsByDates(DateTime startDate, DateTime endDate)
        {
            return Entity.readings.Where(g => g.ReadingTypeID == 1 && g.readTime >= startDate && g.readTime <= endDate).ToList();
        }
        public double getConsumedPower()
        {
            //List<reading> readings  = Entity.readings.Where(g => g.MeterID ==5).ToList();
            //double consumed =0;
            //foreach (reading r in readings)
            //{
            //    consumed += r.readValue;
            //}
            //return consumed;
            return Entity.readings.Where(g => g.MeterID == 5).Sum(g => g.readValue);
        }


        public double getBatteryHealth()
        {
            List<reading> battery = Entity.readings.Where(g => g.MeterID == 4).ToList();

            return battery.Last().readValue;
        }
        public int weatherForecast(DateTime date)
        {
            try
            {
                forecast weather = Entity.forecasts.SingleOrDefault(f => f.ForecastDate == date);
                return weather.WeatherTypeID;
            }
            catch
            {
                return 0;
            }
        }
    }
}
