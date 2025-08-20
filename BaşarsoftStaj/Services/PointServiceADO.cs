using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Interfaces;
using BaşarsoftStaj.Models;
using Npgsql;
using System.Data;
using System.Text.RegularExpressions;

namespace BaşarsoftStaj.Services;

public class PointServiceADO : IPointService
{
    private readonly string _connectionString;
    private readonly string _masterConnectionString;
    
    public PointServiceADO(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        var builder = new NpgsqlConnectionStringBuilder(_connectionString);
        var databaseName = builder.Database;
        builder.Database = "postgres";
        _masterConnectionString = builder.ConnectionString;
        
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

    public ApiResponse<List<Point>> GetAllPoints()
    {
        try
        {
            var points = new List<Point>();
            
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            
            var command = new NpgsqlCommand("SELECT Id, Name, WKT FROM Points ORDER BY Id", connection);
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                points.Add(new Point
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    WKT = reader.GetString("WKT")
                });
            }
            
            return ApiResponse<List<Point>>.SuccessResponse(points, "PointsRetrievedSuccessfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<Point>>.ErrorResponse($"DatabaseError: {ex.Message}");
        }
    }

    public ApiResponse<Point> GetPointById(int id)
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
                var point = new Point
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    WKT = reader.GetString("WKT")
                };
                
                return ApiResponse<Point>.SuccessResponse(point, "PointRetrievedSuccessfully");
            }
            
            return ApiResponse<Point>.ErrorResponse("PointNotFound");
        }
        catch (Exception ex)
        {
            return ApiResponse<Point>.ErrorResponse($"DatabaseError: {ex.Message}");
        }
    }

    public ApiResponse<Point> AddPoint(AddPointDto pointDto)
    {
        if (pointDto == null || string.IsNullOrEmpty(pointDto.Name) || string.IsNullOrEmpty(pointDto.WKT))
        {
            return ApiResponse<Point>.ErrorResponse("ValidationError");
        }

        if (!IsValidWkt(pointDto.WKT))
        {
            return ApiResponse<Point>.ErrorResponse("InvalidWktFormat");
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
                var point = new Point
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    WKT = reader.GetString("WKT")
                };
                
                return ApiResponse<Point>.SuccessResponse(point, "PointAddedSuccessfully");
            }
            
            return ApiResponse<Point>.ErrorResponse("FailedToAddPoint");
        }
        catch (Exception ex)
        {
            return ApiResponse<Point>.ErrorResponse($"DatabaseError: {ex.Message}");
        }
    }

    public ApiResponse<List<Point>> AddRangePoints(List<AddPointDto> pointDtos)
    {
        if (pointDtos == null || !pointDtos.Any())
        {
            return ApiResponse<List<Point>>.ErrorResponse("InvalidInput");
        }

        // Validate all points before processing
        foreach (var pointDto in pointDtos)
        {
            if (pointDto == null || string.IsNullOrEmpty(pointDto.Name) || string.IsNullOrEmpty(pointDto.WKT) || !IsValidWkt(pointDto.WKT))
            {
                return ApiResponse<List<Point>>.ErrorResponse("ValidationError");
            }
        }

        try
        {
            var addedPoints = new List<Point>();
            
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
                        addedPoints.Add(new Point
                        {
                            Id = reader.GetInt32("Id"),
                            Name = reader.GetString("Name"),
                            WKT = reader.GetString("WKT")
                        });
                    }
                    
                    reader.Close();
                }
                
                transaction.Commit();
                return ApiResponse<List<Point>>.SuccessResponse(addedPoints, "PointsAddedSuccessfully");
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            return ApiResponse<List<Point>>.ErrorResponse($"DatabaseError: {ex.Message}");
        }
    }

    public ApiResponse<Point> UpdatePoint(int id, string newName, string newWkt)
    {
        if (string.IsNullOrEmpty(newName) && string.IsNullOrEmpty(newWkt))
        {
            return ApiResponse<Point>.ErrorResponse("ValidationError");
        }

        if (!string.IsNullOrEmpty(newWkt) && !IsValidWkt(newWkt))
        {
            return ApiResponse<Point>.ErrorResponse("InvalidWktFormat");
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
                return ApiResponse<Point>.ErrorResponse("PointNotFound");
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
                var point = new Point
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    WKT = reader.GetString("WKT")
                };
                
                return ApiResponse<Point>.SuccessResponse(point, "PointUpdatedSuccessfully");
            }
            
            return ApiResponse<Point>.ErrorResponse("PointNotFound");
        }
        catch (Exception ex)
        {
            return ApiResponse<Point>.ErrorResponse($"DatabaseError: {ex.Message}");
        }
    }

    public ApiResponse<Point> DeletePoint(int id)
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
                var point = new Point
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    WKT = reader.GetString("WKT")
                };
                
                return ApiResponse<Point>.SuccessResponse(point, "PointDeletedSuccessfully");
            }
            
            return ApiResponse<Point>.ErrorResponse("PointNotFound");
        }
        catch (Exception ex)
        {
            return ApiResponse<Point>.ErrorResponse($"DatabaseError: {ex.Message}");
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