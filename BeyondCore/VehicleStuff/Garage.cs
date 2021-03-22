using System.IO;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json.Linq;

namespace BeyondCore.VehicleStuff
{
    public static class Garage
    {
        public static async void SpawnGarageVehicle(IPlayer player, string info) {
            var data = JArray.Parse(info);

            if (!bool.Parse(data[0]["parking"].ToString())) return;

            player.GetStreamSyncedMetaData("lastVehicle", out IVehicle lastVehicle);

            if (lastVehicle != null && lastVehicle.Exists) {
                lastVehicle.Remove();
                player.DeleteStreamSyncedMetaData("lastVehicle");
            }

            var garage = JArray.Parse(await File.ReadAllTextAsync("resources/LosAssets/content/data/position/garage.json"));

            IVehicle vehicle = null;

            foreach (var item in garage) {
                if (float.Parse(item["x"].ToString()) != float.Parse(data[1]["x"].ToString()))
                    continue;
                if (float.Parse(item["y"].ToString()) != float.Parse(data[1]["y"].ToString()))
                    continue;

                var carHash = uint.Parse(data[0]["hash"].ToString());

                vehicle = await AltAsync.CreateVehicle(
                    carHash,
                    new Position(float.Parse(item["parkslot"]["pos"]["x"].ToString()), float.Parse(item["parkslot"]["pos"]["y"].ToString()),
                        float.Parse(item["parkslot"]["pos"]["z"].ToString())),
                    new Rotation(float.Parse(item["parkslot"]["rot"]["x"].ToString()), float.Parse(item["parkslot"]["rot"]["y"].ToString()),
                        float.Parse(item["parkslot"]["rot"]["z"].ToString())));
                await vehicle.SetDimensionAsync(1);
            }
            
            await player.SetStreamSyncedMetaDataAsync("lastVehicle", vehicle);

            await vehicle.SetStreamSyncedMetaDataAsync("tank", int.Parse(data[0]["tank"].ToString()));
            await vehicle.SetSyncedMetaDataAsync("engine", false);
            await vehicle.SetLockStateAsync(VehicleLockState.Unlocked);
            await vehicle.SetNumberplateTextAsync(data[0]["numberplate"].ToString());
            await vehicle.SetDirtLevelAsync(byte.Parse(data[0]["dirtLevel"].ToString()));
            
            ApplyDamage(vehicle, data[0]);
            ApplyOpticTunings(vehicle, data[0]);
            ApplyPeformanceTunings(vehicle, data[0]);

            await player.EmitAsync("setPedIntoVehicle", vehicle);
            Database.GetGarage(player);
        }

        private static async void ApplyDamage(IVehicle vehicle, JToken data) {
            await vehicle.SetBodyAdditionalHealthAsync(uint.Parse(data["damage"]["bodyAdditionalHealth"].ToString()));
            await vehicle.SetBodyHealthAsync(uint.Parse(data["damage"]["bodyHealth"].ToString()));
            await vehicle.SetEngineHealthAsync(int.Parse(data["damage"]["engineHealth"].ToString()));
            await vehicle.SetPetrolTankHealthAsync(int.Parse(data["damage"]["petrolTankHealth"].ToString()));
            await vehicle.SetHealthDataAsync(data["damage"]["healthDataBase64"].ToString());
        }

        private static async void ApplyOpticTunings(IVehicle vehicle, JToken data) {
            await vehicle.SetCustomTiresAsync(bool.Parse(data["tuning"]["optic"]["customTires"].ToString()));
            await vehicle.SetDashboardColorAsync(byte.Parse(data["tuning"]["optic"]["dashboardColor"].ToString()));
            await vehicle.SetHeadlightColorAsync(byte.Parse(data["tuning"]["optic"]["headlightColor"].ToString()));
            await vehicle.SetInteriorColorAsync(byte.Parse(data["tuning"]["optic"]["interiorColor"].ToString()));
            await vehicle.SetPearlColorAsync(byte.Parse(data["tuning"]["optic"]["pearlColor"].ToString()));
            await vehicle.SetWheelColorAsync(byte.Parse(data["tuning"]["optic"]["wheelColor"].ToString()));
        }

        private static async void ApplyPeformanceTunings(IVehicle vehicle, JToken data) {
            if (byte.Parse(data["tuning"]["modkit"].ToString()) == 0)
                return;

            await vehicle.SetModKitAsync(byte.Parse(data["tuning"]["modkit"].ToString()));
            await vehicle.SetModAsync(12, byte.Parse(data["tuning"]["peformance"]["brakes"].ToString()));
            await vehicle.SetModAsync(11, byte.Parse(data["tuning"]["peformance"]["engine"].ToString()));
            await vehicle.SetModAsync(0, byte.Parse(data["tuning"]["peformance"]["spoiler"].ToString()));
            await vehicle.SetModAsync(15, byte.Parse(data["tuning"]["peformance"]["suspension"].ToString()));
            await vehicle.SetModAsync(13, byte.Parse(data["tuning"]["peformance"]["transmission"].ToString()));
            await vehicle.SetModAsync(18, byte.Parse(data["tuning"]["peformance"]["turbo"].ToString()));
        }
        
        
    }
}