﻿using System.Configuration;
using System.Globalization;
using Microsoft.Data.Sqlite;

namespace SinghxRaj.CodingTracker;

internal class DatabaseManager
{
    private static string ConnectionString = ConfigurationManager.AppSettings.Get("ConnectionString")!;
    private const int SUCCESSFULLY_ADDED_ROW = 1;

    public static void CreateTable()
    {
        string createTable = @"CREATE TABLE IF NOT EXISTS CODING_TRACKER (
                                      Id INTEGER PRIMARY KEY AUTO INCREMENT
                                      Start DATEIME,
                                      End DATETIME,
                                      Duration INTEGER";

        using var connection = new SqliteConnection(ConnectionString);
        using var command = connection.CreateCommand();
        command.CommandText = createTable;
        command.ExecuteNonQuery();
    }

    public static bool AddNewCodingSession(CodingSession session)
    {
        int rowsAdded;

        string start = session.StartTime.ToString(TimeFormat.SessionTimeStampFormat);
        string end = session.EndTime.ToString(TimeFormat.SessionTimeStampFormat);
        int durationInMinutes = (int)session.Duration.TotalMinutes;

        string newSession = @$"INSERT INTO CODING_TRACKER (Start, End, Duration)
                                  VALUES ({ start }, { end }, { durationInMinutes }";

        using var connection = new SqliteConnection(ConnectionString);
        using var command = connection.CreateCommand();
        command.CommandText = newSession;
        rowsAdded = command.ExecuteNonQuery();
        return rowsAdded == SUCCESSFULLY_ADDED_ROW;
    }

    public static List<CodingSession> GetCodingSessions()
    {
        string getSessions = @"SELECT * FROM CODING_TRACKER;";
        var sessions = new List<CodingSession>();

        using var connection = new SqliteConnection(ConnectionString);
        using var command = connection.CreateCommand();
        command.CommandText = getSessions;
        var reader = command.ExecuteReader();

        while (reader.Read())
        {
            int id = reader.GetInt32(0);
            string startStr = reader.GetString(1);
            string endStr = reader.GetString(2);
            int durationInMinutes = reader.GetInt32(3);
            TimeSpan duration = TimeSpan.FromMinutes(durationInMinutes);

            bool parseStart = DateTime.TryParseExact(startStr, TimeFormat.SessionTimeStampFormat,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime start);

            bool parseEnd =  DateTime.TryParseExact(startStr, TimeFormat.SessionTimeStampFormat,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime end);

            if (parseStart && parseEnd)
            {
                sessions.Add(new CodingSession(id, start, end, duration));
            }
        }
        return sessions;
    }
}
