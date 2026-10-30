using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MathTrainer.Model;

namespace MathTrainer.Persistence
{
    public class RewardRequestPersistenceService
    {
        private readonly string _filePath;

        public RewardRequestPersistenceService(string fileName = "requests.json")
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appSpecificFolder = Path.Combine(appDataPath, "MathTrainer");

            if (!Directory.Exists(appSpecificFolder))
            {
                Directory.CreateDirectory(appSpecificFolder);
            }

            _filePath = Path.Combine(appSpecificFolder, fileName);
        }

        public List<RewardRequest> LoadRequests()
        {
            if (!File.Exists(_filePath))
            {
                return new List<RewardRequest>();
            }

            try
            {
                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<RewardRequest>>(json) ?? new List<RewardRequest>();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Fehler beim Belohnungsladen (Requests): {ex.Message}");
                return new List<RewardRequest>();
            }
        }

        public void SaveRequests(List<RewardRequest> request)
        {
            try
            {
                string json = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Fehler beim Belohnungspeichern (Requests): {ex.Message}");
            }
        }

        public void AddRequests(RewardRequest request)
        {
            List<RewardRequest> requests = LoadRequests();
            requests.Add(request);
            SaveRequests(requests);
        }

        public RewardRequest? GetRequestsById(Guid id)
        {
            List<RewardRequest> requests = LoadRequests();
            return requests.FirstOrDefault(r => r.Id == id);
        }

        public void UpdateRequests(RewardRequest updatedRequest)
        {
            List<RewardRequest> requests = LoadRequests();
            int index = requests.FindIndex(r => r.Id == updatedRequest.Id);
            if (index != -1)
            {
                requests[index] = updatedRequest;
                SaveRequests(requests);
            }
            else
            {
                System.Console.WriteLine($"Request mit ID {updatedRequest.Id} wurde nicht gefunden");
            }
        }

        public void DeleteRequests(Guid id)
        {
            List<RewardRequest> requests = LoadRequests();
            requests.RemoveAll(r => r.Id == id);
            SaveRequests(requests);
        }
    }
}