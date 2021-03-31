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

            Database.SetVehicleStatus(player, uint.Parse(data[0]["hash"].ToString()), false);
            vehicle.Visible = true;
            
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
            await vehicle.SetNeonActiveAsync(bool.Parse(data["tuning"]["optic"]["neon"]["left"].ToString()), bool.Parse(data["tuning"]["optic"]["neon"]["right"].ToString()), bool.Parse(data["tuning"]["optic"]["neon"]["top"].ToString()), bool.Parse(data["tuning"]["optic"]["neon"]["back"].ToString()));
            var neonColor = data["tuning"]["optic"]["neonColor"];
            await vehicle.SetNeonColorAsync(new Rgba(byte.Parse(neonColor["R"].ToString()), byte.Parse(neonColor["G"].ToString()), byte.Parse(neonColor["B"].ToString()), byte.Parse(neonColor["A"].ToString())));
            var primaryColor = data["tuning"]["optic"]["primaryColor"];
            await vehicle.SetPrimaryColorRgbAsync(new Rgba(byte.Parse(primaryColor["R"].ToString()), byte.Parse(primaryColor["G"].ToString()), byte.Parse(primaryColor["B"].ToString()), byte.Parse(primaryColor["A"].ToString())));
            var secondaryColor = data["tuning"]["optic"]["secondaryColor"];
            await vehicle.SetSecondaryColorRgbAsync(new Rgba(byte.Parse(secondaryColor["R"].ToString()), byte.Parse(secondaryColor["G"].ToString()), byte.Parse(secondaryColor["B"].ToString()), byte.Parse(secondaryColor["A"].ToString())));
            await vehicle.SetPearlColorAsync(byte.Parse(data["tuning"]["optic"]["pearlColor"].ToString()));
            var tireSmokeColor = data["tuning"]["optic"]["tireSmokeColor"];
            await vehicle.SetTireSmokeColorAsync(new Rgba(byte.Parse(tireSmokeColor["R"].ToString()), byte.Parse(tireSmokeColor["G"].ToString()), byte.Parse(tireSmokeColor["B"].ToString()), byte.Parse(tireSmokeColor["A"].ToString())));
            await vehicle.SetWheelColorAsync(byte.Parse(data["tuning"]["optic"]["wheelColor"].ToString()));
            await vehicle.SetModAsync(4, byte.Parse(data["tuning"]["optic"]["exhaust"].ToString()));
            await vehicle.SetModAsync(5, byte.Parse(data["tuning"]["optic"]["frame"].ToString()));
            await vehicle.SetModAsync(6, byte.Parse(data["tuning"]["optic"]["grille"].ToString()));
            await vehicle.SetModAsync(10, byte.Parse(data["tuning"]["optic"]["roof"].ToString()));
            await vehicle.SetModAsync(14, byte.Parse(data["tuning"]["optic"]["horns"].ToString()));
            await vehicle.SetModAsync(20, byte.Parse(data["tuning"]["optic"]["customTireSmoke"].ToString()));
            await vehicle.SetModAsync(22, byte.Parse(data["tuning"]["optic"]["xenon"].ToString()));
            await vehicle.SetModAsync(31, byte.Parse(data["tuning"]["optic"]["door_interior"].ToString()));
            await vehicle.SetModAsync(32, byte.Parse(data["tuning"]["optic"]["seats"].ToString()));
            await vehicle.SetModAsync(39, byte.Parse(data["tuning"]["optic"]["engine_block"].ToString()));
            await vehicle.SetModAsync(40, byte.Parse(data["tuning"]["optic"]["air_filter"].ToString()));
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

        public static async void RemoveGarageVehicle(IPlayer player) {
            var vehicle = await player.GetStreamSyncedMetaDataAsync<IVehicle>("lastVehicle");

            if (vehicle != null && vehicle.Exists && vehicle.NumberplateText != "ADMIN") {
                Database.UpdateVehicleData(player, vehicle);
                await vehicle.RemoveAsync();
                player.DeleteStreamSyncedMetaData("lastVehicle");
                Database.GetGarage(player);
            } else if (vehicle != null && vehicle.Exists && vehicle.NumberplateText == "ADMIN") {
                await vehicle.RemoveAsync();
                player.DeleteStreamSyncedMetaData("lastVehicle");
            }
        }
    }
}