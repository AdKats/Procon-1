/*
 * SdkTemplatePlugin — Database Operations (Partial Class)
 *
 * Example of MySQL operations using MySqlConnector.
 * Only loaded if the connection string is configured.
 */

using System;
using MySqlConnector;

namespace PRoConEvents
{
    public partial class SdkTemplatePlugin
    {
        private string _dbConnectionString = "";

        /// <summary>
        /// Example: look up player stats from a database.
        /// Returns null if DB is not configured or query fails.
        /// </summary>
        private string GetPlayerStats(string soldierName)
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
                                return string.Format("{0}: {1} kills, {2} deaths, {3:F2} K/D",
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
    }
}
