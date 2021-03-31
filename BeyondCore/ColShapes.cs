using System.IO;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json.Linq;

namespace BeyondCore
{
    public class ColShapes
    {
        public ColShapes() {
            var garage = JArray.Parse(File.ReadAllText("resources/LosAssets/content/data/position/garage.json"));

            foreach (var t in garage) {
                var rob = Alt.CreateColShapeCylinder(new Position(float.Parse(t["x"].ToString()),
                    float.Parse(t["y"].ToString()), float.Parse(t["z"].ToString())), 1.5f, 3f);

                rob.Dimension = 1;
                rob.IsPlayersOnly = true;
                rob.SetData("name", "Garage");
            }

            var robList = JArray.Parse(File.ReadAllText("resources/LosAssets/content/data/position/rob_list.json"));

            foreach (var t in robList) {
                var rob = Alt.CreateColShapeCylinder(new Position(float.Parse(t["x"].ToString()),
                    float.Parse(t["y"].ToString()), float.Parse(t["z"].ToString())), 1.5f, 3f);

                rob.Dimension = 1;
                rob.IsPlayersOnly = true;
                rob.SetData("name", "Bank");
            }

            var carDealer = Alt.CreateColShapeCylinder(new Position(-35.235164642333984f, -1102.6812744140625f, 26.4154052734375f), 1f, 3f);
            carDealer.Dimension = 1;
            carDealer.IsPlayersOnly = true;
            carDealer.SetData("name", "CarDealer");
        }

        public static async Task OnServerColShape(IColShape shape, IEntity entity, bool state) {
            shape.GetData("name", out string result);

            switch (result) {
                case "Garage":
                {
                    if (entity is IPlayer player) {
                        if (state) {
                            await player.EmitAsync("Garage:enter", shape.Position);
                            Database.GetGarage(player);
                        } else {
                            await player.EmitAsync("Garage:leave");
                            Database.GetGarage(player);
                        }
                    }

                    break;
                }
                case "Bank":
                {
                    if (entity is IPlayer player) {
                        if (state)
                            await player.EmitAsync("bank:RobEnter");
                        else
                            await player.EmitAsync("bank:RobLeave");
                    }

                    break;
                }
                case "CarDealer":
                {
                    if (entity is IPlayer player) {
                        if (state)
                            await player.EmitAsync("CarDealer:enter");
                        else
                            await player.EmitAsync("CarDealer:leave");
                    }

                    break;
                }
            }
        }
    }
}