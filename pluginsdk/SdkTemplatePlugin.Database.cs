/*
 * SdkTemplatePlugin — Database Operations (Partial Class)
 *
 * Shows two approaches:
 *   1. Raw SQL with MySqlConnector (full control)
 *   2. Dapper micro-ORM (less boilerplate)
 *
 * Both use the same MySqlConnection. Pick whichever fits your needs.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using MySqlConnector;
using Dapper;

namespace PRoConEvents
{
    // Simple class that Dapper maps query results into
    public class PlayerStats
    {
        public string Name { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public double KD => Deaths > 0 ? (double)Kills / Deaths : Kills;
    }

    public partial class SdkTemplatePlugin
    {
        private string _dbConnectionString = "";

        // =================================================================
        // Option 1: Raw SQL — full control, no magic
        // =================================================================

        private string GetPlayerStatsRaw(string soldierName)
        {
            if (string.IsNullOrEmpty(_dbConnectionString)) return null;

            try
            {
                using (var conn = new MySqlConnection(_dbConnectionString))
                {
                    conn.Open();

                    using (var cmd = new MySqlCommand(
                        "SELECT kills, deaths FROM player_stats WHERE name = @name LIMIT 1", conn))
                    {
                        cmd.Parameters.AddWithValue("@name", soldierName);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int kills = reader.GetInt32("kills");
                                int deaths = reader.GetInt32("deaths");
                                double kd = deaths > 0 ? (double)kills / deaths : kills;
                                return string.Format("{0}: {1}K/{2}D ({3:F2} K/D)",
                                    soldierName, kills, deaths, kd);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Info", "DB error: {0}", ex.Message);
            }

            return null;
        }

        // =================================================================
        // Option 2: Dapper — less boilerplate, automatic mapping
        // =================================================================

        private string GetPlayerStats(string soldierName)
        {
            if (string.IsNullOrEmpty(_dbConnectionString)) return null;

            try
            {
                using (var conn = new MySqlConnection(_dbConnectionString))
                {
                    conn.Open();

                    // Dapper maps columns to PlayerStats properties automatically
                    var stats = conn.QueryFirstOrDefault<PlayerStats>(
                        "SELECT name, kills, deaths FROM player_stats WHERE name = @Name",
                        new { Name = soldierName });

                    if (stats != null)
                    {
                        return string.Format("{0}: {1}K/{2}D ({3:F2} K/D)",
                            stats.Name, stats.Kills, stats.Deaths, stats.KD);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Info", "DB error: {0}", ex.Message);
            }

            return null;
        }

        // =================================================================
        // More Dapper examples
        // =================================================================

        private void DapperExamples()
        {
            using (var conn = new MySqlConnection(_dbConnectionString))
            {
                conn.Open();

                // Query multiple rows → List<T>
                var topPlayers = conn.Query<PlayerStats>(
                    "SELECT name, kills, deaths FROM player_stats ORDER BY kills DESC LIMIT 10"
                ).ToList();

                // Insert
                conn.Execute(
                    "INSERT INTO player_stats (name, kills, deaths) VALUES (@Name, @Kills, @Deaths)",
                    new { Name = "PlayerA", Kills = 10, Deaths = 5 });

                // Update
                conn.Execute(
                    "UPDATE player_stats SET kills = kills + 1 WHERE name = @Name",
                    new { Name = "PlayerA" });

                // Delete
                conn.Execute(
                    "DELETE FROM player_stats WHERE name = @Name",
                    new { Name = "PlayerA" });

                // Scalar value
                int count = conn.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM player_stats WHERE kills > @Min",
                    new { Min = 100 });

                // Bulk insert (executes once per item in the list)
                var newPlayers = new List<PlayerStats>
                {
                    new PlayerStats { Name = "Player1", Kills = 0, Deaths = 0 },
                    new PlayerStats { Name = "Player2", Kills = 0, Deaths = 0 },
                };
                conn.Execute(
                    "INSERT INTO player_stats (name, kills, deaths) VALUES (@Name, @Kills, @Deaths)",
                    newPlayers);
            }
        }
    }
}
