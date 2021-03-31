using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using BeyondCore.VehicleStuff;

namespace BeyondCore
{
    public class Core
    {
        private static Timer _syncDateAndTimer = new(SyncDateAndTimer, null, 0, 300000);

        public Core() {
            var unused = new Database();
            var unused1 = new ColShapes();
            AltAsync.OnPlayerConnect += OnServerPlayerConnect;
            AltAsync.OnPlayerDead += OnServerPlayerDeath;
            AltAsync.OnPlayerDisconnect += OnServerPlayerDisconnect;
            AltAsync.OnPlayerEnterVehicle += OnServerPlayerEnterVehicle;
            AltAsync.OnPlayerLeaveVehicle += OnServerPlayerLeaveVehicle;
            AltAsync.OnPlayerChangeVehicleSeat += OnServerPlayerChangeVehicleSeat;
            AltAsync.OnColShape += ColShapes.OnServerColShape;
            AltAsync.OnServer<IPlayer, string>("discord:AuthDone", Database.Login);
            AltAsync.OnClient<IPlayer, IVehicle, int>("setTank", VehicleSystem.SetTank);
            AltAsync.OnClient<IPlayer, IVehicle>("getTank", VehicleSystem.GetTank);
            AltAsync.OnClient<IPlayer, IVehicle>("isEngineRunning", VehicleSystem.IsEngineRunning);
            AltAsync.OnClient<IPlayer, IVehicle>("toggleEngine", VehicleSystem.ToggleEngine);
            AltAsync.OnClient<IPlayer, IVehicle>("toggleVehicleLock", VehicleSystem.ToggleVehicleLock);
            AltAsync.OnClient<IPlayer, IVehicle>("toggleSirenAudio", VehicleSystem.ToggleSirenAudio);
            AltAsync.OnClient<IPlayer, IVehicle, string>("vehicle:RadioChanged", VehicleSystem.RadioChanged);
            AltAsync.OnClient<IPlayer>("radio:GetRadioStations", VehicleSystem.GetRadioStations);
            AltAsync.OnClient<IPlayer, string>("CarDealer:buyCar", CarDealer.BuyCar);
            AltAsync.OnClient<IPlayer, string>("garage:SpawnVehicle", Garage.SpawnGarageVehicle);
            AltAsync.OnClient<IPlayer>("garage:RemoveVehicle", Garage.RemoveGarageVehicle);
            AltAsync.OnClient<IPlayer>("getGarage", Database.GetGarage);
            AltAsync.Do(Database.SetAllVehicleParking);
        }

        private static async Task OnServerPlayerConnect(IPlayer player, string reason) {
            if (player.Name == "Player")
                await player.KickAsync("Bitte ändere deinen Nutzernamen unter Einstellungen->Nutzername");

            await player.SetMetaDataAsync("loggedIn", false);

            AltAsync.Log(player.Name + " hat den Staat beitreten!");

            await player.SpawnAsync(new Position(4890.94921875f, -4924.7998046875f, 10.3070068359375f));
            await player.SetRotationAsync(new Rotation(0f, 0f, 3.1168558597564697f));
            await player.SetDimensionAsync(player.Id);
            await player.SetDateTimeAsync(DateTime.Now);
            AltAsync.Emit("SaltyChat:EnablePlayer", player);
            player.Visible = false;
        }

        private static async Task OnServerPlayerDeath(IPlayer player, IEntity killer, uint weapon) {
            lock (player) {
                player.Spawn(new Position(-436.1406555175781f, -326.5714416503906f, 35f));
            }

            await Task.CompletedTask;
        }

        private static async Task OnServerPlayerDisconnect(IPlayer player, string reason) {
            lock (player) {
                if (player.Name == "Player") return;

                player.GetMetaData("loggedIn", out bool loggedIn);

                if (!loggedIn) {
                    AltAsync.Log(player.Name + " hat den Staat verlassen!");
                    return;
                }

                Database.UpdatePosition(player);

                player.GetStreamSyncedMetaData("lastVehicle", out IVehicle lastVehicle);

                if (lastVehicle != null && lastVehicle.Exists) {
                    if (lastVehicle.NumberplateText != "ADMIN")
                        Database.UpdateVehicleData(player, lastVehicle);
                    lastVehicle.Remove();
                }

                AltAsync.Log(player.Name + " hat den Staat verlassen!");
            }

            await Task.CompletedTask;
        }

        private static async Task OnServerPlayerEnterVehicle(IVehicle vehicle, IPlayer player, byte seat) {
            if (vehicle == null || !vehicle.Exists) return;

            player.EmitLocked("playerEnteredVehicle", vehicle, int.Parse(seat.ToString()));

            await Task.CompletedTask;
        }

        private static async Task OnServerPlayerLeaveVehicle(IVehicle vehicle, IPlayer player, byte seat) {
            if (vehicle == null || !vehicle.Exists) return;

            if (!vehicle.IsDestroyed && seat == 1)
                vehicle.SetSyncedMetaData("engine", false);

            player.EmitLocked("playerLeftVehicle", vehicle, int.Parse(seat.ToString()));

            await Task.CompletedTask;
        }

        private static async Task OnServerPlayerChangeVehicleSeat(IVehicle vehicle, IPlayer player, byte oldSeat, byte newSeat) {
            if (vehicle == null || !vehicle.Exists) return;

            player.EmitLocked("playerChangedVehicleSeat", vehicle, int.Parse(oldSeat.ToString()), int.Parse(newSeat.ToString()));

            await Task.CompletedTask;
        }

        private static async void SyncDateAndTimer(object unused) {
            foreach (var player in Alt.GetAllPlayers().ToList()) await player.SetDateTimeAsync(DateTime.Now);
        }
    }
}