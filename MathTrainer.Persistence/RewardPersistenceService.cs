using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MathTrainer.Model;

namespace MathTrainer.Persistence
{
    public class RewardPersistenceService
    {
        private readonly string _filePath;

        public RewardPersistenceService(string fileName = "rewards.json")
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appSpecificFolder = Path.Combine(appDataPath, "MathTrainer");

            if (!Directory.Exists(appSpecificFolder))
            {
                Directory.CreateDirectory(appSpecificFolder);
            }

            _filePath = Path.Combine(appSpecificFolder, fileName);
        }

        public List<Reward> LoadRewards()
        {
            if (!File.Exists(_filePath))
            {
                return new List<Reward>();
            }

            try
            {
                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<Reward>>(json) ?? new List<Reward>();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Fehler beim Belohnungsladen: {ex.Message}");
                return new List<Reward>();
            }
        }

        public void SaveReward(List<Reward> rewards)
        {
            try
            {
                string json = JsonSerializer.Serialize(rewards, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Fehler beim Belohnungspeichern: {ex.Message}");
            }
        }

        public void AddReward(Reward reward)
        {
            List<Reward> rewards = LoadRewards();
            rewards.Add(reward);
            SaveReward(rewards);
        }

        public Reward? GetRewardById(Guid id)
        {
            List<Reward> rewards = LoadRewards();
            return rewards.FirstOrDefault(r => r.Id == id);
        }

        public void UpdateReward(Reward updatedReward)
        {
            List<Reward> rewards = LoadRewards();
            int index = rewards.FindIndex(r => r.Id == updatedReward.Id);
            if (index != -1)
            {
                rewards[index] = updatedReward;
                SaveReward(rewards);
            }
            else
            {
                System.Console.WriteLine($"Belohnung mit ID {updatedReward.Id} wurde nicht gefunden");
            }
        }

        public void DeleteReward(Guid id)
        {
            List<Reward> rewards = LoadRewards();
            rewards.RemoveAll(r => r.Id == id);
            SaveReward(rewards);
        }
    }
}