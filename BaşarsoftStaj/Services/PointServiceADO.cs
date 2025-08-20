using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Interfaces;
using BaşarsoftStaj.Models;
using Npgsql;
using System.Data;
using System.Text.RegularExpressions;
using NetTopologySuite.IO;

namespace BaşarsoftStaj.Services;

public class PointServiceADO : IPointService
{
    private readonly string _connectionString;
    private readonly string _masterConnectionString;
    private readonly WKTReader _wktReader;
    
    public PointServiceADO(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        var builder = new NpgsqlConnectionStringBuilder(_connectionString);
        var databaseName = builder.Database;
        builder.Database = "postgres";
        _masterConnectionString = builder.ConnectionString;
        _wktReader = new WKTReader();
        
        InitializeDatabase(databaseName);
    }
    
    private void InitializeDatabase(string databaseName)
    {
        try
        {
           
            using (var masterConnection = new NpgsqlConnection(_masterConnectionString))
            {
                masterConnection.Open();
                
             
                var checkDbCommand = new NpgsqlCommand(
                    "SELECT 1 FROM pg_database WHERE datname = @dbname", masterConnection);
                checkDbCommand.Parameters.AddWithValue("@dbname", databaseName);
                
                var dbExists = checkDbCommand.ExecuteScalar() != null;
                
                if (!dbExists)
                {
                    
                    var createDbCommand = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", masterConnection);
                    createDbCommand.ExecuteNonQuery();
                }
            }
            
            
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                
                var createTableCommand = new NpgsqlCommand(@"
                    CREATE TABLE IF NOT EXISTS Points (
                        Id SERIAL PRIMARY KEY,
                        Name VARCHAR(100) NOT NULL,
                        WKT TEXT NOT NULL
                    )", connection);
                
                createTableCommand.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize database: {ex.Message}", ex);
        }
    }

    public ApiResponse<List<PointE>> GetAllPoints()
    {
        try
        {
            var points = new List<PointE>();
            
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            
            var command = new NpgsqlCommand("SELECT Id, Name, WKT FROM Points ORDER BY Id", connection);
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                var point = new PointE
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    WKT = reader.GetString("WKT") // Geometry conversion happens automatically
                };
                points.Add(point);
            }
            
            return ApiResponse<List<PointE>>.SuccessResponse(points, "PointsRetrievedSuccessfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PointE>>.ErrorResponse($"DatabaseError: {ex.Message}");
        }
    }

    public ApiResponse<PointE> GetPointById(int id)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            
            var command = new NpgsqlCommand("SELECT Id, Name, WKT FROM Points WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var point = new PointE
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    WKT = reader.GetString("WKT") // Geometry conversion happens automatically
                };
                
                return ApiResponse<PointE>.SuccessResponse(point, "PointRetrievedSuccessfully");
            }
            
            return ApiResponse<PointE>.ErrorResponse("PointNotFound");
        }
        catch (Exception ex)
        {
            return ApiResponse<PointE>.ErrorResponse($"DatabaseError: {ex.Message}");
        }
    }

    public ApiResponse<PointE> AddPoint(AddPointDto pointDto)
    {
        if (pointDto == null || string.IsNullOrEmpty(pointDto.Name) || string.IsNullOrEmpty(pointDto.WKT))
        {
            return ApiResponse<PointE>.ErrorResponse("ValidationError");
        }

        if (!IsValidWkt(pointDto.WKT))
        {
            return ApiResponse<PointE>.ErrorResponse("InvalidWktFormat");
        }

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            
            var command = new NpgsqlCommand(@"
                INSERT INTO Points (Name, WKT) 
                VALUES (@Name, @WKT) 
                RETURNING Id, Name, WKT", connection);
            
            command.Parameters.AddWithValue("@Name", pointDto.Name);
            command.Parameters.AddWithValue("@WKT", pointDto.WKT);
            
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var point = new PointE
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    WKT = reader.GetString("WKT") // Geometry conversion happens automatically
                };
                
                return ApiResponse<PointE>.SuccessResponse(point, "PointAddedSuccessfully");
            }
            
            return ApiResponse<PointE>.ErrorResponse("FailedToAddPoint");
        }
        catch (Exception ex)
        {
            return ApiResponse<PointE>.ErrorResponse($"DatabaseError: {ex.Message}");
        }
    }

    public ApiResponse<List<PointE>> AddRangePoints(List<AddPointDto> pointDtos)
    {
        if (pointDtos == null || !pointDtos.Any())
        {
            return ApiResponse<List<PointE>>.ErrorResponse("InvalidInput");
        }

        // Validate all points before processing
        foreach (var pointDto in pointDtos)
        {
            if (pointDto == null || string.IsNullOrEmpty(pointDto.Name) || string.IsNullOrEmpty(pointDto.WKT) || !IsValidWkt(pointDto.WKT))
            {
                return ApiResponse<List<PointE>>.ErrorResponse("ValidationError");
            }
        }

        try
        {
            var addedPoints = new List<PointE>();
            
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            
            using var transaction = connection.BeginTransaction();
            
            try
            {
                foreach (var pointDto in pointDtos)
                {
                    var command = new NpgsqlCommand(@"
                        INSERT INTO Points (Name, WKT) 
                        VALUES (@Name, @WKT) 
                        RETURNING Id, Name, WKT", connection, transaction);
                    
                    command.Parameters.AddWithValue("@Name", pointDto.Name);
                    command.Parameters.AddWithValue("@WKT", pointDto.WKT);
                    
                    using var reader = command.ExecuteReader();
                    
                    if (reader.Read())
                    {
                        var point = new PointE
                        {
                            Id = reader.GetInt32("Id"),
                            Name = reader.GetString("Name"),
                            WKT = reader.GetString("WKT") // Geometry conversion happens automatically
                        };
                        addedPoints.Add(point);
                    }
                    
                    reader.Close();
                }
                
                transaction.Commit();
                return ApiResponse<List<PointE>>.SuccessResponse(addedPoints, "PointsAddedSuccessfully");
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PointE>>.ErrorResponse($"DatabaseError: {ex.Message}");
        }
    }

    public ApiResponse<PointE> UpdatePoint(int id, string newName, string newWkt)
    {
        if (string.IsNullOrEmpty(newName) && string.IsNullOrEmpty(newWkt))
        {
            return ApiResponse<PointE>.ErrorResponse("ValidationError");
        }

        if (!string.IsNullOrEmpty(newWkt) && !IsValidWkt(newWkt))
        {
            return ApiResponse<PointE>.ErrorResponse("InvalidWktFormat");
        }

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            
            // First check if the point exists
            var checkCommand = new NpgsqlCommand("SELECT COUNT(*) FROM Points WHERE Id = @Id", connection);
            checkCommand.Parameters.AddWithValue("@Id", id);
            var exists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;
            
            if (!exists)
            {
                return ApiResponse<PointE>.ErrorResponse("PointNotFound");
            }

            // Build dynamic update query
            var setParts = new List<string>();
            var command = new NpgsqlCommand();
            command.Connection = connection;
            command.Parameters.AddWithValue("@Id", id);

            if (!string.IsNullOrEmpty(newName))
            {
                setParts.Add("Name = @Name");
                command.Parameters.AddWithValue("@Name", newName);
            }

            if (!string.IsNullOrEmpty(newWkt))
            {
                setParts.Add("WKT = @WKT");
                command.Parameters.AddWithValue("@WKT", newWkt);
            }

            command.CommandText = $@"
                UPDATE Points 
                SET {string.Join(", ", setParts)}
                WHERE Id = @Id 
                RETURNING Id, Name, WKT";
            
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var point = new PointE
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    WKT = reader.GetString("WKT") // Geometry conversion happens automatically
                };
                
                return ApiResponse<PointE>.SuccessResponse(point, "PointUpdatedSuccessfully");
            }
            
            return ApiResponse<PointE>.ErrorResponse("PointNotFound");
        }
        catch (Exception ex)
        {
            return ApiResponse<PointE>.ErrorResponse($"DatabaseError: {ex.Message}");
        }
    }

    public ApiResponse<PointE> DeletePoint(int id)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            
            var command = new NpgsqlCommand(@"
                DELETE FROM Points 
                WHERE Id = @Id 
                RETURNING Id, Name, WKT", connection);
            
            command.Parameters.AddWithValue("@Id", id);
            
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var point = new PointE
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    WKT = reader.GetString("WKT") // Geometry conversion happens automatically
                };
                
                return ApiResponse<PointE>.SuccessResponse(point, "PointDeletedSuccessfully");
            }
            
            return ApiResponse<PointE>.ErrorResponse("PointNotFound");
        }
        catch (Exception ex)
        {
            return ApiResponse<PointE>.ErrorResponse($"DatabaseError: {ex.Message}");
        }
    }

    private bool IsValidWkt(string wkt)
    {
        if (string.IsNullOrEmpty(wkt))
            return false;
        
        var patterns = new[]
        {
            @"^POINT\s*\(\s*-?\d+(\.\d+)?\s+-?\d+(\.\d+)?\s*\)$",
            @"^LINESTRING\s*\(\s*(-?\d+(\.\d+)?\s+-?\d+(\.\d+)?\s*,?\s*)+\)$",
            @"^POLYGON\s*\(\s*\(\s*(-?\d+(\.\d+)?\s+-?\d+(\.\d+)?\s*,?\s*)+\)\s*\)$"
        };

        return patterns.Any(pattern => Regex.IsMatch(wkt.Trim(), pattern, RegexOptions.IgnoreCase));
    }
}