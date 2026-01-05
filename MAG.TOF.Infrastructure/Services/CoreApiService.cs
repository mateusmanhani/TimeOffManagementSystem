using MAG.TOF.Application.DTOs;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Application.Mapping;
using MAG.TOF.Application.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace MAG.TOF.Infrastructure.Services
{
    public class CoreApiService : ICoreApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CoreApiService> _logger;

        public CoreApiService(HttpClient httpClient, ILogger<CoreApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<UserDto>> GetUsersAsync()
        {
            _logger.LogInformation("Fetching users from Core API.");

            try
            {
                //  Call CoreApi read json response and deserialise into List<CoreApiUser>
                var users = await _httpClient.GetFromJsonAsync<List<CoreApiUser>>("api/users");

                // Handle null respnse
                if (users == null)
                {
                    _logger.LogWarning("CORE API returned null for users");
                    return new List<UserDto>(); // return empty list intead of null
                }

                _logger.LogInformation("Successfully fetched {Count} users from CORE API", users.Count);

                // Map User list to UserDto list and return it
                return users.ToDtoList();
            }
            catch (HttpRequestException ex)
            {
                // Handle network errors (server down, timeout, etc.)
                _logger.LogError(ex, "An error ocurred while fetching users from Core API.");
                throw; // re-throw so caller can handle it
            }
            catch (Exception ex)
            {
                // Handle unexpected erros (JSON parsing errors)
                _logger.LogError(ex, "Unexpected error while fetching users from CORE API");
                throw;
            }
        }

        public async Task<List<DepartmentDto>> GetDepartmentsAsync()
        {
            try
            {
                // Call Core API, fetch and deserialise json to List<Department>
                var departments = await _httpClient.GetFromJsonAsync<List<DepartmentDto>>("api/departments");

                // Handle null result
                if (departments == null)
                {
                    _logger.LogWarning("CORE API returned null from departments");
                    return new List<DepartmentDto>(); // return empty list
                }

               _logger.LogInformation("Successfully retrieved {Count} department records", departments.Count);
                return departments;

            }
            catch (HttpRequestException ex)
            {
                // Handle network errors (server down, timeout, etc.)
                _logger.LogError(ex, "An error ocurred while fetching departments from Core API.");
                throw; // re-throw so caller can handle it
            }
            catch (Exception ex)
            {
                // Handle network errors (server down, timeout, etc.)
                _logger.LogError(ex, "Unexpcted error while fetching departments from Core API.");
                throw; // re-throw so caller can handle it
            }
        }

        public async Task<List<GradeDto>> GetGradesAsync()
        {
            try
            {
                // Call Core API, fetch and deserialise json to List<Department>
                var grades = await _httpClient.GetFromJsonAsync<List<GradeDto>>("api/grades");

                // Handle null result
                if (grades == null)
                {
                    _logger.LogWarning("CORE API returned null from grades");
                    return new List<GradeDto>(); // return empty list
                }

                _logger.LogInformation("Successfully retrieved {Count} grade records", grades.Count);
                return grades;

            }
            catch (HttpRequestException ex)
            {
                // Handle network errors (server down, timeout, etc.)
                _logger.LogError(ex, "An error ocurred while fetching grades from Core API.");
                throw; // re-throw so caller can handle it
            }
            catch (Exception ex)
            {
                // Handle network errors (server down, timeout, etc.)
                _logger.LogError(ex, "Unexpcted error while fetching grades from Core API.");
                throw; // re-throw so caller can handle it
            }
        }
    }
}
