using System;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;

namespace BeyondCore.VehicleStuff
{
    public static class CarDealer
    {
        public static async void BuyCar(IPlayer player, string carName) {
            player.GetStreamSyncedMetaData("lastVehicle", out IVehicle lastVehicle);

            if (lastVehicle != null && lastVehicle.Exists) {
                lastVehicle.Remove();
                player.DeleteStreamSyncedMetaData("lastVehicle");
            }

            var carHash = Alt.Hash(carName);

            var vehicle = Alt.CreateVehicle(carHash, new Position(-31.094505310058594f, -1089.20654296875f, 25.859375f),
                new Rotation(0f, 0f, -0.346317321062088f));
            await vehicle.SetDimensionAsync(1);

            await player.SetStreamSyncedMetaDataAsync("lastVehicle", vehicle);

            await vehicle.SetStreamSyncedMetaDataAsync("tank", 100);
            await vehicle.SetSyncedMetaDataAsync("engine", false);
            await vehicle.SetLockStateAsync(VehicleLockState.Unlocked);
            var numberPlate = MakeNumberPlate(8);
            await vehicle.SetNumberplateTextAsync(numberPlate);
            Database.AddVehicleToGarage(player, carName, numberPlate, vehicle);
            await player.EmitAsync("setPedIntoVehicle", vehicle);
            Database.GetGarage(player);
        }

        private static string MakeNumberPlate(int length) {
            var result = "";
            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var charactersLength = characters.Length;

            for (var i = 0; i < length; i++)
                result += characters[int.Parse(Math.Floor(new Random().NextDouble() * charactersLength).ToString())];

            return result;
        }
    }
}