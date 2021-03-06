﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vahti.Mobile.Forms.Models;
using Vahti.Shared.Data;
using Vahti.Shared.DataProvider;
using Vahti.Shared.Enum;
using Vahti.Shared.TypeData;
using Xamarin.Essentials;

namespace Vahti.Mobile.Forms.Services
{
    /// <summary>
    /// Data service used to get and handle current location data
    /// </summary>
    public class LocationDataService : IDataService<Models.Location>
    {
        private readonly IDataProvider _dataProvider;
        private List<Models.Location> _locations;

        public LocationDataService(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;            
        }

        public async Task<IReadOnlyList<Models.Location>> GetAllDataAsync(bool forceRefresh)
        {            
            if (forceRefresh || _locations == null)
            {
                await LoadAll();
            }         
         
            return _locations;         
        }

        public async Task<Models.Location> GetDataAsync(string id, bool forceRefresh)
        {            
            if (forceRefresh || _locations == null)
            {
                await LoadAll();
            }

            return _locations.FirstOrDefault(l => l.Name.Equals(id));
        }

        public Task UpdateAsync(Models.Location location)
        {
            foreach (var measurement in location)
            {
                Preferences.Set(GetKeyName(location.Name, measurement.SensorId), measurement.IsVisible);
            }
            return Task.CompletedTask;
        }

        private string GetKeyName(string locationName, string measurementName)
        {
            return $"{locationName}£${measurementName}";
        }

        private async Task<IEnumerable<Models.Location>> LoadAll()
        {
            if (_locations == null)
            {
                _locations = new List<Models.Location>();
            }
            else
            {
                _locations.Clear();
            }            

            var sensorDeviceTypes = (await _dataProvider.LoadAllItemsAsync<SensorDeviceType>()).ToDictionary(p => p.Id);
            var sensorDevices = (await _dataProvider.LoadAllItemsAsync<SensorDevice>()).ToDictionary(p => p.Id);

            foreach (var locationData in await _dataProvider.LoadAllItemsAsync<LocationData>())
            {
                var measurements = new List<Measurement>();

                foreach (var m in locationData.Measurements)
                {                    
                    string unit;
                    string sensorName;
                    var sensor = sensorDeviceTypes[sensorDevices[m.SensorDeviceId].SensorDeviceTypeId].Sensors.FirstOrDefault(t => t.Id.Equals(m.SensorId, StringComparison.OrdinalIgnoreCase));
                    var succeeded = Enum.TryParse<SensorClass>(sensor?.Class, true, out var category);
                                        
                    if (succeeded)
                    {
                        unit = sensorDeviceTypes[sensorDevices[m.SensorDeviceId].SensorDeviceTypeId].Sensors.FirstOrDefault(t => t.Id.Equals(m.SensorId, StringComparison.OrdinalIgnoreCase)).Unit;
                        sensorName = sensor.Name;
                    }                    
                    else
                    {
                        // A custom measurement
                        var custom = sensorDevices[m.SensorDeviceId].CalculatedMeasurements?.FirstOrDefault(c => c.Id.Equals(m.SensorId, StringComparison.OrdinalIgnoreCase));
                        Enum.TryParse<SensorClass>(custom.Class, true, out category);
                        unit = custom.Unit;
                        sensorName = custom.Name;
                    }                    
                    
                    measurements.Add(new Measurement()
                    {
                        SensorId = m.SensorId,
                        SensorDeviceId = m.SensorDeviceId,
                        SensorClass = category,
                        Unit = unit,
                        Value = m.Value,
                        SensorName = sensorName,
                        IsVisible = Preferences.Get(GetKeyName(locationData.Name, m.SensorId), true)
                    });                  

                }

                var location = new Models.Location(locationData.Name, locationData.Timestamp, locationData.UpdateInterval, measurements);
                _locations.Add(location);
            }

            return _locations;
        }
    }
}