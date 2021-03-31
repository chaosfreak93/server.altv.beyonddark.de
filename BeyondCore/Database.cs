using System.Numerics;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace BeyondCore
{
    public class Database
    {
        public Database() {
            Db = new MongoClient("mongodb://keiner:Gommekiller93@127.0.0.1:27017/");
        }

        private static MongoClient Db { get; set; }

        public static async void Login(IPlayer player, string discordInfoString) {
            await player.EmitAsync("loginFinished");
            var discordInfo = JObject.Parse(discordInfoString);

            var database = Db.GetDatabase("altv");
            var accounts = database.GetCollection<BsonDocument>("accounts");
            var socialIdFilter = Builders<BsonDocument>.Filter.Eq("socialclub", player.SocialClubId.ToString());
            var discordIdFilter = Builders<BsonDocument>.Filter.Eq("discord", discordInfo["id"]?.ToString());

            var socialFound = accounts.Find(socialIdFilter);
            var discordFound = accounts.Find(discordIdFilter);

            if (socialFound.CountDocumentsAsync().Result <= 0 && discordFound.CountDocumentsAsync().Result <= 0) {
                var document = new BsonDocument {
                    {"username", player.Name},
                    {"socialclub", player.SocialClubId.ToString()},
                    {"discord", discordInfo["id"]?.ToString()},
                    {"pos", new BsonDocument {{"x", -1044.6988525390625}, {"y", -2749.6220703125}, {"z", 22.3604736328125}}},
                    {"job", 1},
                    {"money", new BsonDocument {{"hand", 5000}, {"bank", 0}, {"black", 0}}},
                    {"garage", new BsonArray()}
                };

                await accounts.InsertOneAsync(document);

                await player.SetDimensionAsync(1);
                await player.SetModelAsync(Alt.Hash("u_m_m_jesus_01"));
                await player.EmitAsync("chat:Init");
                await player.EmitAsync("teleportToLastPosition",
                    new Vector3(-1044.6988525390625f, -2749.6220703125f, 22.3604736328125f));
                player.Visible = true;
                await player.SetMetaDataAsync("loggedIn", true);
                GetGarage(player);
            } else if (discordFound.CountDocumentsAsync().Result > 0 && socialFound.CountDocumentsAsync().Result <= 0) {
                await player.KickAsync(
                    "Fehler 001: Discord und Socialclub Konto passen nicht zusammen. Socialclub Konto falsch angegeben. Versuche es bitte erneut.");
            } else if (discordFound.CountDocumentsAsync().Result <= 0 && socialFound.CountDocumentsAsync().Result > 0) {
                await player.KickAsync(
                    "Fehler 002: Discord und Socialclub Konto passen nicht zusammen. Discord Konto falsch angegeben. Versuche es bitte erneut.");
            } else if (discordFound.CountDocumentsAsync().Result > 0 && socialFound.CountDocumentsAsync().Result > 0) {
                var account = await socialFound.FirstAsync();
                await player.SetDimensionAsync(1);
                await player.SetModelAsync(Alt.Hash("u_m_m_jesus_01"));
                await player.EmitAsync("chat:Init");
                var pos = account["pos"];
                await player.EmitAsync("teleportToLastPosition",
                    new Vector3(float.Parse(pos["x"]?.ToString()), float.Parse(pos["y"]?.ToString()),
                        float.Parse(pos["z"]?.ToString())));
                player.Visible = true;
                await player.SetMetaDataAsync("loggedIn", true);
                GetGarage(player);
            }

            //await AltAsync.EmitAsync(player, "joinJob");
        }

        public static async void UpdatePosition(IPlayer player) {
            var filter = Builders<BsonDocument>.Filter.Eq("socialclub", player.SocialClubId.ToString());
            var update = Builders<BsonDocument>.Update.Set("pos",
                new BsonDocument {{"x", player.Position.X}, {"y", player.Position.Y}, {"z", player.Position.Z + 0.5f}});

            var database = Db.GetDatabase("altv");
            var accounts = database.GetCollection<BsonDocument>("accounts");

            await accounts.UpdateOneAsync(filter, update);
        }

        public static async void AddMoneyToBank(IPlayer player, int amount) {
            var database = Db.GetDatabase("altv");
            var accounts = database.GetCollection<BsonDocument>("accounts");
            var socialIdFilter = Builders<BsonDocument>.Filter.Eq("socialclub", player.SocialClubId.ToString());

            var account = await accounts.Find(socialIdFilter).FirstAsync();
            var moneyHand = int.Parse(account["money"]["hand"].ToString());
            var moneyBlack = int.Parse(account["money"]["black"].ToString());
            var oldMoneyBank = int.Parse(account["money"]["bank"].ToString());
            var newMoneyBank = oldMoneyBank + amount;

            var update = Builders<BsonDocument>.Update.Set("money",
                new BsonDocument {{"hand", moneyHand}, {"bank", newMoneyBank}, {"black", moneyBlack}});

            await accounts.UpdateOneAsync(socialIdFilter, update);
        }

        public static async void AddMoneyToHand(IPlayer player, int amount) {
            var database = Db.GetDatabase("altv");
            var accounts = database.GetCollection<BsonDocument>("accounts");
            var socialIdFilter = Builders<BsonDocument>.Filter.Eq("socialclub", player.SocialClubId.ToString());

            var account = await accounts.Find(socialIdFilter).FirstAsync();
            var moneyBank = int.Parse(account["money"]["bank"].ToString());
            var moneyBlack = int.Parse(account["money"]["black"].ToString());
            var oldMoneyHand = int.Parse(account["money"]["hand"].ToString());
            var newMoneyHand = oldMoneyHand + amount;

            var update = Builders<BsonDocument>.Update.Set("money",
                new BsonDocument {{"hand", newMoneyHand}, {"bank", moneyBank}, {"black", moneyBlack}});

            await accounts.UpdateOneAsync(socialIdFilter, update);
        }

        public static async void AddBlackMoney(IPlayer player, int amount) {
            var database = Db.GetDatabase("altv");
            var accounts = database.GetCollection<BsonDocument>("accounts");
            var socialIdFilter = Builders<BsonDocument>.Filter.Eq("socialclub", player.SocialClubId.ToString());

            var account = await accounts.Find(socialIdFilter).FirstAsync();
            var moneyHand = int.Parse(account["money"]["hand"].ToString());
            var moneyBank = int.Parse(account["money"]["bank"].ToString());
            var oldBlackMoney = int.Parse(account["money"]["black"].ToString());
            var newBlackMoney = oldBlackMoney + amount;

            var update = Builders<BsonDocument>.Update.Set("money",
                new BsonDocument {{"hand", moneyHand}, {"bank", moneyBank}, {"black", newBlackMoney}});

            await accounts.UpdateOneAsync(socialIdFilter, update);
        }

        public static async Task<int> GetMoneyOnBank(IPlayer player) {
            var database = Db.GetDatabase("altv");
            var accounts = database.GetCollection<BsonDocument>("accounts");
            var socialIdFilter = Builders<BsonDocument>.Filter.Eq("socialclub", player.SocialClubId.ToString());

            var account = await accounts.Find(socialIdFilter).FirstAsync();
            return int.Parse(account["money"]["bank"].ToString());
        }

        public static async Task<int> GetMoneyOnHand(IPlayer player) {
            var database = Db.GetDatabase("altv");
            var accounts = database.GetCollection<BsonDocument>("accounts");
            var socialIdFilter = Builders<BsonDocument>.Filter.Eq("socialclub", player.SocialClubId.ToString());

            var account = await accounts.Find(socialIdFilter).FirstAsync();
            return int.Parse(account["money"]["hand"].ToString());
        }

        public static async Task<int> GetBlackMoney(IPlayer player) {
            var database = Db.GetDatabase("altv");
            var accounts = database.GetCollection<BsonDocument>("accounts");
            var socialIdFilter = Builders<BsonDocument>.Filter.Eq("socialclub", player.SocialClubId.ToString());

            var account = await accounts.Find(socialIdFilter).FirstAsync();
            return int.Parse(account["money"]["black"].ToString());
        }

        public static async void RemoveMoneyFromBank(IPlayer player, int amount) {
            var database = Db.GetDatabase("altv");
            var accounts = database.GetCollection<BsonDocument>("accounts");
            var socialIdFilter = Builders<BsonDocument>.Filter.Eq("socialclub", player.SocialClubId.ToString());

            var account = await accounts.Find(socialIdFilter).FirstAsync();
            var moneyHand = int.Parse(account["money"]["hand"].ToString());
            var moneyBlack = int.Parse(account["money"]["black"].ToString());
            var oldMoneyBank = int.Parse(account["money"]["bank"].ToString());
            var newMoneyBank = oldMoneyBank - amount;

            var update = Builders<BsonDocument>.Update.Set("money",
                new BsonDocument {{"hand", moneyHand}, {"bank", newMoneyBank}, {"black", moneyBlack}});

            await accounts.UpdateOneAsync(socialIdFilter, update);
        }

        public static async void RemoveMoneyFromHand(IPlayer player, int amount) {
            var database = Db.GetDatabase("altv");
            var accounts = database.GetCollection<BsonDocument>("accounts");
            var socialIdFilter = Builders<BsonDocument>.Filter.Eq("socialclub", player.SocialClubId.ToString());

            var account = await accounts.Find(socialIdFilter).FirstAsync();
            var moneyBank = int.Parse(account["money"]["bank"].ToString());
            var moneyBlack = int.Parse(account["money"]["black"].ToString());
            var oldMoneyHand = int.Parse(account["money"]["hand"].ToString());
            var newMoneyHand = oldMoneyHand - amount;

            var update = Builders<BsonDocument>.Update.Set("money",
                new BsonDocument {{"hand", newMoneyHand}, {"bank", moneyBank}, {"black", moneyBlack}});

            await accounts.UpdateOneAsync(socialIdFilter, update);
        }

        public static async void RemoveBlackMoney(IPlayer player, int amount) {
            var database = Db.GetDatabase("altv");
            var accounts = database.GetCollection<BsonDocument>("accounts");
            var socialIdFilter = Builders<BsonDocument>.Filter.Eq("socialclub", player.SocialClubId.ToString());

            var account = await accounts.Find(socialIdFilter).FirstAsync();
            var moneyHand = int.Parse(account["money"]["hand"].ToString());
            var moneyBank = int.Parse(account["money"]["bank"].ToString());
            var oldBlackMoney = int.Parse(account["money"]["black"].ToString());
            var newBlackMoney = oldBlackMoney - amount;

            var update = Builders<BsonDocument>.Update.Set("money",
                new BsonDocument {{"hand", moneyHand}, {"bank", moneyBank}, {"black", newBlackMoney}});

            await accounts.UpdateOneAsync(socialIdFilter, update);
        }

        public static async Task<int> GetJobIdOfPlayer(IPlayer player) {
            var database = Db.GetDatabase("altv");
            var accounts = database.GetCollection<BsonDocument>("accounts");
            var socialIdFilter = Builders<BsonDocument>.Filter.Eq("socialclub", player.SocialClubId.ToString());

            var account = await accounts.Find(socialIdFilter).FirstAsync();

            return int.Parse(account["job"].ToString());
        }

        public static async Task<int> GetJobSalary(int jobId) {
            var database = Db.GetDatabase("altv");
            var jobs = database.GetCollection<BsonDocument>("jobs");
            var jobIdFilter = Builders<BsonDocument>.Filter.Eq("id", jobId);

            var job = await jobs.Find(jobIdFilter).FirstAsync();

            return int.Parse(job["salary"].ToString());
        }

        public static string GetJobName(int jobId) {
            var database = Db.GetDatabase("altv");
            var jobs = database.GetCollection<BsonDocument>("jobs");
            var jobIdFilter = Builders<BsonDocument>.Filter.Eq("id", jobId);

            var job = jobs.Find(jobIdFilter).FirstAsync().Result;

            return job["jobName"].ToString();
        }

        // TODO: Set Job
        public async void SetJobIdOfPlayer(IPlayer player, int jobId) {

        }

        public static async void GetGarage(IPlayer player) {
            player.GetMetaData("loggedIn", out bool loggedIn);

            if (!loggedIn)
                return;

            var database = Db.GetDatabase("altv");
            var accounts = database.GetCollection<BsonDocument>("accounts");
            var socialIdFilter = Builders<BsonDocument>.Filter.Eq("socialclub", player.SocialClubId.ToString());

            var account = await accounts.Find(socialIdFilter).FirstAsync();

            await player.EmitAsync("getGarage", account["garage"].ToString());
        }
        
        public static async void AddVehicleToGarage(IPlayer player, string carName, string numberPlate, IVehicle vehicle) {
            var filter = Builders<BsonDocument>.Filter.Eq("socialclub", player.SocialClubId.ToString());

            var database = Db.GetDatabase("altv");
            var accounts = database.GetCollection<BsonDocument>("accounts");

            var account = await accounts.Find(filter).FirstAsync();
            var garage = account["garage"].AsBsonArray;

            bool left = false;
            bool right = false;
            bool top = false;
            bool back = false;
            vehicle.GetNeonActive(ref left, ref right, ref top, ref back);
            
            garage.Add(new BsonDocument {
                {"name", carName},
                {"hash", vehicle.Model.ToString()},
                {"tank", 100},
                {"numberplate", numberPlate},
                {"parking", false},
                {"dirtLevel", vehicle.DirtLevel}, {
                    "damage", new BsonDocument {
                        {"bodyAdditionalHealth", int.Parse(vehicle.BodyAdditionalHealth.ToString())},
                        {"bodyHealth", int.Parse(vehicle.BodyHealth.ToString())},
                        {"engineHealth", int.Parse(vehicle.EngineHealth.ToString())},
                        {"petrolTankHealth", int.Parse(vehicle.PetrolTankHealth.ToString())},
                        {"healthDataBase64", vehicle.HealthData}
                    }
                }, {
                    "tuning", new BsonDocument {
                        {"modkit", vehicle.ModKit},
                        {"modkitCount", vehicle.ModKitsCount}, {
                            "optic", new BsonDocument {
                                {"customTires", vehicle.CustomTires},
                                {"dashboardColor", vehicle.DashboardColor},
                                {"headlightColor", vehicle.HeadlightColor},
                                {"interiorColor", vehicle.InteriorColor},
                                {"neon", new BsonDocument { {"left", left}, {"right", right}, {"top", top}, {"back", back} } },
                                {"neonColor", vehicle.NeonColor.ToBsonDocument()},
                                {"primaryColor", vehicle.PrimaryColorRgb.ToBsonDocument()},
                                {"secondaryColor", vehicle.SecondaryColorRgb.ToBsonDocument()},
                                {"pearlColor", vehicle.PearlColor},
                                {"tireSmokeColor", vehicle.TireSmokeColor.ToBsonDocument()},
                                {"wheelColor", vehicle.WheelColor},
                                {"exhaust", vehicle.GetMod(4) },
                                {"frame", vehicle.GetMod(5) },
                                {"grille", vehicle.GetMod(6) },
                                {"roof", vehicle.GetMod(10) },
                                {"horns", vehicle.GetMod(14) },
                                {"customTireSmoke", vehicle.GetMod(20) },
                                {"xenon", vehicle.GetMod(22) },
                                {"door_interior", vehicle.GetMod(31) },
                                {"seats", vehicle.GetMod(32) },
                                {"engine_block", vehicle.GetMod(39) },
                                {"air_filter", vehicle.GetMod(40) },
                            }
                        }, {
                            "peformance", new BsonDocument {
                                {"brakes", vehicle.GetMod(12)},
                                {"engine", vehicle.GetMod(11)},
                                {"spoiler", vehicle.GetMod(0)},
                                {"suspension", vehicle.GetMod(15)},
                                {"transmission", vehicle.GetMod(13)},
                                {"turbo", vehicle.GetMod(18)}
                            }
                        }
                    }
                }
            });

            var update = Builders<BsonDocument>.Update.Set("garage", garage.AsBsonValue);

            await accounts.UpdateOneAsync(filter, update);
        }

        public static async void SetVehicleStatus(IPlayer player, uint hash, bool status) {
            var database = Db.GetDatabase("altv");
            var accounts = database.GetCollection<BsonDocument>("accounts");
            var socialIdFilter = Builders<BsonDocument>.Filter.Eq("socialclub", player.SocialClubId.ToString());

            var account = await accounts.Find(socialIdFilter).FirstAsync();
            var garage = account["garage"].AsBsonArray;

            foreach (var vehicle in garage) {
                if (vehicle["hash"].ToString() == hash.ToString()) {
                    vehicle["parking"] = status;
                }
            }
            
            var update = Builders<BsonDocument>.Update.Set("garage", garage.AsBsonValue);

            await accounts.UpdateOneAsync(socialIdFilter, update);
        }
        
        public static async void UpdateVehicleData(IPlayer player, IVehicle vehicle) {
            var database = Db.GetDatabase("altv");
            var accounts = database.GetCollection<BsonDocument>("accounts");
            var socialIdFilter = Builders<BsonDocument>.Filter.Eq("socialclub", player.SocialClubId.ToString());

            var account = await accounts.Find(socialIdFilter).FirstAsync();
            var garage = account["garage"].AsBsonArray;
            foreach (var garageVehicle in garage) {
                if (uint.Parse(garageVehicle["hash"].ToString()) == vehicle.Model) {
                    vehicle.GetStreamSyncedMetaData("tank", out int tank);
                    garageVehicle["tank"] = tank;
                    garageVehicle["parking"] = true;
                    garageVehicle["dirtLevel"] = vehicle.DirtLevel;
                    garageVehicle["damage"]["bodyAdditionalHealth"] = int.Parse(vehicle.BodyAdditionalHealth.ToString());
                    garageVehicle["damage"]["bodyHealth"] = int.Parse(vehicle.BodyHealth.ToString());
                    garageVehicle["damage"]["engineHealth"] = int.Parse(vehicle.EngineHealth.ToString());
                    garageVehicle["damage"]["petrolTankHealth"] = int.Parse(vehicle.PetrolTankHealth.ToString());
                    garageVehicle["damage"]["healthDataBase64"] = vehicle.HealthData;
                    garageVehicle["tuning"]["modkit"] = vehicle.ModKit;
                    garageVehicle["tuning"]["modkitCount"] = vehicle.ModKitsCount;
                    garageVehicle["tuning"]["optic"]["customTires"] = vehicle.CustomTires;
                    garageVehicle["tuning"]["optic"]["dashboardColor"] = vehicle.DashboardColor;
                    garageVehicle["tuning"]["optic"]["headlightColor"] = vehicle.HeadlightColor;
                    garageVehicle["tuning"]["optic"]["interiorColor"] = vehicle.InteriorColor;
                    bool left = false;
                    bool right = false;
                    bool top = false;
                    bool back = false;
                    vehicle.GetNeonActive(ref left, ref right, ref top, ref back);
                    garageVehicle["tuning"]["optic"]["neon"] = new BsonDocument { {"left", left}, {"right", right}, {"top", top}, {"back", back} };
                    garageVehicle["tuning"]["optic"]["neonColor"] = vehicle.NeonColor.ToBsonDocument();
                    garageVehicle["tuning"]["optic"]["primaryColor"] = vehicle.PrimaryColorRgb.ToBsonDocument();
                    garageVehicle["tuning"]["optic"]["secondaryColor"] = vehicle.SecondaryColorRgb.ToBsonDocument();
                    garageVehicle["tuning"]["optic"]["pearlColor"] = vehicle.PearlColor;
                    garageVehicle["tuning"]["optic"]["tireSmokeColor"] = vehicle.TireSmokeColor.ToBsonDocument();
                    garageVehicle["tuning"]["optic"]["wheelColor"] = vehicle.WheelColor;
                    garageVehicle["tuning"]["optic"]["exhaust"] = vehicle.GetMod(4);
                    garageVehicle["tuning"]["optic"]["frame"] = vehicle.GetMod(5);
                    garageVehicle["tuning"]["optic"]["grille"] = vehicle.GetMod(6);
                    garageVehicle["tuning"]["optic"]["roof"] = vehicle.GetMod(10);
                    garageVehicle["tuning"]["optic"]["horns"] = vehicle.GetMod(14);
                    garageVehicle["tuning"]["optic"]["customTireSmoke"] = vehicle.GetMod(20);
                    garageVehicle["tuning"]["optic"]["xenon"] = vehicle.GetMod(22);
                    garageVehicle["tuning"]["optic"]["door_interior"] = vehicle.GetMod(31);
                    garageVehicle["tuning"]["optic"]["seats"] = vehicle.GetMod(32);
                    garageVehicle["tuning"]["optic"]["engine_block"] = vehicle.GetMod(39);
                    garageVehicle["tuning"]["optic"]["air_filter"] = vehicle.GetMod(40);
                    garageVehicle["tuning"]["peformance"]["brakes"] = vehicle.GetMod(12);
                    garageVehicle["tuning"]["peformance"]["engine"] = vehicle.GetMod(11);
                    garageVehicle["tuning"]["peformance"]["spoiler"] = vehicle.GetMod(0);
                    garageVehicle["tuning"]["peformance"]["suspension"] = vehicle.GetMod(15);
                    garageVehicle["tuning"]["peformance"]["transmission"] = vehicle.GetMod(13);
                    garageVehicle["tuning"]["peformance"]["turbo"] = vehicle.GetMod(18);
                }
            }

            var update = Builders<BsonDocument>.Update.Set("garage", garage.AsBsonValue);

            await accounts.UpdateOneAsync(socialIdFilter, update);
        }
        
        public static async void SetAllVehicleParking() {
            var database = Db.GetDatabase("altv");
            var accounts = database.GetCollection<BsonDocument>("accounts");

            var account = accounts.Find(new BsonDocument()).ToList();

            foreach (BsonDocument doc in account) {
                var socialIdFilter = Builders<BsonDocument>.Filter.Eq("socialclub", doc["socialclub"]);
                var garage = doc["garage"].AsBsonArray;
                foreach (var vehicle in garage) {
                    if (vehicle["parking"] == false) {
                        vehicle["parking"] = true;
                    }
                }
                var update = Builders<BsonDocument>.Update.Set("garage", garage.AsBsonValue);

                await accounts.UpdateOneAsync(socialIdFilter, update);
            }
        }
    }
}