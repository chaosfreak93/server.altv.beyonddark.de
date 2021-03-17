using System.IO;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json.Linq;

namespace BeyondCore.VehicleStuff
{
    public static class VehicleSystem
    {
        public static async void SetTank(IPlayer player, IVehicle vehicle, int tank) {
            if (vehicle == null || !vehicle.Exists || vehicle.IsDestroyed) return;

            await vehicle.SetStreamSyncedMetaDataAsync("tank", tank);
        }

        public static async void GetTank(IPlayer player, IVehicle vehicle) {
            if (vehicle == null || !vehicle.Exists || vehicle.IsDestroyed) return;

            vehicle.GetStreamSyncedMetaData("tank", out int tank);
            await player.EmitAsync("getTank", tank);
        }

        public static async void IsEngineRunning(IPlayer player, IVehicle vehicle) {
            if (vehicle == null || !vehicle.Exists || vehicle.IsDestroyed) return;

            await player.EmitAsync("isEngineRunning", vehicle.EngineOn);
        }

        public static async void ToggleEngine(IPlayer player, IVehicle vehicle) {
            if (vehicle == null || !vehicle.Exists || vehicle.IsDestroyed) return;

            vehicle.GetSyncedMetaData("engine", out bool state);

            if (!state) {
                await vehicle.SetSyncedMetaDataAsync("engine", true);
                await player.EmitAsync("displayVehicleNotification", "Turned motor ~g~On");
            } else {
                await vehicle.SetSyncedMetaDataAsync("engine", false);
                await player.EmitAsync("displayVehicleNotification", "Turned motor ~r~Off");
            }
        }

        public static async void ToggleVehicleLock(IPlayer player, IVehicle vehicle) {
            if (vehicle == null || !vehicle.Exists || vehicle.IsDestroyed) return;

            var state = await vehicle.GetLockStateAsync();

            if (state == VehicleLockState.Locked) {
                await vehicle.SetLockStateAsync(VehicleLockState.Unlocked);
                await player.EmitAsync("displayVehicleNotification", "Your Vehicle is now ~g~Unlocked");
            } else {
                await vehicle.SetLockStateAsync(VehicleLockState.Locked);
                await player.EmitAsync("displayVehicleNotification", "Your Vehicle is now ~r~Locked");
            }

            await player.EmitAsync("LockStateAnimation");
        }

        public static async void ToggleSirenAudio(IPlayer player, IVehicle vehicle) {
            if (vehicle == null || !vehicle.Exists || vehicle.IsDestroyed) return;

            vehicle.GetStreamSyncedMetaData("sirenAudio", out bool state);

            if (!state) {
                await vehicle.SetSyncedMetaDataAsync("sirenAudio", true);
                await player.EmitAsync("displayVehicleNotification", "Turned siren ~r~Off");
            } else {
                await vehicle.SetSyncedMetaDataAsync("sirenAudio", false);
                await player.EmitAsync("displayVehicleNotification", "Turned siren ~g~On");
            }
        }

        public static async void RadioChanged(IPlayer player, IVehicle vehicle, string radioStation) {
            await vehicle.SetSyncedMetaDataAsync("radioStation", radioStation);
        }

        public static async void GetRadioStations(IPlayer player) {
            var radios = JArray.Parse(await File.ReadAllTextAsync("resources/LosAssets/content/data/radio.json"));

            foreach (var station in radios)
                await player.EmitAsync("radio:AddStation", station.ToString());
        }
    }
}